
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor;
using UdonSharpEditor;
using System.Collections.Immutable;
#endif

namespace MMMaellon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual), RequireComponent(typeof(P_Shooter))]
    public class Scope : UdonSharpBehaviour
    {
        [System.NonSerialized]
        public P_Shooter shooter;
        public GameObject scopeCam;
        public Transform scopeAnchor;
        public float zoomInTime = 0.25f;
        public float zoomOutTime = 0.1f;
        VRCPlayerApi _localPlayer;

        VRC_Pickup leftPickup;
        VRC_Pickup rightPickup;
        Scope leftScope;
        Scope rightScope;
        VRCPlayerApi.TrackingData headData;

        private bool ADS = false;
        private Vector3 zoomStartPos;
        private Vector3 zoomStopPos;
        private Quaternion zoomStartRot;
        private Quaternion zoomStopRot;
        private float zoomStart;
        float lerp;
        float smoothedLerp;
        Vector3 leftPos;
        Vector3 rightPos;
        [System.NonSerialized]
        public bool _loop = false;
        public bool loop
        {
            get => _loop;
            set
            {
                if (!_loop && value)
                {
                    SendCustomEventDelayedFrames(nameof(UpdateLoop), 0);
                }
                _loop = value;
            }
        }
        void Start()
        {
            _localPlayer = Networking.LocalPlayer;
            shooter = GetComponentInParent<P_Shooter>();
            if (Utilities.IsValid(scopeCam))
            {
                scopeCam.SetActive(false);
            }
        }
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
            if (!Utilities.IsValid(scopeAnchor))
            {
                zoomStopPos = Vector3.ProjectOnPlane((transform.position - headData.position), headData.rotation * Vector3.left) + headData.position;
                zoomStopPos = transform.InverseTransformPoint(zoomStopPos);
            }
            else
            {
                zoomStopPos = Quaternion.Inverse(transform.rotation) * (headData.position - scopeAnchor.transform.position);
                zoomStopRot = scopeAnchor.transform.localRotation;
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

        public void UpdateLoop()
        {
            if (!loop)
            {
                return;
            }
            SendCustomEventDelayedFrames(nameof(UpdateLoop), 0);
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
                leftPickup = _localPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Left);
                rightPickup = _localPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Right);
                headData = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

                leftScope = Utilities.IsValid(leftPickup) ? leftScope.GetComponent<Scope>() : null;
                rightScope = Utilities.IsValid(rightPickup) ? rightScope.GetComponent<Scope>() : null;

                if (Utilities.IsValid(leftScope) && Utilities.IsValid(rightScope))
                {
                    leftPos = Utilities.IsValid(leftScope.scopeAnchor) ? leftScope.scopeAnchor.position : leftScope.transform.position;
                    rightPos = Utilities.IsValid(rightScope.scopeAnchor) ? rightScope.scopeAnchor.position : rightScope.transform.position;
                    if (leftScope == this)
                    {
                        scopeCam.SetActive(rightScope == null || Vector3.Distance(leftPos, headData.position) < Vector3.Distance(rightPos, headData.position));
                    } else if (rightScope == this)
                    {
                        scopeCam.SetActive(leftPickup == null || Vector3.Distance(leftPos, headData.position) >= Vector3.Distance(rightPos, headData.position));
                    }
                } else
                {
                    scopeCam.SetActive(true);
                }
            }

            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                ADS = true;
                recordStartZoomTransforms();
            }
            else if (Input.GetKeyUp(KeyCode.LeftAlt))
            {
                ADS = false;
                recordStartZoomTransforms();
            }

            if (Input.GetKey(KeyCode.LeftAlt))
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