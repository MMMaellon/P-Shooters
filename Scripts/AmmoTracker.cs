using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor;
using UdonSharpEditor;
using System.Collections.Immutable;
#endif

namespace MMMaellon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual), RequireComponent(typeof(P_Shooter))]
    public abstract class AmmoTracker : UdonSharpBehaviour
    {
        [System.NonSerialized]
        public P_Shooter shooter;

        public AudioSource reloadSource;
        public AudioClip[] reloads;
        public AudioSource reloadEndSource;
        public AudioClip[] reloadEnds;
        public AudioSource ejectEmptySource;
        public AudioClip[] ejectEmptys;
        public ParticleSystem ejectEmptyParticles;
        public AudioSource outOfAmmoSource;
        public AudioClip[] outOfAmmos;
        public abstract bool CanShoot();
        public abstract void Shoot();
        public abstract bool CanReload();
        public abstract void Reload();
        public abstract void ReloadEnd();
        public virtual void Start()
        {
            if (!Utilities.IsValid(shooter))
            {
                shooter = GetComponent<P_Shooter>();
                shooter.ammo = this;
            }
        }
        public abstract bool ChamberAmmo();
        public abstract bool ConsumeAmmo();

        public virtual void ReloadFX()
        {
            if (Utilities.IsValid(reloadSource) && reloads.Length > 0)
            {
                reloadSource.clip = reloads[Random.Range(0, reloads.Length)];
                reloadSource.Play();
            }
        }
        public virtual void ReloadEndFX()
        {
            if (Utilities.IsValid(reloadEndSource) && reloadEnds.Length > 0)
            {
                reloadEndSource.clip = reloadEnds[Random.Range(0, reloadEnds.Length)];
                reloadEndSource.Play();
            }
        }
        public virtual void OutOfAmmoFX()
        {
            if (Utilities.IsValid(outOfAmmoSource) && outOfAmmos.Length > 0)
            {
                outOfAmmoSource.clip = outOfAmmos[Random.Range(0, outOfAmmos.Length)];
                outOfAmmoSource.Play();
            }
        }

        public virtual void EjectEmptyFX()
        {
            if (Utilities.IsValid(ejectEmptySource) && ejectEmptys.Length > 0)
            {
                ejectEmptySource.clip = ejectEmptys[Random.Range(0, ejectEmptys.Length)];
                ejectEmptySource.Play();
            }
            if (Utilities.IsValid(ejectEmptyParticles))
            {
                ejectEmptyParticles.Play();
            }
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
        public void Reset()
        {
            SerializedObject obj = new SerializedObject(GetComponent<P_Shooter>());
            obj.FindProperty("ammo").objectReferenceValue = this;
            obj.ApplyModifiedProperties();
        }
#endif
    }
}