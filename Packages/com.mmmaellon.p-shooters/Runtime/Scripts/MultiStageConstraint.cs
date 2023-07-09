
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.P_Shooters
{
    public class MultiStageConstraint : SmartObjectSyncListener
    {
        [System.NonSerialized, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(stage))]
        public int _stage = -1001;
        
        public int stage{
            get => _stage;
            set
            {
                _stage = value;
                for (int i = 0; i < stages.Length; i++)
                {
                    if (!Utilities.IsValid(stages[i]))
                    {
                        continue;
                    }
                    stages[i].enabled = i == value;
                }
                if (value < 0)
                {
                    if (Utilities.IsValid(controllerObject))
                    {
                        controllerObject.transform.localPosition = enablePosition;
                        controllerObject.transform.localRotation = enableRotation;
                    }
                    if (Utilities.IsValid(constrainedObject))
                    {
                        constrainedObject.localPosition = startPos;
                        constrainedObject.localRotation = startRot;
                    }
                }
                if (Networking.LocalPlayer.IsOwner(gameObject))
                {
                    RequestSerialization();
                }
            }
        }
        public PickupConstraint[] stages;
        Vector3 startPos;
        Quaternion startRot;
        Quaternion enableRotation;
        Vector3 enablePosition;
        public SmartObjectSync controllerObject;
        public Transform constrainedObject;

        public virtual void Start()
        {
            if (Utilities.IsValid(constrainedObject))
            {
                startPos = constrainedObject.localPosition;
                startRot = constrainedObject.localRotation;
            }

            if (Utilities.IsValid(controllerObject))
            {
                controllerObject.AddListener(this);
                enablePosition = controllerObject.transform.localPosition;
                enableRotation = controllerObject.transform.localRotation;
            }
        }

        public void NextStage()
        {
            if (!Utilities.IsValid(stages) || stages.Length <= 0 || !Networking.LocalPlayer.IsOwner(gameObject))
            {
                return;
            }
            if (stage < 0)
            {
                stage = 0;
            } else if (stage == stages.Length - 1)
            {
                stage = -1001;
            } else
            {
                stage += 1;
            }
        }

        public override void OnChangeState(SmartObjectSync sync, int oldState, int newState)
        {
            if (!Utilities.IsValid(sync))
            {
                return;
            }
            if (Networking.LocalPlayer.IsOwner(gameObject))
            {
                if (oldState != SmartObjectSync.STATE_LEFT_HAND_HELD && oldState != SmartObjectSync.STATE_RIGHT_HAND_HELD && oldState != SmartObjectSync.STATE_NO_HAND_HELD && sync.IsHeld())
                {
                    //onpickup
                    if (stage < 0)
                    {
                        NextStage();
                    }
                } else if (!sync.IsHeld())
                {
                    if (stage >= 0)
                    {
                        stage = -1001;
                    }
                }
            }
        }

        public override void OnChangeOwner(SmartObjectSync sync, VRCPlayerApi oldOwner, VRCPlayerApi newOwner)
        {
            // if (Utilities.IsValid(newOwner))
            // {
            //     Networking.SetOwner(newOwner, gameObject);
            // }
        }
    }
}
