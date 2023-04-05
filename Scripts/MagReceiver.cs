
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace MMMaellon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class MagReceiver : UdonSharpBehaviour
    {
        [System.NonSerialized]
        public MagReload magReload;
        [System.NonSerialized, FieldChangeCallback(nameof(attachedMag))]
        public Mag _attachedMag;
        public bool ejectExistingMagOnTap;
        [Tooltip("How the mag gets ejected")]
        public Vector3 ejectVelocity = Vector3.down;
        [Tooltip("How long we have to wait after ejecting to insert a new mag")]
        public float ejectCooldown = 0.5f;
        float lastEject = -1001f;

        public void OnTriggerEnter(Collider other)
        {
            if ((lastEject + ejectCooldown > Time.realtimeSinceStartup) || !Utilities.IsValid(other) || !Utilities.IsValid(magReload) || !Utilities.IsValid(magReload.shooter) || !magReload.shooter.sync.IsLocalOwner())
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
            attachedMag = other.GetComponent<Mag>();
            if (Utilities.IsValid(attachedMag))
            {
                if (!attachedMag.childState.IsActiveState())
                {
                    attachedMag.Attach(transform);
                }
                else
                {
                    attachedMag = null;
                }
            }
        }

        public void Eject()
        {
            lastEject = Time.realtimeSinceStartup;
            if (Utilities.IsValid(attachedMag) && attachedMag.childState.IsActiveState())
            {
                attachedMag.childState.sync.rigid.velocity = attachedMag.transform.rotation * ejectVelocity;
                attachedMag.childState.ExitState();
                attachedMag.childState.sync.vel = attachedMag.childState.sync.rigid.velocity;
                attachedMag = null;
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
            }
        }
    }
}