
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    // [RequireComponent(typeof(AudioSource))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class P_ShooterParticleEffects : UdonSharpBehaviour
    {
        public P_Shooter shooter;
        public ParticleSystem shooter_system;
        public ParticleSystem hit_particle_system;
        public ParticleSystem trail_particle_system;
        public float sound_interval = 0.1f;
        public float last_sound = 0.1f;
        public ParticleSystem.Particle[] particles = new ParticleSystem.Particle[10];
        public AudioSource sound_source;
        public AudioClip hit_sound_override;
        void Start()
        {
            if (sound_source != null)
            {
                sound_source.transform.parent = null;
            }
        }

        void OnParticleCollision(GameObject other)
        {
            if (last_sound + sound_interval > Time.timeSinceLevelLoad)
            {
                return;
            }
            last_sound = Time.timeSinceLevelLoad;
            int particleCount = hit_particle_system.GetParticles(particles, 5);
            AudioClip clip = null;
            if (shooter != null && hit_sound_override == null)
            {
                clip = shooter.sound_bullet_impact_environment;
            }
            else
            {
                clip = hit_sound_override;
            }
            //only play at the last particle
            if (particleCount > 0 && particleCount < particles.Length)
            {
                Vector3 sound_pos = particles[particleCount - 1].position;
                if (Vector3.Distance(sound_pos, Networking.LocalPlayer.GetPosition()) < sound_source.maxDistance)
                {
                    sound_source.clip = clip;
                    sound_source.transform.position = sound_pos;
                    sound_source.Play();
                    // AudioSource.PlayClipAtPoint(clip, sound_pos);
                }
            }
        }

        void OnParticleTrigger()
        {
            SendCustomEventDelayedFrames(nameof(AlignTrailToParticle), 0, VRC.Udon.Common.Enums.EventTiming.LateUpdate);
        }

        public void AlignTrailToParticle()
        {
            if (shooter_system != null)
            {
                int particleCount = shooter_system.GetParticles(particles, 5);
                if (trail_particle_system != null && particleCount > 0 && particleCount < particles.Length && particles[particleCount - 1].velocity.sqrMagnitude > 0)
                {
                    Vector3 trail_pos = particles[particleCount - 1].position;
                    Quaternion trail_rot = Quaternion.LookRotation(particles[particleCount - 1].velocity);
                    trail_particle_system.transform.position = trail_pos;
                    trail_particle_system.transform.rotation = trail_rot;
                    trail_particle_system.Play();
                    ParticleSystem.EmissionModule emission = trail_particle_system.emission;
                    emission.enabled = true;
                }
                else if (trail_particle_system != null)
                {
                    ParticleSystem.EmissionModule emission = trail_particle_system.emission;
                    emission.enabled = false;
                }
            }
        }
    }
}