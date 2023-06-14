
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

namespace MMMaellon.P_Shooters
{
#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(AltFire))]
    public class AltFireEditor : Editor
    {
        SerializedProperty altFireModeCount;
        SerializedProperty affectDamage;
        SerializedProperty rapidFireDamage;
        SerializedProperty altFireDamage;

        void OnEnable()
        {
            // Fetch the objects from the MyScript script to display in the inspector
            altFireModeCount = serializedObject.FindProperty("altFireModeCount");
            affectDamage = serializedObject.FindProperty("affectDamage");
            rapidFireDamage = serializedObject.FindProperty("rapidFireDamage");
            altFireDamage = serializedObject.FindProperty("altFireDamage");
        }
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            AltFire rapid = (AltFire)target;
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            DrawPropertiesExcluding(serializedObject, "m_Script", "rapidFireDamage", "altFireDamage", "affectDamage", "rapidFireDamage", "altFireDamage");
            EditorGUILayout.PropertyField(affectDamage, true);
            if (affectDamage.boolValue)
            {
                EditorGUILayout.PropertyField(rapidFireDamage, true);
                // EditorGUILayout.PropertyField(altFireDamage, true);
                // Loop over each item in the array and draw an IntField for it
                for (int i = 0; i < rapid.altFireDamage.Length; i++)
                {
                    rapid.altFireDamage[i] = EditorGUILayout.IntField($"AltFire Damage {i}", rapid.altFireDamage[i]);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                SyncSerializedValues();
            }
        }
        public void SyncSerializedValues()
        {
            AltFire rapid = (AltFire)target;
            if (altFireModeCount.intValue > altFireDamage.arraySize)
            {
                int[] newDamage = new int[altFireModeCount.intValue];
                for (int i = altFireDamage.arraySize; i < newDamage.Length; i++)
                {
                    altFireDamage.InsertArrayElementAtIndex(i);
                    altFireDamage.GetArrayElementAtIndex(i).intValue = rapid.shooter.damage;
                }
            }
            else
            {
                while (altFireDamage.arraySize > 0 && altFireDamage.arraySize > altFireModeCount.intValue)
                {
                    altFireDamage.DeleteArrayElementAtIndex(altFireDamage.arraySize - 1);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual), RequireComponent(typeof(P_Shooter))]
    public class AltFire : UdonSharpBehaviour
    {
        public bool startRapidFire = true;
        public int startAltFireMode = 0;
        [HideInInspector]
        public P_Shooter shooter;
        [HideInInspector, UdonSynced, FieldChangeCallback(nameof(rapidFire))]
        public bool _rapidFire = true;
        [HideInInspector, UdonSynced, FieldChangeCallback(nameof(altFire))]
        public int _altFire = 0;
        [Tooltip("Set to 0 to disable")]
        public int altFireModeCount = 0;
        public bool affectDamage = false;
        public int rapidFireDamage = 15;
        public int[] altFireDamage = new int[] { 15 };

        public void OnEnable()
        {
            shooter.animator.SetBool("rapidfire", rapidFire);
            shooter.animator.SetInteger("altfire", altFire);
        }
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset()
        {
            SetupRapidFire(this);
        }

        public static void SetupRapidFire(AltFire rapid)
        {
            if (!Utilities.IsValid(rapid) || (Utilities.IsValid(rapid.shooter) && rapid.shooter.gameObject == rapid.gameObject))
            {
                //null or already setup
                return;
            }
            if (!Helper.IsEditable(rapid))
            {
                Helper.ErrorLog(rapid, "RapidFire is not editable");
                return;
            }
            SerializedObject obj = new SerializedObject(rapid);
            obj.FindProperty("shooter").objectReferenceValue = rapid.GetComponent<P_Shooter>();
            obj.ApplyModifiedProperties();
        }
#endif

        public bool rapidFire
        {
            get => _rapidFire;
            set
            {
                _rapidFire = value;
                shooter.animator.SetBool("rapidfire", value);
            }
        }
        public int altFire
        {
            get => _altFire;
            set
            {
                _altFire = value;
                shooter.animator.SetInteger("altfire", value);
            }
        }

        public void ToggleAltFire()
        {
            if (altFireModeCount <= 0)
            {
                return;
            }
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            altFire = (altFire + 1) % (altFireModeCount + 1);
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
            if (!rapidFire)
            {
                rapidFire = true;
                altFire = 0;
            }
            else if (altFire == 0)
            {
                ToggleAltFire();
            }
            else
            {
                ToggleAltFire();
                if (altFire == 0)
                {
                    rapidFire = false;
                }
            }
            RequestSerialization();
        }
    }
}