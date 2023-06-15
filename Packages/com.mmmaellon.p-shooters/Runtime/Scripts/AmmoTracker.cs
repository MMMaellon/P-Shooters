using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor;
using UdonSharpEditor;
using System.Collections.Immutable;
#endif

namespace MMMaellon.P_Shooters
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual), RequireComponent(typeof(P_Shooter))]
    public abstract class AmmoTracker : UdonSharpBehaviour
    {
        [HideInInspector]
        public P_Shooter shooter;
        
        [FieldChangeCallback(nameof(reloadSpeed))]
        public float _reloadSpeed = 1.0f;
        public float reloadSpeed
        {
            get => _reloadSpeed;
            set
            {
                _reloadSpeed = value;
                shooter.animator.SetFloat("reload_speed", value);
            }
        }

        public AudioSource audioSource;
        public AudioClip[] reloads;
        [Range(0.0f, 1.0f)]
        public float reloadVol = 1.0f;
        public AudioClip[] reloadEnds;
        [Range(0.0f, 1.0f)]
        public float reloadEndVol = 1.0f;
        public AudioClip[] outOfAmmos;
        [Range(0.0f, 1.0f)]
        public float outOfAmmoVol = 1.0f;
        public AudioClip[] ejectEmptys;
        [Range(0.0f, 1.0f)]
        public float ejectEmptyVol = 1.0f;
        public ParticleSystem ejectEmptyParticles;
        public abstract bool CanShoot();
        public abstract void Shoot();
        public abstract bool CanReload();
        public abstract void Reload();
        public abstract void ReloadEnd();
        public abstract bool ChamberAmmo();
        public abstract bool ConsumeAmmo();

        public virtual void ReloadFX()
        {
            RandomOneShot(reloads, reloadVol);
        }
        public virtual void ReloadEndFX()
        {
            RandomOneShot(reloadEnds, reloadEndVol);
        }
        public virtual void OutOfAmmoFX()
        {
            RandomOneShot(outOfAmmos, outOfAmmoVol);
        }

        public virtual void EjectEmptyFX()
        {
            RandomOneShot(ejectEmptys, ejectEmptyVol);
            if (Utilities.IsValid(ejectEmptyParticles))
            {
                ejectEmptyParticles.Play();
            }
        }

        public void RandomOneShot(AudioClip[] clips, float volume)
        {
            if (Utilities.IsValid(audioSource) && clips.Length > 0)
            {
                audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
            }
        }
        public virtual void OnEnable()
        {
            //reset all the animator stuff
            reloadSpeed = reloadSpeed;
        }


        [System.NonSerialized]
        public bool _loop = false;
        public bool loop
        {
            get => _loop;
            set
            {
                if (!_loop && value)
                {
                    SendCustomEventDelayedFrames(nameof(UpdateLoop), 0, VRC.Udon.Common.Enums.EventTiming.LateUpdate);
                }
                _loop = value;
            }
        }
        public virtual void UpdateLoop()
        {
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public virtual void Reset()
        {
            SetupAmmoTracker(this);
        }

        public static void SetupAmmoTracker(AmmoTracker ammoTracker)
        {
            if (!Utilities.IsValid(ammoTracker) || (Utilities.IsValid(ammoTracker.shooter) && ammoTracker.shooter.ammo == ammoTracker && ammoTracker.gameObject == ammoTracker.shooter.gameObject))
            {
                //was null or was already set up
                return;
            }
            if (!Helper.IsEditable(ammoTracker))
            {
                Helper.ErrorLog(ammoTracker, "AmmoTracker is not editable");
                return;
            }
            SerializedObject obj = new SerializedObject(ammoTracker);
            obj.FindProperty("shooter").objectReferenceValue = ammoTracker.GetComponent<P_Shooter>();
            obj.ApplyModifiedProperties();
            if (ammoTracker.shooter == null)
            {
                Helper.ErrorLog(ammoTracker, "AmmoTracker is missing a P_Shooter");
                return;
            }
            if (Utilities.IsValid(ammoTracker.shooter.ammo) && ammoTracker.shooter.ammo != ammoTracker && ammoTracker.shooter.gameObject == ammoTracker.shooter.ammo.gameObject)
            {
                Helper.ErrorLog(ammoTracker, "AmmoTracker is already assigned to a different P_Shooter. Make sure you do not have two ammo trackers on the same object");
                return;
            }
            if (ammoTracker.shooter.ammo != ammoTracker)
            {
                if (!Helper.IsEditable(ammoTracker.shooter))
                {
                    Helper.ErrorLog(ammoTracker.shooter, "Shooter is not editable");
                    return;
                }
                SerializedObject serializedShooter = new SerializedObject(ammoTracker.shooter);
                serializedShooter.FindProperty("ammo").objectReferenceValue = ammoTracker;
                serializedShooter.ApplyModifiedProperties();
            }
        }
#endif
    }
}