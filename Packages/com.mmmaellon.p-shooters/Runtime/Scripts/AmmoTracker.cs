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