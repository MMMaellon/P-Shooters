
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class MagReload : AmmoTracker
    {
        public MagReceiver magReceiver;
        [Tooltip("How much ammo each bullet consumes")]
        public int ammoPerShot = 1;
        [Tooltip("How much ammo can be chambered. Some guns like shotguns require chambering each round instead of loading a magazine. Set to 0 to have the gun shoot directly from the mag.")]
        public int chamberCapacity = 1;
        [Tooltip("If this is true, then chambering the gun while a round was already chambered will waste ammo.")]
        public bool wasteAmmoOnRechamber = false;
        
        [System.NonSerialized, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(chamberAmmo))]
        public int _chamberAmmo = 0;
        [Tooltip("After shooting, the next round automatically gets chambered.")]
        public bool autoChamber;
        public int chamberAmmo
        {
            get => _chamberAmmo;
            set
            {
                if (autoChamber && _chamberAmmo > value)
                {
                    EjectEmptyFX();
                }
                _chamberAmmo = value;
                if (!Utilities.IsValid(shooter))
                {
                    return;
                }
                if (shooter.sync.IsLocalOwner())
                {
                    RequestSerialization();
                }
                shooter.animator.SetInteger("chamber", value);
            }
        }
        public override void Start()
        {
            base.Start();
            magReceiver.magReload = this;
            magReceiver.attachedMag = magReceiver.GetComponentInChildren<Mag>();
            if (Utilities.IsValid(magReceiver.attachedMag))
            {
                magReceiver.attachedMag.Attach(magReceiver.transform);
            } else
            {
                shooter.state = P_Shooter.STATE_RELOAD;
            }
        }
        public override bool CanReload()
        {
            if (shooter.state != P_Shooter.STATE_IDLE || !Utilities.IsValid(magReceiver.attachedMag))
            {
                return false;
            }
            return true;
        }

        public override bool CanShoot()
        {
            if (chamberCapacity > 0)
            {
                return chamberAmmo >= ammoPerShot;
            }
            return Utilities.IsValid(magReceiver.attachedMag) && magReceiver.attachedMag.ammo >= ammoPerShot;
        }

        public override void Shoot()
        {
            Debug.LogWarning("Start of Shoot");
            if (shooter.state == P_Shooter.STATE_IDLE)
            {
                Debug.LogWarning("we were idle");
                if (CanShoot())
                {
                    Debug.LogWarning("we can shoot");
                    shooter.state = P_Shooter.STATE_SHOOT;
                }
                else
                {
                    Debug.LogWarning("we cannot shoot");
                    shooter.state = P_Shooter.STATE_EMPTY;
                }
            }
            Debug.LogWarning("End of Shoot");
        }

        public override void Reload()
        {
            shooter.state = P_Shooter.STATE_RELOAD;
            magReceiver.Eject();
        }

        public override void ReloadEnd()
        {
            if (!Utilities.IsValid(shooter) || !shooter.sync.IsLocalOwner() || shooter.state == P_Shooter.STATE_DISABLED)
            {
                return;
            }
            shooter.state = P_Shooter.STATE_IDLE;
        }

        int actualChamberAmmoAmount;
        public override bool ChamberAmmo()
        {
            if (chamberCapacity <= 0 || !Utilities.IsValid(shooter) || !shooter.sync.IsLocalOwner() || shooter.state == P_Shooter.STATE_DISABLED)
            {
                return false;
            }
            if (!wasteAmmoOnRechamber)
            {
                if (chamberAmmo < chamberCapacity && Utilities.IsValid(magReceiver.attachedMag))
                {
                    actualChamberAmmoAmount = Mathf.Min(chamberCapacity - chamberAmmo, magReceiver.attachedMag.ammo, ammoPerShot);
                    chamberAmmo += actualChamberAmmoAmount;
                    magReceiver.attachedMag.ammo -= actualChamberAmmoAmount;
                }
            } else if (Utilities.IsValid(magReceiver.attachedMag))
            {
                if (magReceiver.attachedMag.ammo <= 0)
                {
                    chamberAmmo = Mathf.Max(0, chamberAmmo - ammoPerShot);
                } else
                {
                    actualChamberAmmoAmount = Mathf.Min(magReceiver.attachedMag.ammo, ammoPerShot);
                    chamberAmmo = Mathf.Min(chamberCapacity, chamberAmmo + actualChamberAmmoAmount);
                    magReceiver.attachedMag.ammo -= actualChamberAmmoAmount;
                }
            } else
            {
                chamberAmmo = Mathf.Max(0, chamberAmmo - ammoPerShot);
            }
            if (chamberAmmo >= ammoPerShot)
            {
                shooter.state = P_Shooter.STATE_IDLE;
            }
            return true;
        }

        public override bool ConsumeAmmo()
        {
            if (Utilities.IsValid(shooter) && CanShoot())
            {
                if (chamberCapacity <= 0 || (autoChamber && Utilities.IsValid(magReceiver.attachedMag) && magReceiver.attachedMag.ammo >= ammoPerShot))
                {
                    magReceiver.attachedMag.ammo -= ammoPerShot;
                }
                else
                {
                    chamberAmmo -= ammoPerShot;
                }
                return true;
            }
            return false;
        }

        public void SetMagParameter()
        {
            if (Utilities.IsValid(magReceiver.attachedMag))
            {
                shooter.animator.SetInteger("mag", magReceiver.attachedMag.ammo);
            } else
            {
                shooter.animator.SetInteger("mag", 0);
            }
            shooter.animator.SetInteger("chamber", chamberCapacity > 0 ? chamberAmmo : 1);
        }

        public override void ReloadFX()
        {
            base.ReloadFX();
            if (!autoChamber)
            {
                EjectEmptyFX();
            }
        }
    }
}