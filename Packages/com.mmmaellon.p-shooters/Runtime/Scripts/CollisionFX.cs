
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

        public bool LocalOnlyFX = true;

        public float cooldown = 0.5f;
        float lastHit = -1001f;

        public Vector3 startPosParticles;
        public Vector3 startPosSound;

        [System.NonSerialized, UdonSynced, FieldChangeCallback(nameof(fxPos))]
        public Vector3 _fxPos;
        public Vector3 fxPos
        {
            get => _fxPos;
            set
            {
                _fxPos = value;

                particles.transform.position = value;
                audioSource.transform.position = value;
                PlayFX();

                if (! Networking.LocalPlayer.IsOwner(gameObject))
                {
                    RequestSerialization();
                }
            }
        }

        public void Start()
        {
            startPosParticles = particles.transform.localPosition;
            startPosSound = audioSource.transform.localPosition;
        }
        public void OnCollisionEnter(Collision collision)
        {
            if (!Utilities.IsValid(collision.gameObject) || !fxOnCollisionEnter || lastHit + cooldown > Time.timeSinceLevelLoad)
            {
                return;
            }
            lastHit = Time.timeSinceLevelLoad;
            if (((1 << collision.gameObject.layer) & layerMask) != 0)
            {
                if (!LocalOnlyFX)
                {
                    Networking.LocalPlayer.TakeOwnership(gameObject);
                    fxPos = collision.contacts[0].point;
                }
                else
                {
                    particles.transform.position = collision.contacts[0].point;
                    audioSource.transform.position = collision.contacts[0].point;
                    PlayFX();
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
                particles.transform.localPosition = startPosParticles;
                audioSource.transform.localPosition = startPosSound;
                if (!LocalOnlyFX)
                {
                    fxPos = audioSource.transform.position;
                }
                else
                {
                    PlayFX();
                }
            }
        }

        public void PlayFX()
        {
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