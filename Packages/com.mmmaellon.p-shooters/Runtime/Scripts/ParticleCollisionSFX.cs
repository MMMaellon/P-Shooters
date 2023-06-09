
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

namespace MMMaellon
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleCollisionSFX : UdonSharpBehaviour
    {
        [HideInInspector]
        public ParticleSystem particles;
        [Tooltip("This must be a sub-emitter of the root particle system that gets triggered on collisions")]
        public ParticleSystem hitParticles;
        public AudioSource audioSource;
        public bool playAtCollision = true;
        public bool allowOverlappingAudio = true;
        ParticleSystem.Particle[] particleArray = new ParticleSystem.Particle[10];//we're just going to hardcode 10 because we can't get the actual number

        public void OnParticleCollision(GameObject other)
        {
            if (!playAtCollision)
            {
                if (allowOverlappingAudio)
                {
                    audioSource.PlayOneShot(audioSource.clip);
                } else
                {
                    audioSource.Play();
                }
                return;
            }
            hitParticles.GetParticles(particleArray, particleArray.Length);
            if(particleArray[0].remainingLifetime > 0)
            {
                audioSource.transform.position = particleArray[0].position;
                if (allowOverlappingAudio)
                {
                    audioSource.PlayOneShot(audioSource.clip);
                }
                else
                {
                    audioSource.Play();
                }
            }
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset()
        {
            SetupParticleCollisionSFX(this);
        }

        public static void SetupParticleCollisionSFX(ParticleCollisionSFX sfx)
        {
            if (!Utilities.IsValid(sfx) || (sfx.audioSource != null && sfx.audioSource.gameObject == sfx.gameObject))
            {
                return;
            }
            SerializedObject serializedObject = new SerializedObject(sfx);
            serializedObject.FindProperty("particles").objectReferenceValue = sfx.GetComponent<ParticleSystem>();
            serializedObject.ApplyModifiedProperties();
        }
#endif
    }
}