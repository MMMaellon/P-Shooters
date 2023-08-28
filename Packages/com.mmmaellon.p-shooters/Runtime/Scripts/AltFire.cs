
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;
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
        SerializedProperty affectSFX;
        SerializedProperty affectAmmo;
        SerializedProperty rapidFireDamage;
        SerializedProperty altFireDamage;
        SerializedProperty altFireSFX;
        SerializedProperty rapidSFX;
        SerializedProperty rapidAmmo;
        SerializedProperty altFireEndIndexes;
        SerializedProperty altFireAmmo;
        bool[] SFXfoldouts;
        AudioClip[][] altFireSFX2d;

        void OnEnable()
        {
            // Fetch the objects from the MyScript script to display in the inspector
            altFireModeCount = serializedObject.FindProperty("altFireModeCount");
            affectDamage = serializedObject.FindProperty("affectDamage");
            affectSFX = serializedObject.FindProperty("affectSFX");
            affectAmmo = serializedObject.FindProperty("affectAmmo");
            rapidFireDamage = serializedObject.FindProperty("rapidFireDamage");
            altFireDamage = serializedObject.FindProperty("altFireDamage");
            altFireSFX = serializedObject.FindProperty("altFireSFX");
            rapidSFX = serializedObject.FindProperty("rapidSFX");
            rapidAmmo = serializedObject.FindProperty("rapidAmmo");
            altFireEndIndexes = serializedObject.FindProperty("altFireEndIndexes");
            altFireAmmo = serializedObject.FindProperty("altFireAmmo");
            AltFire rapid = (AltFire)target;
            Generate2dArray(rapid);
        }
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            AltFire rapid = (AltFire)target;
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            DrawPropertiesExcluding(serializedObject, "m_Script", "rapidFireDamage", "altFireDamage", "affectDamage", "altFireDamage", "affectSFX", "affectAmmo", "altFireSFX", "altFireAmmo", "altFireEndIndexes", "rapidSFX");
            EditorGUILayout.PropertyField(affectDamage, true);
            EditorGUILayout.PropertyField(affectSFX, true);
            EditorGUILayout.PropertyField(affectAmmo, true);
            if (affectDamage.boolValue || affectSFX.boolValue || affectAmmo.boolValue)
            {
                if (affectDamage.boolValue)
                {
                    EditorGUILayout.PropertyField(rapidFireDamage, true);
                }
                if (affectSFX.boolValue)
                {
                    EditorGUILayout.PropertyField(rapidSFX, true);
                }
                if (affectAmmo.boolValue)
                {
                    EditorGUILayout.PropertyField(rapidAmmo, true);
                }
                if (rapid.altFireDamage == null || rapid.altFireModeCount != rapid.altFireDamage.Length)
                {
                    int[] newArr = new int[rapid.altFireModeCount];
                    for (int i = 0; i < rapid.altFireModeCount; i++)
                    {
                        newArr[i] = rapid.altFireDamage != null && rapid.altFireDamage.Length > i ? rapid.altFireDamage[i] : rapid.shooter.damage;
                    }
                    rapid.altFireDamage = newArr;
                }
                if (SFXfoldouts == null || rapid.altFireModeCount != SFXfoldouts.Length)
                {
                    bool[] newArr = new bool[rapid.altFireModeCount];
                    for (int i = 0; i < rapid.altFireModeCount; i++)
                    {
                        newArr[i] = SFXfoldouts != null && SFXfoldouts.Length > i ? SFXfoldouts[i] : true;
                    }
                    SFXfoldouts = newArr;
                }
                if (rapid.altFireEndIndexes == null || rapid.altFireModeCount != rapid.altFireEndIndexes.Length)
                {
                    int[] newEndIndexes = new int[rapid.altFireModeCount];
                    for (int i = 0; i < rapid.altFireModeCount; i++)
                    {
                        newEndIndexes[i] = rapid.altFireEndIndexes != null && rapid.altFireEndIndexes.Length > i ? rapid.altFireEndIndexes[i] : rapid.altFireEndIndexes != null && rapid.altFireEndIndexes.Length > 0 ? rapid.altFireEndIndexes[rapid.altFireEndIndexes.Length - 1] : 0;
                    }
                    rapid.altFireEndIndexes = newEndIndexes;
                }
                if (rapid.altFireSFX == null || rapid.altFireSFX.Length != (rapid.altFireEndIndexes.Length > 0 ? rapid.altFireEndIndexes[rapid.altFireEndIndexes.Length - 1] : 0))
                {
                    AudioClip[] newArr = new AudioClip[rapid.altFireEndIndexes.Length > 0 ? rapid.altFireEndIndexes[rapid.altFireEndIndexes.Length - 1] : 0];
                    for (int i = 0; i < newArr.Length; i++)
                    {
                        newArr[i] = rapid.altFireSFX != null && rapid.altFireSFX.Length > i ? rapid.altFireSFX[i] : null;
                    }
                    rapid.altFireSFX = newArr;
                    Generate2dArray(rapid);
                }
                if (rapid.altFireAmmo == null || rapid.altFireModeCount != rapid.altFireAmmo.Length)
                {
                    AmmoTracker[] newArr = new AmmoTracker[rapid.altFireModeCount];
                    for (int i = 0; i < rapid.altFireModeCount; i++)
                    {
                        newArr[i] = rapid.altFireAmmo != null && rapid.altFireAmmo.Length > i ? rapid.altFireAmmo[i] : null;
                    }
                    rapid.altFireAmmo = newArr;
                }
                for (int i = 0; i < rapid.altFireModeCount; i++)
                {
                    SFXfoldouts[i] = EditorGUILayout.BeginFoldoutHeaderGroup(SFXfoldouts[i], $"AltFire {i}");
                    if (SFXfoldouts[i])
                    {
                        EditorGUI.indentLevel++;
                        if (affectDamage.boolValue)
                        {
                            rapid.altFireDamage[i] = EditorGUILayout.IntField($"Damage", rapid.altFireDamage[i]);
                        }
                        if (affectSFX.boolValue)
                        {
                            var startingClipCount = i == 0 ? rapid.altFireEndIndexes[i] : rapid.altFireEndIndexes[i] - rapid.altFireEndIndexes[i - 1];
                            var clipCount = EditorGUILayout.IntField($"SFX GunShot Clip Count", startingClipCount);
                            EditorGUI.indentLevel++;
                            if (clipCount < 0)
                            {
                                clipCount = startingClipCount;
                            }
                            else if (clipCount != startingClipCount)
                            {
                                AudioClip[] newArr = new AudioClip[clipCount - startingClipCount + rapid.altFireSFX.Length];
                                for (int j = 0; j < Mathf.Min(rapid.altFireSFX.Length, newArr.Length); j++)
                                {
                                    newArr[j] = rapid.altFireSFX[j];
                                }
                                for (int j = rapid.altFireEndIndexes[i]; j < rapid.altFireSFX.Length; j++)
                                {
                                    newArr[j + clipCount - startingClipCount] = rapid.altFireSFX[j];
                                }
                                if (clipCount > startingClipCount)
                                {
                                    for (int j = rapid.altFireEndIndexes[i]; j < rapid.altFireEndIndexes[i] + clipCount - startingClipCount; j++)
                                    {
                                        newArr[j] = null;
                                    }
                                }
                                for (int j = i; j < rapid.altFireEndIndexes.Length; j++)
                                {
                                    rapid.altFireEndIndexes[j] += clipCount - startingClipCount;
                                }
                                rapid.altFireSFX = newArr;
                                Generate2dArray(rapid);
                            }
                            if (altFireSFX2d != null && altFireSFX2d.Length > i)
                            {
                                for (int j = 0; j < altFireSFX2d[i].Length; j++)
                                {
                                    altFireSFX2d[i][j] = EditorGUILayout.ObjectField(altFireSFX2d[i][j], typeof(AudioClip), true) as AudioClip;
                                }
                            }
                            EditorGUI.indentLevel--;
                        }
                        if (affectAmmo.boolValue)
                        {
                            rapid.altFireAmmo[i] = EditorGUILayout.ObjectField($"AltFire Ammo {i}", rapid.altFireAmmo[i], typeof(AmmoTracker), true) as AmmoTracker;
                        }
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                SyncSerializedValues();
            }
        }

        public void Generate2dArray(AltFire rapid)
        {
            if (rapid.altFireEndIndexes == null || rapid.altFireSFX == null || rapid.altFireEndIndexes.Length != rapid.altFireModeCount)
            {
                return;
            }
            altFireSFX2d = new AudioClip[rapid.altFireModeCount][];
            int pointer = 0;
            for (int i = 0; i < rapid.altFireModeCount; i++)
            {
                if (i == 0)
                {
                    altFireSFX2d[i] = new AudioClip[rapid.altFireEndIndexes[i]];
                }
                else
                {
                    altFireSFX2d[i] = new AudioClip[rapid.altFireEndIndexes[i] - rapid.altFireEndIndexes[i - 1]];
                }
                for (int j = 0; j < altFireSFX2d[i].Length; j++)
                {
                    altFireSFX2d[i][j] = rapid.altFireSFX[pointer];
                    pointer++;
                }
            }
        }
        public void SyncSerializedValues()
        {
            AltFire rapid = (AltFire)target;
            //acting weird, so we need to manually set things
            altFireDamage.ClearArray();
            for (int i = 0; i < rapid.altFireDamage.Length; i++)
            {
                altFireDamage.InsertArrayElementAtIndex(i);
                altFireDamage.GetArrayElementAtIndex(i).intValue = rapid.altFireDamage[i];
            }
            altFireEndIndexes.ClearArray();
            if (rapid.altFireEndIndexes != null)
            {
                for (int i = 0; i < rapid.altFireEndIndexes.Length; i++)
                {
                    altFireEndIndexes.InsertArrayElementAtIndex(i);
                    altFireEndIndexes.GetArrayElementAtIndex(i).intValue = rapid.altFireEndIndexes[i];
                }
            }
            altFireSFX.ClearArray();
            if (altFireSFX2d == null)
            {
                if (rapid.altFireSFX != null)
                {
                    for (int i = 0; i < rapid.altFireSFX.Length; i++)
                    {
                        altFireSFX.InsertArrayElementAtIndex(i);
                        altFireSFX.GetArrayElementAtIndex(i).objectReferenceValue = rapid.altFireSFX[i];
                    }
                }
            }
            else
            {
                int pointer = 0;
                for (int i = 0; i < altFireSFX2d.Length; i++)
                {
                    for (int j = 0; j < altFireSFX2d[i].Length; j++)
                    {
                        altFireSFX.InsertArrayElementAtIndex(pointer);
                        altFireSFX.GetArrayElementAtIndex(pointer).objectReferenceValue = altFireSFX2d[i][j];
                        pointer++;
                    }
                }
            }
            altFireAmmo.ClearArray();
            if (rapid.altFireAmmo != null)
            {
                for (int i = 0; i < rapid.altFireAmmo.Length; i++)
                {
                    altFireAmmo.InsertArrayElementAtIndex(i);
                    altFireAmmo.GetArrayElementAtIndex(i).objectReferenceValue = rapid.altFireAmmo[i];
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
        public bool affectSFX = false;
        public bool affectAmmo = false;
        public int rapidFireDamage = 15;
        public int[] altFireDamage = new int[] { 15 };
        public AudioClip[] rapidSFX;
        public AmmoTracker rapidAmmo;
        public AudioClip[] altFireSFX;
        public int[] altFireEndIndexes;
        public AmmoTracker[] altFireAmmo;

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

        [System.NonSerialized]
        public int originalDamage;
        [System.NonSerialized]
        public AudioClip[] originalSFX = null;
        [System.NonSerialized]
        public AmmoTracker originalAmmo = null;
        public bool rapidFire
        {
            get => _rapidFire;
            set
            {
                _rapidFire = value;
                shooter.animator.SetBool("rapidfire", value);
                if (_altFire == 0)
                {
                    if (rapidFire)
                    {
                        if (affectDamage)
                        {
                            shooter.damage = rapidFireDamage;
                        }
                        if (affectSFX)
                        {
                            shooter.gunshots = rapidSFX;
                        }
                        if (affectAmmo)
                        {
                            shooter.ammo = rapidAmmo;
                        }
                    }
                    else if (originalSFX != null)
                    {
                        if (affectDamage)
                        {
                            shooter.damage = originalDamage;
                        }
                        if (affectSFX)
                        {
                            shooter.gunshots = originalSFX;
                        }
                        if (affectAmmo)
                        {
                            shooter.ammo = originalAmmo;
                        }
                    }
                }
                if (Networking.LocalPlayer.IsOwner(gameObject))
                {
                    RequestSerialization();
                }
            }
        }
        public int altFire
        {
            get => _altFire;
            set
            {
                _altFire = value;
                shooter.animator.SetInteger("altfire", value);
                if (_altFire == 0)
                {
                    rapidFire = rapidFire;
                }
                else if (_altFire > 0)
                {
                    if (affectDamage)
                    {
                        shooter.damage = altFireDamage[value - 1];
                    }
                    if (affectSFX)
                    {
                        Debug.LogWarning("setting altfire to " + value);
                        shooter.gunshots = new AudioClip[value == 1 ? altFireEndIndexes[0] : (altFireEndIndexes[value - 1] - altFireEndIndexes[value - 2])];
                        for (int i = 0; i < shooter.gunshots.Length; i++)
                        {
                            if (value == 1)
                            {
                                shooter.gunshots[i] = altFireSFX[i];
                            }
                            else
                            {
                                shooter.gunshots[i] = altFireSFX[i + altFireEndIndexes[value - 2]];
                            }
                        }
                    }
                    if (affectAmmo)
                    {
                        shooter.ammo = altFireAmmo[value - 1];
                    }
                }
                if (Networking.LocalPlayer.IsOwner(gameObject))
                {
                    RequestSerialization();
                }
            }
        }
        public void Start()
        {
            originalDamage = shooter.damage;
            originalSFX = (AudioClip[])shooter.gunshots.Clone();
            originalAmmo = shooter.ammo;
            rapidFire = startRapidFire;
            altFire = startAltFireMode;
        }

        public void ResetAltFire()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            rapidFire = rapidFire;
            altFire = altFire;
            RequestSerialization();
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
            else if (altFire < altFireModeCount)
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