
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor;
using UdonSharpEditor;
using System.Collections.Generic;
using VRC.Udon.Serialization;
#endif

namespace MMMaellon.P_Shooters
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
            SetupMag(this);
        }
        public static void SetupMag(Mag mag)
        {
            if (!Utilities.IsValid(mag) || (Utilities.IsValid(mag.childState) && mag.childState.gameObject == mag.gameObject))
            {
                //was null or was already set up
                return;
            }
            if (!Helper.IsEditable(mag))
            {
                Helper.ErrorLog(mag, "Mag is not editable");
                return;
            }
            SerializedObject serialized = new SerializedObject(mag);
            serialized.FindProperty(nameof(childState)).objectReferenceValue = mag.GetComponent<ChildAttachmentState>();
            serialized.ApplyModifiedProperties();
            if (mag.childState == null)
            {
                Helper.ErrorLog(mag, "Mag is missing a ChildAttachmentState");
                return;
            }
        }
#endif

        public void Attach(Transform newTransform)
        {
            Debug.LogWarning("mag attach has been called");
            if (!Utilities.IsValid(newTransform))
            {
                Debug.LogWarning("mag return early");
                return;
            }
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

        public bool respawnAfterDrop = false;
        public bool refillAmmoOnRespawn = true;
        public float respawnTimer = 3f;
        private float lastDrop;

        public virtual void Start()
        {
            childState.sync.AddListener(this);
        }
        public void Respawn()
        {
            if (!respawnAfterDrop || lastDrop < 0 || lastDrop + respawnTimer > Time.realtimeSinceStartup)
            {
                return;
            }
            lastDrop = -1001f;
            if (refillAmmoOnRespawn)
            {
                ammo = _maxAmmo;
            }
            childState.sync.Respawn();
        }

        public override void OnChangeState(SmartObjectSync sync, int oldState, int newState)
        {
            // sync.pickup.pickupable = !childState.IsActiveState();

            if (!childState.IsActiveState() && Utilities.IsValid(receiver) && receiver.attachedMag == this)
            {
                receiver.Eject();
                receiver = null;
            }

            if (sync.IsHeld())
            {
                lastDrop = -1001f;
            } else
            {
                if ((oldState >= SmartObjectSync.STATE_LEFT_HAND_HELD || oldState < SmartObjectSync.STATE_SLEEPING) && sync.state < SmartObjectSync.STATE_LEFT_HAND_HELD && sync.state >= SmartObjectSync.STATE_SLEEPING)
                {
                    lastDrop = Time.realtimeSinceStartup;
                    if (Utilities.IsValid(receiver) && Utilities.IsValid(receiver.magReload) && Utilities.IsValid(receiver.magReload.shooter))
                    {
                        receiver.magReload.shooter.EnableAnimator();
                    } else if (respawnAfterDrop && sync.IsLocalOwner())
                    {
                        SendCustomEventDelayedSeconds(nameof(Respawn), respawnTimer + 0.1f);
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