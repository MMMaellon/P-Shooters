
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    public class PickupConstraint : SmartObjectSyncListener
    {
        public SmartObjectSync controllerObject;
        public bool enableOnPickupController;
        public bool disableOnDropController;
        public Transform constrainedObject;
        public UdonBehaviour targetUdon;
        public bool dropOnHitLimit = false;
        public string hitMinEvent = "";
        public string hitMaxEvent = "";
        public int constraint = 0;

        public const int CONSTRAINT_MOVE_X = 0;
        public const int CONSTRAINT_MOVE_Y = 1;
        public const int CONSTRAINT_MOVE_Z = 2;
        public const int CONSTRAINT_ROTATE_X = 3;
        public const int CONSTRAINT_ROTATE_Y = 4;
        public const int CONSTRAINT_ROTATE_Z = 5;
        public float min = 0f;
        public float max = 0.25f;

        private float distance;

        Vector3 startPos;
        Quaternion startRot;
        public MultiStageConstraint multi;
        public bool nextMultiStageOnMin = false;
        public bool nextMultiZtageOnMax = false;

        public void Start()
        {
            enabled = false;
            controllerObject.AddListener(this);
        }

        public override void OnChangeOwner(SmartObjectSync sync, VRCPlayerApi oldOwner, VRCPlayerApi newOwner)
        {
        }
        public override void OnChangeState(SmartObjectSync sync, int oldState, int newState)
        {
            if (controllerObject.IsHeld())
            {
                if (!enabled && enableOnPickupController)
                {
                    enabled = true;
                }
            } else
            {
                if (enabled && disableOnDropController)
                {
                    ResetController();
                    MoveToConstrainedPos();
                    enabled = false;
                }
            }
        }
        bool hitMin;
        bool hitMax;
        Quaternion enableRotation;
        Vector3 enablePosition;
        public void OnEnable()
        {
            startPos = constrainedObject.localPosition;
            startRot = constrainedObject.localRotation;
            enablePosition = controllerObject.transform.localPosition;
            enableRotation = controllerObject.transform.localRotation;
            hitMin = false;
            hitMax = false;
        }
        Quaternion relativeRotation;
        Vector3 relativePosition;
        Vector3 startRelativePos;
        Vector3 currentRelativePos;
        public void MoveToConstrainedPos()
        {
            constrainedObject.localPosition = startPos;
            constrainedObject.localRotation = startRot;
            if (Utilities.IsValid(controllerObject.transform.parent))
            {
                relativeRotation = controllerObject.transform.parent.rotation * enableRotation;
                relativePosition = controllerObject.transform.parent.position + controllerObject.transform.parent.rotation * enablePosition;
            } else
            {
                relativeRotation = enableRotation;
                relativePosition = enablePosition;
            }
            startRelativePos = Quaternion.Inverse(relativeRotation) * (relativePosition - constrainedObject.transform.position);
            currentRelativePos = Quaternion.Inverse(relativeRotation) * (controllerObject.transform.position - constrainedObject.transform.position);
            switch (constraint)
            {
                case (CONSTRAINT_MOVE_X):
                    {
                        distance = Mathf.Clamp(currentRelativePos.x - startRelativePos.x, min, max);
                        constrainedObject.position += relativeRotation * Vector3.right * distance;
                        break;
                    }
                case (CONSTRAINT_MOVE_Y):
                    {
                        distance = Mathf.Clamp(currentRelativePos.y - startRelativePos.y, min, max);
                        constrainedObject.position += relativeRotation * Vector3.up * distance;
                        break;
                    }
                case (CONSTRAINT_MOVE_Z):
                    {
                        distance = Mathf.Clamp(currentRelativePos.z - startRelativePos.z, min, max);
                        constrainedObject.position += relativeRotation * Vector3.forward * distance;
                        break;
                    }
                case (CONSTRAINT_ROTATE_X):
                    {
                        startRelativePos.x = 0;
                        currentRelativePos.x = 0;
                        distance = Mathf.Clamp(Vector3.SignedAngle(startRelativePos, currentRelativePos, Vector3.right), min, max);
                        constrainedObject.rotation = Quaternion.AngleAxis(distance, relativeRotation * Vector3.right) * constrainedObject.rotation;
                        break;
                    }
                case (CONSTRAINT_ROTATE_Y):
                    {
                        startRelativePos.y = 0;
                        currentRelativePos.y = 0;
                        distance = Mathf.Clamp(Vector3.SignedAngle(startRelativePos, currentRelativePos, Vector3.right), min, max);
                        constrainedObject.rotation = Quaternion.AngleAxis(distance, relativeRotation * Vector3.right) * constrainedObject.rotation;
                        break;
                    }
                case (CONSTRAINT_ROTATE_Z):
                    {
                        startRelativePos.z = 0;
                        currentRelativePos.z = 0;
                        distance = Mathf.Clamp(Vector3.SignedAngle(startRelativePos, currentRelativePos, Vector3.right), min, max);
                        constrainedObject.rotation = Quaternion.AngleAxis(distance, relativeRotation * Vector3.right) * constrainedObject.rotation;
                        break;
                    }
            }

            if (distance == min)
            {
                OnMin();
            }
            else if (distance == max)
            {
                OnMax();
            }
        }

        public override void PostLateUpdate()
        {
            MoveToConstrainedPos();
        }

        
        public void OnMin()
        {
            if (!controllerObject.IsLocalOwner() || hitMin)
            {
                return;
            }
            hitMax = false;
            hitMin = true;
            if (nextMultiStageOnMin && Utilities.IsValid(multi))
            {
                multi.NextStage();
            }
            if (Utilities.IsValid(hitMinEvent) && hitMinEvent.Length >= 0 && Utilities.IsValid(targetUdon))
            {
                targetUdon.SendCustomEvent(hitMinEvent);
                if (dropOnHitLimit)
                {
                    controllerObject.pickup.Drop();
                }
            }
        }

        public void OnMax()
        {
            if (!controllerObject.IsLocalOwner() || hitMax)
            {
                return;
            }
            hitMax = true;
            hitMin = false;
            if (nextMultiZtageOnMax && Utilities.IsValid(multi))
            {
                multi.NextStage();
            }
            if (Utilities.IsValid(hitMinEvent) && hitMinEvent.Length >= 0 && Utilities.IsValid(targetUdon))
            {
                targetUdon.SendCustomEvent(hitMaxEvent);
                if (dropOnHitLimit)
                {
                    controllerObject.pickup.Drop();
                }
            }
        }

        public void ResetController()
        {
            controllerObject.transform.localPosition = enablePosition;
            controllerObject.transform.localRotation = enableRotation;
        }
    }
}