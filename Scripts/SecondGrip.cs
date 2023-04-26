
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
    [RequireComponent(typeof(VRC.SDK3.Components.VRCPickup)), RequireComponent(typeof(SmartObjectSync))]//slightly after everything else
    public class SecondGrip : SmartObjectSyncListener
    {
        [System.NonSerialized] public Vector3 localSyncOffset;
        public SmartObjectSync sync;
        public SmartObjectSync parentSync;
        public Transform constrainedObject;
        [Tooltip("Forces the gun to point directly forward no matter how you're holding the second grip")]
        public bool lockAim = false;
        public float horizontalLeewayWhileLocked = 0.5f;
        public Transform stabilizationPoint;
        [System.NonSerialized]
        public Vector3 restPos;
        [System.NonSerialized]
        public Quaternion restRot;
        [System.NonSerialized]
        public Vector3 startPos;
        [System.NonSerialized]
        public Quaternion startRot;
        public override void OnChangeState(SmartObjectSync s, int oldState, int newState)
        {
            if (!Utilities.IsValid(sync))
            {
                return;
            }
            if (s == sync)
            {
                enabled = sync.IsHeld();
                if (!enabled)
                {
                    sync.transform.localPosition = restPos;
                    sync.transform.localRotation = restRot;
                    constrainedObject.localPosition = startPos;
                    constrainedObject.localRotation = startRot;
                }
            } else if (s == parentSync)
            {
                sync.pickup.pickupable = parentSync.pickup.IsHeld;
                sync.pickup.Drop();
            }
        }

        public override void OnChangeOwner(SmartObjectSync s, VRCPlayerApi oldPlayer, VRCPlayerApi newPlayer)
        {
            // if (s == parentSync)
            // {
            //     Networking.SetOwner(newPlayer, gameObject);
            // }
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset()
        {
            SerializedObject serialized = new SerializedObject(this);
            serialized.FindProperty("sync").objectReferenceValue = GetComponent<SmartObjectSync>();
            serialized.FindProperty("parentSync").objectReferenceValue = transform.parent.GetComponent<SmartObjectSync>();
            serialized.ApplyModifiedProperties();
        }
#endif
        private VRCPlayerApi _localPlayer;

        void Start()
        {
            _localPlayer = Networking.LocalPlayer;
            if (!Utilities.IsValid(parentSync))
            {
                parentSync = transform.parent.GetComponent<SmartObjectSync>();
            }
            if (!Utilities.IsValid(sync))
            {
                sync = GetComponent<SmartObjectSync>();
            }
            sync.rigid.isKinematic = true;
            parentSync.AddListener(this);
            sync.AddListener(this);
            startPos = constrainedObject.localPosition;
            startRot = constrainedObject.localRotation;
            restPos = sync.transform.localPosition;
            restRot = sync.transform.localRotation;
            startOffset = Quaternion.Inverse(parentSync.transform.rotation) * (sync.transform.position - GetGrabPos(parentSync));
            enabled = sync.IsHeld();
            sync.pickup.pickupable = (parentSync.IsHeld() && !sync.IsHeld());
        }

        public Vector3 GetGrabPos(SmartObjectSync pickup)
        {
            if (pickup.pickup.orientation == VRC_Pickup.PickupOrientation.Gun)
            {
                if (Utilities.IsValid(pickup.pickup.ExactGun))
                {
                    return pickup.pickup.ExactGun.position;
                }
            } else if (pickup.pickup.orientation == VRC_Pickup.PickupOrientation.Grip && Utilities.IsValid(pickup.pickup.ExactGrip))
            {
                if (Utilities.IsValid(pickup.pickup.ExactGrip))
                {
                    return pickup.pickup.ExactGrip.position;
                }
            } else if (Utilities.IsValid(pickup.owner))
            {
                if (pickup.state == SmartObjectSync.STATE_LEFT_HAND_HELD)
                {
                    Vector3 handPos = pickup.owner.GetBonePosition(HumanBodyBones.LeftMiddleProximal);//base of middle finger
                    if (handPos != Vector3.zero)
                    {
                        return handPos;
                    }
                    handPos = pickup.owner.GetBonePosition(HumanBodyBones.LeftHand);
                    if (handPos != Vector3.zero)
                    {
                        return handPos;
                    }
                } else if (pickup.state == SmartObjectSync.STATE_RIGHT_HAND_HELD)
                {
                    Vector3 handPos = pickup.owner.GetBonePosition(HumanBodyBones.RightMiddleProximal);
                    if (handPos != Vector3.zero)
                    {
                        return handPos;
                    }
                    handPos = pickup.owner.GetBonePosition(HumanBodyBones.RightHand);
                    if (handPos != Vector3.zero)
                    {
                        return handPos;
                    }
                }
            }
            return pickup.transform.position;
        }
        Vector3 startOffset;
        Vector3 newLocalOffset;
        Vector3 worldOffset;
        Vector3 newWorldOffset;
        Quaternion adjustmentRotation;
        Vector3 axis;
        float angle;
        public override void PostLateUpdate()
        {
            if (!Utilities.IsValid(sync) || !sync.IsHeld())
            {
                return;
            }
            if (lockAim)
            {
                AlignToView();
            } else
            {
                AlignToGrip();
            }
        }

        // public override void OnPreSerialization()
        // {
        //     if (!sync.IsHeld())
        //     {
        //         return;
        //     }
        //     sync.pos = sync.startPos;
        //     sync.generic_Interpolate(1.0f);
        // }
        // public override void OnPostSerialization(VRC.Udon.Common.SerializationResult result)
        // {
        //     if (!sync.IsHeld())
        //     {
        //         return;
        //     }
        //     if (sync.pos != sync.startPos)
        //     {
        //         RequestSerialization();
        //     }
        //     sync.pos = sync.startPos;
        //     sync.generic_Interpolate(1.0f);
        // }

        Vector3 stabilizationRotationPoint;
        Vector3 upVector;
        Vector3 forwardVector;
        Vector3 squashedForwardVector;
        public void AlignToView()
        {
            if (!Utilities.IsValid(sync.owner))
            {
                return;
            }
            constrainedObject.localPosition = startPos;
            constrainedObject.localRotation = startRot;
            forwardVector = constrainedObject.rotation * Vector3.forward;
            if (Utilities.IsValid(stabilizationPoint))
            {
                stabilizationRotationPoint = Vector3.Lerp(stabilizationPoint.position, Vector3.Project(GetGrabPos(parentSync) - stabilizationPoint.position, stabilizationPoint.rotation * Vector3.forward) + stabilizationPoint.position, 0.5f);
                adjustmentRotation = Quaternion.FromToRotation(stabilizationPoint.rotation * Vector3.forward, stabilizationRotationPoint - sync.owner.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position);
            } else
            {
                stabilizationRotationPoint = GetGrabPos(parentSync);
                adjustmentRotation = Quaternion.FromToRotation(constrainedObject.rotation * Vector3.forward, constrainedObject.position - sync.owner.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position);
            }
            adjustmentRotation.ToAngleAxis(out angle, out axis);
            constrainedObject.RotateAround(stabilizationRotationPoint, axis, angle);
            if (horizontalLeewayWhileLocked > 0)
            {
                upVector = Vector3.up;
                squashedForwardVector = Vector3.ProjectOnPlane(forwardVector, upVector);
                constrainedObject.RotateAround(stabilizationRotationPoint, upVector, -Vector3.SignedAngle(squashedForwardVector, constrainedObject.rotation * Vector3.forward, upVector) * horizontalLeewayWhileLocked);
            }
        }

        public void AlignToGrip()
        {
            if (sync.IsLocalOwner())
            {
                if (sync.state == SmartObjectSync.STATE_NO_HAND_HELD)
                {
                    sync.noHand_CalcParentTransform();
                } else
                {
                    sync.bone_CalcParentTransform();
                }
                sync.generic_Interpolate(1.0f);
            }
            newLocalOffset = Quaternion.Inverse(parentSync.transform.rotation) * (sync.transform.position - GetGrabPos(parentSync));
            worldOffset = parentSync.transform.rotation * startOffset;
            newWorldOffset = parentSync.transform.rotation * newLocalOffset;
            adjustmentRotation = Quaternion.FromToRotation(worldOffset, newWorldOffset);
            adjustmentRotation.ToAngleAxis(out angle, out axis);
            constrainedObject.localPosition = startPos;
            constrainedObject.localRotation = startRot;
            constrainedObject.RotateAround(GetGrabPos(parentSync), axis, angle);
        }
        public Vector3 CalcGripPosFromBone(SmartObjectSync gripSync, HumanBodyBones bone)
        {
            if (Utilities.IsValid(gripSync.owner))
            {
                return gripSync.owner.GetBonePosition(bone) + gripSync.owner.GetBoneRotation(bone) * gripSync.startPos;
            }
            return gripSync.transform.position;
        }
        public Vector3 CalcGripPosFromOwner(SmartObjectSync gripSync)
        {
            if (Utilities.IsValid(gripSync.owner))
            {
                return gripSync.owner.GetPosition() + gripSync.owner.GetRotation() * gripSync.startPos;
            }
            return gripSync.transform.position;
        }
    }
}