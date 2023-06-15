
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor;
using UdonSharpEditor;
using System.Collections.Immutable;
#endif

namespace MMMaellon.P_Shooters
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
        public AudioClip[] chamberSounds;
        [Range(0.0f, 1.0f)]
        public float chamberVol = 1.0f;
        public int chamberAmmo
        {
            get => _chamberAmmo;
            set
            {
                if (_chamberAmmo > value)
                {
                    EjectEmptyFX();
                }
                else if (_chamberAmmo < value)
                {
                    ChamberFX();
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
                shooter.animator.SetInteger("chamber", chamberCapacity > 0 ? value : -1001);
            }
        }
        public void Start()
        {
            if (Utilities.IsValid(magReceiver.attachedMag))
            {
                magReceiver.attachedMag.Attach(magReceiver.transform);
            }
        }


#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public override void Reset()
        {
            base.Reset();
            SetupMagReload(this);
        }

        public static void SetupMagReload(MagReload reload)
        {
            if (!Utilities.IsValid(reload) || (Utilities.IsValid(reload.magReceiver) && reload.magReceiver.magReload == reload))
            {
                //was null or was already set up
                return;
            }
            if (!Utilities.IsValid(reload.magReceiver))
            {
                Helper.ErrorLog(reload, "MagReload is missing a mag receiver");
                return;
            }
            if (!Helper.IsEditable(reload.magReceiver))
            {
                Helper.ErrorLog(reload, "MagReload's MagReceiver is not editable");
                return;
            }
            SerializedObject serialized = new SerializedObject(reload.magReceiver);
            serialized.FindProperty("magReload").objectReferenceValue = reload;
            serialized.ApplyModifiedProperties();
        }
#endif

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
                    //we must subtract first
                    magReceiver.attachedMag.ammo -= actualChamberAmmoAmount;
                    chamberAmmo = Mathf.Min(chamberCapacity, chamberAmmo + actualChamberAmmoAmount);
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
        }

        public override void OnEnable()
        {
            base.OnEnable();
            SetMagParameter();
            chamberAmmo = chamberAmmo;
        }

        public override void ReloadFX()
        {
            base.ReloadFX();
            if (!autoChamber)
            {
                EjectEmptyFX();
            }
        }
        public void ChamberFX()
        {
            RandomOneShot(chamberSounds, chamberVol);
        }
    }
}