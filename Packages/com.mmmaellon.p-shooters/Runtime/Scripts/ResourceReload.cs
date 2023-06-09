
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.P_Shooters
{
    public class ResourceReload : SimpleReload
    {
        [Tooltip("Define a resource on the player objects and write it's name here to use it as ammo.")]
        public ResourceManager resource;
        [Tooltip("If this is true, then reloading the gun when the magazine isn't empty wastes ammo.")]
        public bool wasteAmmoOnReload = false;
        [Tooltip("If this is true, then chambering the gun while a round was already chambered will waste ammo.")]
        public bool wasteAmmoOnRechamber = false;

        bool ammoResourceIdAssigned;

        public override bool CanReload()
        {
            if (!Utilities.IsValid(resource.localPlayerObject))
            {
                return false;
            }
            if (resource.id >= 0)
            {
                if (resource.localPlayerObject.GetResourceValueById(resource.id) <= 0)
                {
                    return false;
                }
            }
            return base.CanReload();
        }

        public override void ReloadEnd()
        {
            if (!Utilities.IsValid(shooter) || !shooter.sync.IsLocalOwner() || shooter.state == P_Shooter.STATE_DISABLED || !Utilities.IsValid(resource.localPlayerObject))
            {
                return;
            }

            if (resource.id >= 0)
            {
                int reloadAmount = Mathf.Min(magCapacity, resource.localPlayerObject.GetResourceValueById(resource.id));
                if (wasteAmmoOnRechamber)
                {
                    magAmmo = magCapacity;
                }
                else
                {
                    reloadAmount -= magAmmo;
                    magAmmo += reloadAmount;
                }
                resource.localPlayerObject.ChangeResourceValueById(resource.id, -reloadAmount);
            }
            else
            {
                magAmmo = magCapacity;
            }
            shooter.state = P_Shooter.STATE_IDLE;
        }
        
        
        public override bool ChamberAmmo()
        {
            if (!Utilities.IsValid(shooter) || !shooter.sync.IsLocalOwner() || (chamberCapacity <= 0) || ammoPerShot <= 0)
            {
                return false;
            }
            shooter._print("ChamberAmmo");
            if (chamberAmmo > 0)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(EjectEmptyFX));
            }
            actualChamberAmmoAmount = Mathf.Min(magAmmo, Mathf.Min(ammoPerShot, chamberCapacity - chamberAmmo));
            if (chamberAmmo >= chamberCapacity && !wasteAmmoOnRechamber)
            {
                return false;
            }
            if (magCapacity > 0)
            {
                if (magAmmo > 0)
                {
                    magAmmo -= actualChamberAmmoAmount;
                }
                else
                {
                    //NOT ENOUGH AMMO IN MAG
                    shooter._print("NOT ENOUGH AMMO IN MAG");
                    return false;
                }
            }
            chamberAmmo += actualChamberAmmoAmount;
            return true;
        }
    }
}
