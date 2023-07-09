
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor;
using UdonSharpEditor;
using System.Collections.Immutable;
#endif

namespace MMMaellon.P_Shooters
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class MagReceiver : UdonSharpBehaviour
    {
        [HideInInspector]
        public MagReload magReload;
        [FieldChangeCallback(nameof(attachedMag))]
        public Mag _attachedMag;
        public bool ejectExistingMagOnTInteract = false;
        public bool ejectExistingMagOnTap = true;
        [Tooltip("How the mag gets ejected")]
        public Vector3 ejectVelocity = Vector3.down;
        [Tooltip("How long we have to wait after ejecting to insert a new mag")]
        public float ejectCooldown = 0.5f;
        float lastEject = -1001f;


        public virtual void Start()
        {
            attachedMag = attachedMag;
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset()
        {
            SetupMagReceiver(this);
        }

        public static void SetupMagReceiver(MagReceiver receiver)
        {
            if (!Utilities.IsValid(receiver) || (Utilities.IsValid(receiver.attachedMag) && receiver.attachedMag.transform.parent == receiver.transform))
            {
                //was null or was already set up
                return;
            }
            if (!Helper.IsEditable(receiver))
            {
                Helper.ErrorLog(receiver, "MagReceiver is not editable");
                return;
            }
            SerializedObject serialized = new SerializedObject(receiver);
            serialized.FindProperty("_attachedMag").objectReferenceValue = receiver.GetComponentInChildren<Mag>();
            serialized.ApplyModifiedProperties();
        }
#endif

        public void OnTriggerEnter(Collider other)
        {
            if ((lastEject + ejectCooldown > Time.realtimeSinceStartup) || !Utilities.IsValid(other) || !Utilities.IsValid(magReload) || !Utilities.IsValid(magReload.shooter) || !magReload.shooter.sync.IsLocalOwner())
            {
                return;
            }
            Mag otherMag = other.GetComponent<Mag>();
            if (!Utilities.IsValid(otherMag) || otherMag == attachedMag || otherMag.childState.IsActiveState())
            {
                return;
            }
            if (Utilities.IsValid(attachedMag))
            {
                if (ejectExistingMagOnTap)
                {
                    Eject();
                }
                return;
            }
            otherMag.Attach(transform);
        }

        public override void Interact()
        {
            Eject();
        }

        public void Eject()
        {
            lastEject = Time.realtimeSinceStartup;
            if (Utilities.IsValid(attachedMag) && attachedMag.childState.IsActiveState())
            {
                attachedMag.childState.sync.rigid.velocity = attachedMag.transform.rotation * ejectVelocity;
                attachedMag.childState.sync.vel = attachedMag.childState.sync.rigid.velocity;
                attachedMag.childState.ExitState();
            }
        }
        
        public Mag attachedMag{
            get => _attachedMag;
            set
            {
                _attachedMag = value;
                if (Utilities.IsValid(magReload))
                {
                    if (Utilities.IsValid(_attachedMag))
                    {
                        magReload.ReloadEnd();
                    }
                    magReload.SetMagParameter();
                }
                DisableInteractive = !ejectExistingMagOnTInteract || value == null;
            }
        }
    }
}