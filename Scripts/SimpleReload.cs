
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SimpleReload : AmmoTracker
    {
        public KeyCode desktopReloadShortcut = KeyCode.E;
        public SmartObjectSync vrReloadPickup = null;
        [Tooltip("How much ammo each bullet consumes")]
        public int ammoPerShot = 1;

        [Tooltip("How much ammo can fit in the magazine. This is refilled on every reload")]
        public int magCapacity = 6;
        [Tooltip("How much ammo can be chambered. Some guns like shotguns require chambering each round instead of loading a magazine. Set to 0 to have the gun shoot directly from the mag.")]
        public int chamberCapacity = 0;
        [Tooltip("Reload when you aim straight down. Set to 0 to disable")]
        public float pointDownToReloadAngle = 15f;
        [Tooltip("Reload when you aim straight up. Set to 0 to disable")]
        public float pointUpToReloadAngle = 15f;
        [Tooltip("Only applies if starting ammo was defined above. Start with starting ammo already loaded into magazine and a round chambered.")]
        public bool startLoaded = true;
        [Tooltip("If this is true, then the gun automatically reloads if the player pulls the trigger while it's empty.")]
        public bool autoReload = true;

        
        [System.NonSerialized, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(magAmmo))]
        public int _magAmmo;
        public int magAmmo
        {
            get => _magAmmo;
            set
            {
                if (_magAmmo > value)
                {
                    EjectEmptyFX();
                }
                _magAmmo = value;
                if (!Utilities.IsValid(shooter))
                {
                    return;
                }
                if (shooter.sync.IsLocalOwner())
                {
                    RequestSerialization();
                }
                shooter.animator.SetInteger("mag", value);
            }
        }

        [System.NonSerialized, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(chamberAmmo))]
        public int _chamberAmmo = 0;
        public int chamberAmmo
        {
            get => _chamberAmmo;
            set
            {
                if (_chamberAmmo > value)
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
            if (startLoaded)
            {
                chamberAmmo = chamberCapacity;
                magAmmo = magCapacity;
            }
        }

        public override void Shoot()
        {
            if (shooter.state == P_Shooter.STATE_IDLE)
            {
                if (CanShoot())
                {
                    shooter.state = P_Shooter.STATE_SHOOT;
                } else if (autoReload)
                {
                    Reload();
                } else
                {
                    shooter.state = P_Shooter.STATE_EMPTY;
                }
            }
        }

        public override bool CanReload()
        {
            return (chamberCapacity <= 0) ? magCapacity > 0 && magAmmo < magCapacity : chamberAmmo < chamberCapacity;
        }

        Vector3 pointVector;
        public override void UpdateLoop()
        {
            if (!loop)
            {
                return;
            }
            SendCustomEventDelayedFrames(nameof(UpdateLoop), 0, VRC.Udon.Common.Enums.EventTiming.LateUpdate);
            
            if (!Utilities.IsValid(shooter))
            {
                return;
            }
            if (shooter.sync.IsLocalOwner() && shooter.sync.IsHeld())
            {
                if (Input.GetKeyDown(desktopReloadShortcut))
                {
                    Reload();
                }
                pointVector = transform.rotation * Vector3.forward;
                if (pointDownToReloadAngle > 0 && pointDownToReloadAngle >= Vector3.Angle(Vector3.down, pointVector))
                {
                    Reload();
                }
                else if (pointUpToReloadAngle > 0 && pointUpToReloadAngle >= Vector3.Angle(Vector3.up, pointVector))
                {
                    Reload();
                }
            }
            if (Utilities.IsValid(vrReloadPickup) && vrReloadPickup.IsHeld())
            {
                if (vrReloadPickup.IsLocalOwner())
                {
                    if (vrReloadPickup.pickup.currentHand == VRC_Pickup.PickupHand.Left)
                    {
                        VRCPlayerApi.TrackingData data = vrReloadPickup.owner.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
                        vrReloadPickup.transform.position = data.position;
                        vrReloadPickup.transform.rotation = data.rotation * Quaternion.Euler(0, 67.5f, 90); //found experimentally
                    }
                    else if (vrReloadPickup.pickup.currentHand == VRC_Pickup.PickupHand.Right)
                    {
                        VRCPlayerApi.TrackingData data = vrReloadPickup.owner.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
                        vrReloadPickup.transform.position = data.position;
                        vrReloadPickup.transform.rotation = data.rotation * Quaternion.Euler(0, 67.5f, 90);
                    }
                } else
                {
                    vrReloadPickup.Interpolate();
                }
            }
        }

        [System.NonSerialized]
        public int actualChamberAmmoAmount;
        public override bool ChamberAmmo()
        {
            if (!Utilities.IsValid(shooter) || !shooter.sync.IsLocalOwner() || (chamberCapacity <= 0) || ammoPerShot <= 0)
            {
                return false;
            }
            shooter._print("ChamberAmmo");
            actualChamberAmmoAmount = Mathf.Min(magAmmo, Mathf.Min(ammoPerShot, chamberCapacity - chamberAmmo));
            if (magCapacity > 0)
            {
                if (magAmmo > 0)
                {
                    magAmmo -= actualChamberAmmoAmount;
                } else
                {
                    //NOT ENOUGH AMMO IN MAG
                    shooter._print("NOT ENOUGH AMMO IN MAG");
                    return false;
                }
            }
            chamberAmmo += actualChamberAmmoAmount;
            return true;
        }
        public override bool CanShoot()
        {
            if (chamberCapacity > 0 && chamberAmmo >= ammoPerShot)
            {
                return true;
            }
            if (Utilities.IsValid(vrReloadPickup) && vrReloadPickup.IsHeld())
            {
                return false;
            }
            if (ammoPerShot <= 0 || magCapacity <= 0)
            {
                return true;
            }
            if (chamberCapacity <= 0 && magAmmo >= ammoPerShot)
            {
                return true;
            }
            return false;
        }
        public override bool ConsumeAmmo()
        {
            if (Utilities.IsValid(shooter) && shooter.sync.IsLocalOwner() && CanShoot())
            {
                if ((chamberCapacity <= 0))
                {
                    magAmmo -= ammoPerShot;
                } else
                {
                    chamberAmmo -= ammoPerShot;
                }
                return true;
            }
            return false;
        }

        public override void Reload()
        {
            if (!Utilities.IsValid(shooter) || !shooter.sync.IsLocalOwner())
            {
                return;
            }
            if (shooter.state != P_Shooter.STATE_IDLE && shooter.state != P_Shooter.STATE_EMPTY)
            {
                return;
            }
            if (CanReload())
            {
                shooter.state = P_Shooter.STATE_RELOAD;
            } else
            {
                shooter.state = P_Shooter.STATE_EMPTY;
            }
        }

        public override void ReloadEnd()
        {
            if (!Utilities.IsValid(shooter) || !shooter.sync.IsLocalOwner() || shooter.state == P_Shooter.STATE_DISABLED)
            {
                return;
            }
            magAmmo = magCapacity;
            shooter.state = P_Shooter.STATE_IDLE;
        }

        public override void OnPickup()
        {
            if (Utilities.IsValid(vrReloadPickup))
            {
                vrReloadPickup.pickup.pickupable = shooter.sync.IsLocalOwner();
            }
        }
        public override void OnDrop()
        {
            if (Utilities.IsValid(vrReloadPickup))
            {
                vrReloadPickup.pickup.pickupable = false;
            }
        }

        public override void ReloadEndFX()
        {
            base.ReloadEndFX();
            if (Utilities.IsValid(vrReloadPickup))
            {
                vrReloadPickup.pickup.Drop();
            }
        }
    }
}
