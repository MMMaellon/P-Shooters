
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor;
using UdonSharpEditor;
using System.Collections.Generic;
#endif

namespace MMMaellon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual), RequireComponent(typeof(ChildAttachmentState))]
    public class Mag : SmartObjectSyncListener
    {
        [System.NonSerialized]
        public MagReceiver receiver;
        [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(ammo))]
        public int _ammo = 6;
        [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(maxAmmo))]
        public int _maxAmmo = 6;
        public int ammo
        {
            get => _ammo;
            set
            {
                if (_ammo > value && Utilities.IsValid(receiver) && Utilities.IsValid(receiver.magReload) && (receiver.magReload.chamberCapacity <= 0 || receiver.magReload.chamberAmmo > 0))
                {
                    receiver.magReload.EjectEmptyFX();
                }
                _ammo = value;
                if (childState.sync.IsLocalOwner())
                {
                    RequestSerialization();
                }
                if (Utilities.IsValid(childState.parentTransform))
                {
                    if (Utilities.IsValid(receiver) && Utilities.IsValid(receiver.magReload))
                    {
                        receiver.magReload.SetMagParameter();
                    }
                }
            }
        }
        public int maxAmmo
        {
            get => _maxAmmo;
            set
            {
                _maxAmmo = value;
                if (childState.sync.IsLocalOwner())
                {
                    RequestSerialization();
                }
            }
        }
        public ChildAttachmentState childState;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset()
        {
            SerializedObject serialized = new SerializedObject(this);
            serialized.FindProperty(nameof(childState)).objectReferenceValue = GetComponent<ChildAttachmentState>();
            serialized.ApplyModifiedProperties();
        }
#endif

        public void Attach(Transform newTransform)
        {
            Debug.LogWarning("MAG Attach");
            if (!Utilities.IsValid(newTransform))
            {
                return;
            }
            Debug.LogWarning("MAG Attach | transform valid: " + newTransform.name);
            childState.Attach(newTransform);
            if (childState.IsActiveState())
            {
                childState.sync.pos = Vector3.zero;
                childState.sync.rot = Quaternion.identity;
                RequestSerialization();
            }
        }

        public override void OnPickup()
        {
            if (!Utilities.IsValid(transform.parent))
            {
                return;
            }
            if (!Utilities.IsValid(receiver) || !Utilities.IsValid(receiver.magReload) || !Utilities.IsValid(receiver.magReload.shooter))
            {
                return;
            }
            if (receiver.magReload.CanReload())
            {
                receiver.magReload.Reload();
            }
            else
            {
                //Nope! let me go!
                childState.Attach(receiver.transform);
            }
        }

        public void Refill()
        {
            ammo = maxAmmo;
        }

        public bool hideAfterThrow;
        public float hideTimer = 3f;
        private float lastDrop;
        public void Start()
        {
            childState.sync.AddListener(this);
        }
        public void Respawn()
        {
            if (!hideAfterThrow || lastDrop < 0 || lastDrop + hideTimer > Time.realtimeSinceStartup)
            {
                return;
            }
            lastDrop = -1001f;
        }

        public override void OnChangeState(SmartObjectSync sync, int oldState, int newState)
        {
            // sync.pickup.pickupable = !childState.IsActiveState();

            if (childState.IsActiveState() && Utilities.IsValid(childState.parentTransform))
            {
                receiver = childState.parentTransform.GetComponent<MagReceiver>();
                if (Utilities.IsValid(receiver))
                {
                    receiver.attachedMag = this;
                }
            }
            else if (Utilities.IsValid(receiver))
            {
                if (receiver.attachedMag == this)
                {
                    receiver.attachedMag = null;
                }
                receiver = null;
            }
            
            if (sync.IsHeld())
            {
                lastDrop = -1001f;
            } else
            {
                if (oldState == SmartObjectSync.STATE_LEFT_HAND_HELD || oldState == SmartObjectSync.STATE_RIGHT_HAND_HELD || oldState == SmartObjectSync.STATE_NO_HAND_HELD)
                {
                    lastDrop = Time.realtimeSinceStartup;
                    if (Utilities.IsValid(receiver) && Utilities.IsValid(receiver.magReload) && Utilities.IsValid(receiver.magReload.shooter))
                    {
                        receiver.magReload.shooter.EnableAnimator();
                    } else if (hideAfterThrow && sync.IsLocalOwner())
                    {
                        SendCustomEventDelayedSeconds(nameof(Respawn), hideTimer + 0.1f);
                    }
                }
            }
        }

        public override void OnChangeOwner(SmartObjectSync sync, VRCPlayerApi oldOwner, VRCPlayerApi newOwner)
        {
            // if (!sync.IsLocalOwner())
            // {
            //     Networking.SetOwner(sync.owner, gameObject);
            // }
        }
    }
}