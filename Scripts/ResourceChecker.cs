
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
#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(ResourceChecker))]
    public class ResourceCheckerEditor : Editor
    {
        public static void SetupChecker(ResourceChecker checker)
        {
            if (!Utilities.IsValid(checker))
            {
                Debug.LogError("<color=red>[P-Shooter Resource Checker AUTOSETUP]: FAILED</color> No Resource Checker Found");
                return;
            }
            Cyan.PlayerObjectPool.CyanPlayerObjectAssigner assigner = GameObject.FindObjectOfType<Cyan.PlayerObjectPool.CyanPlayerObjectAssigner>();
            if (!Utilities.IsValid(assigner))
            {
                Debug.LogError("<color=red>[P-Shooter Resource Checker AUTOSETUP]: FAILED</color> Could not find player object pool. Please set up a player object pool");
                return;
            }
            if (!Utilities.IsValid(assigner.GetComponentInChildren<Player>()))
            {
                Debug.LogError("<color=red>[P-Shooter Resource Checker AUTOSETUP]: FAILED</color> Could not find players in player object pool. Please make sure your player prefab uses a Player script on the root object");
                return;
            }
            SerializedObject serialized = new SerializedObject(checker);
            serialized.FindProperty("assigner").objectReferenceValue = assigner;
            serialized.ApplyModifiedProperties();
        }
        public override void OnInspectorGUI()
        {
            ResourceChecker checker = target as ResourceChecker;
            if (!Utilities.IsValid(checker))
            {
                return;
            }
            if (!Utilities.IsValid(checker.assigner))
            {
                EditorGUILayout.LabelField("Setup Required");
                EditorGUILayout.HelpBox(
@"Please set up a player object pool in the scene and then use the Setup button below
", MessageType.Info);

                EditorGUILayout.Space();

                if (GUILayout.Button(new GUIContent("Setup")))
                {
                    ResourceCheckerEditor.SetupChecker(target as ResourceChecker);
                }
                EditorGUILayout.Space();
            }
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            EditorGUILayout.Space();
            base.OnInspectorGUI();
        }
    }
#endif

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ResourceChecker : UdonSharpBehaviour
    {
        [System.NonSerialized]
        public int resourceId = -1001;
        public string resourceName;
        public bool passCheckIfEqual;
        public bool passCheckIfGreaterThan;
        public bool passCheckIfLessThan;
        public int checkValue;
        public UdonBehaviour passUdon;
        public string passUdonEvent;
        public UdonBehaviour failUdon;
        public string failUdonEvent;
        [HideInInspector]
        public Cyan.PlayerObjectPool.CyanPlayerObjectAssigner assigner;
        [System.NonSerialized]
        Player localPlayer;
        void Start()
        {
            
        }

        public void Check()
        {
            if (!Utilities.IsValid(localPlayer))
            {
                GameObject obj = assigner._GetPlayerPooledObject(Networking.LocalPlayer);
                if (!Utilities.IsValid(obj))
                {
                    return;
                }
                localPlayer = obj.GetComponent<Player>();
                if (!Utilities.IsValid(localPlayer))
                {
                    return;
                }
            }

            CheckByPlayer(localPlayer);
        }
        public void CheckByPlayer(Player player)
        {
            if (resourceId < 0)
            {
                resourceId = player.GetResourceId(resourceName);
            }
            if (resourceId >= 0)
            {
                int value = player.GetResourceValueById(resourceId);
                if (value == checkValue && passCheckIfEqual)
                {
                    PassCheck();
                }
                else if (value < checkValue && passCheckIfLessThan){
                    PassCheck();
                } else if (value > checkValue && passCheckIfGreaterThan)
                {
                    PassCheck();
                } else
                {
                    FailCheck();
                }
            }
        }

        public void PassCheck()
        {
            passUdon.SendCustomEvent(passUdonEvent);
        }
        public void FailCheck()
        {
            failUdon.SendCustomEvent(failUdonEvent);
        }
    }
}