
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    public class ResourceReload : SimpleReload
    {
        [System.NonSerialized]
        public int _ammoResourceId = -1001;
        [Tooltip("Define a resource on the player objects and write it's name here to use it as ammo.")]
        public string ammoResource = "ammo";
        [Tooltip("If this is true, then reloading the gun when the magazine isn't empty wastes ammo.")]
        public bool wasteAmmoOnReload = false;
        [Tooltip("If this is true, then chambering the gun while a round was already chambered will waste ammo.")]
        public bool wasteAmmoOnRechamber = false;

        bool ammoResourceIdAssigned;
        public int ammoResourceId
        {
            get
            {
                if (!ammoResourceIdAssigned && Utilities.IsValid(shooter) && Utilities.IsValid(shooter.localPlayerObject))
                {
                    ammoResourceIdAssigned = true;
                    _ammoResourceId = shooter.localPlayerObject.GetResourceId(ammoResource);
                }
                return _ammoResourceId;
            }
        }

        public override bool CanReload()
        {
            if (ammoResourceId >= 0)
            {
                if (shooter.localPlayerObject.GetResourceValueById(ammoResourceId) <= 0)
                {
                    return false;
                }
            }
            return base.CanReload();
        }

        public override void ReloadEnd()
        {
            if (!Utilities.IsValid(shooter) || !shooter.sync.IsLocalOwner() || shooter.state == P_Shooter.STATE_DISABLED)
            {
                return;
            }

            if (ammoResourceId >= 0)
            {
                int reloadAmount = Mathf.Min(magCapacity, shooter.localPlayerObject.GetResourceValueById(ammoResourceId));
                if (wasteAmmoOnRechamber)
                {
                    magAmmo = magCapacity;
                }
                else
                {
                    reloadAmount -= magAmmo;
                    magAmmo += reloadAmount;
                }
                shooter.localPlayerObject.ChangeResourceValueById(ammoResourceId, -reloadAmount);
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
            ChamberFX();
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
