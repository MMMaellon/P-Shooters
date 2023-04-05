
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
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
        public override void _Register(P_Shooter shooter)
        {
            base._Register(shooter);
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
            if (shooter.state == P_Shooter.STATE_IDLE)
            {
                if (CanShoot())
                {
                    shooter.state = P_Shooter.STATE_SHOOT;
                }
                else
                {
                    shooter.state = P_Shooter.STATE_EMPTY;
                }
            }
        }

        public override void Reload()
        {
            shooter.state = P_Shooter.STATE_RELOAD;
            magReceiver.Eject();
        }

        public override void ReloadEnd()
        {
            if (chamberCapacity <= 0 || chamberAmmo >= ammoPerShot)
            {
                shooter.state = P_Shooter.STATE_IDLE;
            }
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ReloadFX));
        }

        int actualChamberAmmoAmount;
        public override bool ChamberAmmo()
        {
            if (chamberCapacity <= 0 || !Utilities.IsValid(shooter) || !shooter.sync.IsLocalOwner() || shooter.state == P_Shooter.STATE_DISABLED)
            {
                return false;
            }
            if ((Utilities.IsValid(magReceiver.attachedMag) && magReceiver.attachedMag.ammo > 0) || (chamberAmmo > 0))
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ChamberFX));
            }
            if (chamberAmmo > 0)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ChamberParticleFX));
            }
            if (!wasteAmmoOnRechamber)
            {
                if (chamberAmmo < chamberAmmo && Utilities.IsValid(magReceiver.attachedMag))
                {
                    actualChamberAmmoAmount = Mathf.Min(chamberCapacity - chamberAmmo, magReceiver.attachedMag.ammo, ammoPerShot);
                    chamberAmmo += actualChamberAmmoAmount;
                    magReceiver.attachedMag.ammo -= actualChamberAmmoAmount;
                }
            } else if (Utilities.IsValid(magReceiver.attachedMag))
            {
                actualChamberAmmoAmount = Mathf.Min(magReceiver.attachedMag.ammo, ammoPerShot);
                chamberAmmo = Mathf.Min(chamberCapacity, chamberAmmo + actualChamberAmmoAmount);
                magReceiver.attachedMag.ammo -= actualChamberAmmoAmount;
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
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ChamberFX));
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
    }
}