using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace MMMaellon
{
    public abstract class AmmoTracker : UdonSharpBehaviour
    {
        [System.NonSerialized]
        public P_Shooter shooter;

        public AudioSource reloadSource;
        public AudioClip[] reloads;
        public AudioSource chamberSource;
        public AudioClip[] chambers;
        public ParticleSystem chamberParticles;
        public AudioSource outOfAmmoSource;
        public AudioClip[] outOfAmmos;
        public abstract bool CanShoot();
        public abstract void Shoot();
        public abstract bool CanReload();
        public abstract void Reload();
        public abstract void ReloadEnd();
        public virtual void _Register(P_Shooter newShooter)
        {
            shooter = newShooter;
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
        public virtual void OutOfAmmoFX()
        {
            if (Utilities.IsValid(outOfAmmoSource) && outOfAmmos.Length > 0)
            {
                outOfAmmoSource.clip = outOfAmmos[Random.Range(0, outOfAmmos.Length)];
                outOfAmmoSource.Play();
            }
        }

        public virtual void ChamberFX()
        {
            if (Utilities.IsValid(chamberSource) && chambers.Length > 0)
            {
                chamberSource.clip = chambers[Random.Range(0, chambers.Length)];
                chamberSource.Play();
            }
        }

        public virtual void ChamberParticleFX()
        {
            if (Utilities.IsValid(chamberParticles))
            {
                chamberParticles.Play();
            }
        }
    }
}