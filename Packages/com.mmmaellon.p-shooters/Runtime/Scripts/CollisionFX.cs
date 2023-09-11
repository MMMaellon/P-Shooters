
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.P_Shooters
{
    public class CollisionFX : UdonSharpBehaviour
    {
        public ParticleSystem particles;
        public AudioSource audioSource;
        public AudioClip[] audioClips;
        public LayerMask layerMask;
        public bool allowOverlappingAudio = false;
        public bool fxOnCollisionEnter = true;
        public bool fxOnTriggerEnter = true;

        public float cooldown = 0.5f;
        float lastHit = -1001f;
        public void OnCollisionEnter(Collision collision)
        {
            if (!Utilities.IsValid(collision.gameObject) || !fxOnCollisionEnter || lastHit + cooldown > Time.timeSinceLevelLoad)
            {
                return;
            }
            lastHit = Time.timeSinceLevelLoad;
            if (((1 << collision.gameObject.layer) & layerMask) != 0)
            {
                particles.transform.position = collision.contacts[0].point;
                audioSource.transform.position = collision.contacts[0].point;
                particles.Play();
                if (allowOverlappingAudio)
                {
                    audioSource.PlayOneShot(audioClips[Random.Range(0, audioClips.Length)]);
                }
                else
                {
                    audioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
                    audioSource.Play();
                }
            }
        }
        public void OnTriggerEnter(Collider other)
        {
            if (!Utilities.IsValid(other) || !fxOnTriggerEnter || lastHit + cooldown > Time.timeSinceLevelLoad)
            {
                return;
            }
            lastHit = Time.timeSinceLevelLoad;
            if (((1 << other.gameObject.layer) & layerMask) != 0)
            {
                particles.transform.position = transform.position;
                audioSource.transform.position = transform.position;
                particles.Play();
                if (allowOverlappingAudio)
                {
                    audioSource.PlayOneShot(audioClips[Random.Range(0, audioClips.Length)]);
                }
                else
                {
                    audioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
                    audioSource.Play();
                }
            }
        }
    }
}