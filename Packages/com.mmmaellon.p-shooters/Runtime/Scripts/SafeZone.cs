
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.P_Shooters
{
    public class SafeZone : UdonSharpBehaviour
    {
        public bool affectGunState = true;
        public bool affectPlayerState = true;
        public void OnTriggerStay(Collider other)
        {
            if (!Utilities.IsValid(other))
            {
                return;
            }
            P_Shooter gun = other.GetComponent<P_Shooter>();
            Player player = other.GetComponent<Player>();
            if (affectGunState && Utilities.IsValid(gun) && gun.sync.IsLocalOwner() && gun.state == P_Shooter.STATE_IDLE)
            {
                gun.state = P_Shooter.STATE_DISABLED;
            }
            else if (affectPlayerState && Utilities.IsValid(player) && player.IsOwnerLocal() && player.state == Player.STATE_NORMAL)
            {
                player.state = Player.STATE_INVINCIBLE;
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (!Utilities.IsValid(other))
            {
                return;
            }
            P_Shooter gun = other.GetComponent<P_Shooter>();
            Player player = other.GetComponent<Player>();
            if (affectGunState && Utilities.IsValid(gun) && gun.sync.IsLocalOwner() && gun.state == P_Shooter.STATE_DISABLED)
            {
                gun.state = P_Shooter.STATE_IDLE;
            }
            else if (affectPlayerState && Utilities.IsValid(player) && player.IsOwnerLocal() && player.state == Player.STATE_INVINCIBLE)
            {
                player.state = Player.STATE_NORMAL;
            }
        }

    }
}
