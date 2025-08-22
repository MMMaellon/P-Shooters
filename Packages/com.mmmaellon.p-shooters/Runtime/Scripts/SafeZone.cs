
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.P_Shooters
{
    public class SafeZone : UdonSharpBehaviour
    {
        public void OnTriggerEnter(Collider other)
        {
            if (!Utilities.IsValid(other))
            {
                return;
            }
            P_Shooter gun = other.GetComponent<P_Shooter>();
            if (!Utilities.IsValid(gun) || !gun.sync.IsLocalOwner())
            {
                return;
            }
            gun.state = P_Shooter.STATE_DISABLED;
        }

        public void OnTriggerExit(Collider other)
        {
            if (!Utilities.IsValid(other))
            {
                return;
            }
            P_Shooter gun = other.GetComponent<P_Shooter>();
            if (!Utilities.IsValid(gun) || !gun.sync.IsLocalOwner())
            {
                return;
            }
            gun.state = P_Shooter.STATE_IDLE;
        }

    }
}
