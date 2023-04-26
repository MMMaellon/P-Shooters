
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
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual), RequireComponent(typeof(Animator))]
    public class RapidFire : UdonSharpBehaviour
    {
        public Animator animator;
        [UdonSynced, FieldChangeCallback(nameof(rapidFire))]
        public bool _rapidFire = true;
        [UdonSynced, FieldChangeCallback(nameof(altFire))]
        public bool _altFire = false;

        public void Start()
        {
            if (!Utilities.IsValid(animator))
            {
                animator = GetComponent<Animator>();
            }
            animator.SetBool("rapidfire", rapidFire);
            animator.SetBool("altfire", altFire);
        }
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset()
        {
            SerializedObject obj = new SerializedObject(this);
            obj.FindProperty("animator").objectReferenceValue = GetComponent<Animator>();
            obj.ApplyModifiedProperties();
        }
#endif

        public bool rapidFire
        {
            get => _rapidFire;
            set
            {
                _rapidFire = value;
                if (Utilities.IsValid(animator))
                {
                    animator.SetBool("rapidfire", value);
                }
            }
        }
        public bool altFire
        {
            get => _altFire;
            set
            {
                _altFire = value;
                if (Utilities.IsValid(animator))
                {
                    animator.SetBool("altfire", value);
                }
            }
        }

        public void ToggleAltFire()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            altFire = !altFire;
            RequestSerialization();
        }

        public void ToggleRapidFire()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            rapidFire = !rapidFire;
            RequestSerialization();
        }

        public void Cycle()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            //rapid -> alt -> single
            if (rapidFire && !altFire)
            {
                rapidFire = false;
                altFire = true;
            }
            else if (!rapidFire && altFire)
            {
                rapidFire = false;
                altFire = false;
            }
            else
            {
                rapidFire = true;
                altFire = false;
            }
            RequestSerialization();
        }
    }
}