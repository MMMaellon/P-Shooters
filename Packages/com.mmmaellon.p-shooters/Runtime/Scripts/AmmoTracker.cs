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
        public abstract void ResetAmmo();

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
            if (!enabled)
            {
                return;
            }
            RandomOneShot(ejectEmptys, ejectEmptyVol);
            if (Utilities.IsValid(ejectEmptyParticles))
            {
                ejectEmptyParticles.Play();
            }
        }

        public void RandomOneShot(AudioClip[] clips, float volume)
        {
            if (!enabled)
            {
                return;
            }
            if (Time.timeSinceLevelLoad < 5f)//delay to prevent the spam you get at load in
            {
                return;
            }
            if (Utilities.IsValid(audioSource) && clips.Length > 0)
            {
                audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
            }
        }
        public virtual void OnEnable()
        {
            if (shooter.ammo != this)
            {
                enabled = false;
                return;
            }
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
            if (ammoTracker.shooter.ammo == null)
            {
                if (!Helper.IsEditable(ammoTracker.shooter))
                {
                    Helper.ErrorLog(ammoTracker.shooter, "Shooter is not editable");
                    return;
                }
                SerializedObject serializedShooter = new SerializedObject(ammoTracker.shooter);
                serializedShooter.FindProperty("_ammo").objectReferenceValue = ammoTracker;
                serializedShooter.ApplyModifiedProperties();
            }
        }
#endif
    }
}