
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Cinemachine;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor;
using UdonSharpEditor;
using System.Collections.Immutable;
#endif

namespace MMMaellon.P_Shooters
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual), DefaultExecutionOrder(5)]
    public class Scope : SmartObjectSyncListener
    {
        public KeyCode zoomShortcut = KeyCode.LeftAlt;
        public P_Shooter shooter;
        public GameObject scopeCam;
        
        public Transform ADSPosition;
        public float zoomInTime = 0.25f;
        public float zoomOutTime = 0.1f;
        VRCPlayerApi _localPlayer;
        VRC_Pickup otherPickup;
        VRCPlayerApi.TrackingData headData;

        [System.NonSerialized]
        public bool ADS = false;
        [System.NonSerialized]
        public Vector3 zoomStartPos;
        [System.NonSerialized]
        public Vector3 zoomStopPos;
        [System.NonSerialized]
        public Quaternion zoomStartRot;
        [System.NonSerialized]
        public Quaternion zoomStopRot;
        [System.NonSerialized]
        public float zoomStart;
        [System.NonSerialized]
        public float lerp;
        [System.NonSerialized]
        public float smoothedLerp;
        Vector3 thisPos;
        Vector3 otherPos;
        [System.NonSerialized]
        public bool _loop = false;
        public bool loop
        {
            get => _loop;
            set
            {
                if (!_loop && value)
                {
                    // SendCustomEventDelayedFrames(nameof(UpdateLoop), 0, VRC.Udon.Common.Enums.EventTiming.LateUpdate);
                    //loop just got enabled
                    if (shooter.sync.pickup.currentHand == VRC_Pickup.PickupHand.Left)
                    {
                        otherPickup = _localPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Right);
                    } else
                    {
                        otherPickup = _localPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Left);
                    }
                    if (Utilities.IsValid(otherPickup))
                    {
                        otherScope = otherPickup.GetComponentInChildren<Scope>();
                        if (Utilities.IsValid(otherScope))
                        {
                            otherScope.otherScope = this;
                        }
                    }
                    else
                    {
                        otherScope = null;
                    }
                } else if (!value)
                {
                    otherScope = null;
                }
                _loop = value;
                enabled = value;
            }
        }
        Scope otherScope;
        void Start()
        {
            _localPlayer = Networking.LocalPlayer;
            if (Utilities.IsValid(scopeCam))
            {
                scopeCam.SetActive(false);
            }
            shooter.sync.AddListener(this);
        }

        public override void OnChangeState(SmartObjectSync sync, int oldState, int newState)
        {
            loop = loop || sync.pickup.IsHeld;
        }
        public override void OnChangeOwner(SmartObjectSync sync, VRCPlayerApi oldOwner, VRCPlayerApi newOwner)
        {
            
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset()
        {
            SetupScope(this);
        }

        public static void SetupScope(Scope scope)
        {
            if (!Utilities.IsValid(scope) || (scope.shooter != null && scope.ADSPosition != null && scope.scopeCam != null))
            {
                //null or already setup
                return;
            }
            if (!Helper.IsEditable(scope))
            {
                Helper.ErrorLog(scope, "Scope is not editable");
                return;
            }
            P_Shooter parentShooter = scope.GetComponentInParent<P_Shooter>();
            if(!Utilities.IsValid(parentShooter))
            {
                Helper.ErrorLog(scope, "Scope is not a child of a P-Shooter");
                return;
            }
            SerializedObject serializedObject = new SerializedObject(scope);
            serializedObject.FindProperty("shooter").objectReferenceValue = parentShooter;
            if (scope.ADSPosition == null)
            {
                serializedObject.FindProperty("ADSPosition").objectReferenceValue = scope.transform;
            }
            if (scope.scopeCam == null)
            {
                Debug.LogWarning("scope cam not found");
                Camera cam = parentShooter.GetComponentInChildren<Camera>();
                Debug.LogWarning("cam found: " + cam != null);
                serializedObject.FindProperty("scopeCam").objectReferenceValue = cam != null ? cam.gameObject : null;
            }
            if (scope.scopeCam != null)
            {
                scope.scopeCam.SetActive(false);
            }
            serializedObject.ApplyModifiedProperties();
        }
#endif
        public override void OnPickup()
        {
            loop = true;
        }
        public void recordStartZoomTransforms()
        {
            zoomStartPos = zoomStopPos;
            zoomStartRot = zoomStopRot;
            float incompleteZoom = 0;
            float currentZoomTime = (Time.realtimeSinceStartup - zoomStart);
            if (!ADS)
            {
                if (zoomInTime == 0)
                {
                    zoomStart = Time.realtimeSinceStartup;
                    return;
                }
                incompleteZoom = 1 - (currentZoomTime / zoomInTime);
                incompleteZoom = Mathf.Lerp(0, Mathf.Lerp(0, incompleteZoom, incompleteZoom), incompleteZoom);
            }
            else
            {
                if (zoomOutTime == 0)
                {
                    zoomStart = Time.realtimeSinceStartup;
                    return;
                }
                incompleteZoom = 1 - (currentZoomTime / zoomOutTime);
                incompleteZoom = Mathf.Lerp(0, Mathf.Lerp(0, incompleteZoom, incompleteZoom), incompleteZoom);
            }
            if (incompleteZoom < 0)
            {
                zoomStart = Time.realtimeSinceStartup;
                return;
            }
            if (!ADS)
            {
                incompleteZoom *= zoomOutTime;
            }
            else
            {
                incompleteZoom *= zoomInTime;
            }
            zoomStart = Time.realtimeSinceStartup - incompleteZoom;
        }
        public void zoomIn()
        {
            VRCPlayerApi.TrackingData headData = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            if (!Utilities.IsValid(ADSPosition))
            {
                zoomStopPos = Vector3.ProjectOnPlane((shooter.transform.position - headData.position), headData.rotation * Vector3.left) + headData.position;
                zoomStopPos = shooter.transform.InverseTransformPoint(zoomStopPos);
            }
            else
            {
                zoomStopPos = Quaternion.Inverse(shooter.transform.rotation) * (headData.position - ADSPosition.transform.position);
                zoomStopRot = ADSPosition.transform.localRotation;
            }

            if (zoomInTime <= 0)
            {
                shooter.gunParent.localPosition = zoomStopPos;
                shooter.gunParent.localRotation = zoomStopRot;
            }
            else
            {
                float lerp = (Time.realtimeSinceStartup - zoomStart) / zoomInTime;
                float smoothedLerp = Mathf.Lerp(lerp, 1, lerp);
                shooter.gunParent.localPosition = Vector3.Lerp(zoomStartPos, zoomStopPos, smoothedLerp);
                shooter.gunParent.localRotation = Quaternion.Lerp(zoomStartRot, zoomStopRot, smoothedLerp);
            }
        }

        public void zoomOut()
        {
            zoomStopPos = Vector3.zero;
            zoomStopRot = Quaternion.identity;

            if (zoomOutTime <= 0)
            {
                shooter.gunParent.localPosition = zoomStopPos;
                shooter.gunParent.localRotation = zoomStopRot;
            }
            else
            {
                lerp = (Time.realtimeSinceStartup - zoomStart) / zoomOutTime;
                smoothedLerp = Mathf.Lerp(lerp, 1, lerp);
                shooter.gunParent.localPosition = Vector3.Lerp(zoomStartPos, zoomStopPos, smoothedLerp);
                shooter.gunParent.localRotation = Quaternion.Lerp(zoomStartRot, zoomStopRot, smoothedLerp);
            }
        }
        bool nextADS;
        public override void PostLateUpdate()
        {
            if (!loop)
            {
                return;
            }
            // SendCustomEventDelayedFrames(nameof(UpdateLoop), 0, VRC.Udon.Common.Enums.EventTiming.LateUpdate);
            if (!Utilities.IsValid(shooter) || !Utilities.IsValid(_localPlayer))
            {
                return;
            }
            if (!shooter.sync.pickup.IsHeld)
            {
                if (ADS)
                {
                    ADS = false;
                    recordStartZoomTransforms();
                    zoomOut();
                }
                else if (Time.realtimeSinceStartup - zoomStart < zoomOutTime)
                {
                    zoomOut();
                } else
                {
                    loop = false;
                    if (Utilities.IsValid(scopeCam))
                    {
                        scopeCam.SetActive(false);
                    }
                }
                return;
            }

            if (Utilities.IsValid(scopeCam))
            {
                headData = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

                if (Utilities.IsValid(otherScope))
                {
                    thisPos = Utilities.IsValid(ADSPosition) ? ADSPosition.position : transform.position;
                    otherPos = Utilities.IsValid(otherScope.ADSPosition) ? otherScope.ADSPosition.position : otherScope.transform.position;
                    scopeCam.SetActive(Vector3.Distance(thisPos, headData.position) < Vector3.Distance(otherPos, headData.position));
                } else
                {
                    scopeCam.SetActive(true);
                }
            }

            nextADS = Input.GetKey(zoomShortcut) && shooter.state != P_Shooter.STATE_RELOAD;
            if (nextADS != ADS)
            {
                recordStartZoomTransforms();
            }
            ADS = nextADS;

            if (ADS)
            {
                zoomIn();
            }
            else
            {
                zoomOut();
            }
        }
    }

}