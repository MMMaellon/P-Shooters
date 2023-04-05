
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
    [CustomEditor(typeof(ResourceChanger))]
    public class ResourceChangerEditor : Editor
    {
        public static void SetupChanger(ResourceChanger changer)
        {
            if (!Utilities.IsValid(changer))
            {
                Debug.LogError("<color=red>[P-Shooter Resource Changer AUTOSETUP]: FAILED</color> No Resource Changer Found");
                return;
            }
            Cyan.PlayerObjectPool.CyanPlayerObjectAssigner assigner = GameObject.FindObjectOfType<Cyan.PlayerObjectPool.CyanPlayerObjectAssigner>();
            if (!Utilities.IsValid(assigner))
            {
                Debug.LogError("<color=red>[P-Shooter Resource Changer AUTOSETUP]: FAILED</color> Could not find player object pool. Please set up a player object pool");
                return;
            }
            if (!Utilities.IsValid(assigner.GetComponentInChildren<Player>()))
            {
                Debug.LogError("<color=red>[P-Shooter Resource Changer AUTOSETUP]: FAILED</color> Could not find players in player object pool. Please make sure your player prefab uses a Player script on the root object");
                return;
            }
            SerializedObject serialized = new SerializedObject(changer);
            serialized.FindProperty("assigner").objectReferenceValue = assigner;
            serialized.ApplyModifiedProperties();
        }
        public override void OnInspectorGUI()
        {
            ResourceChanger changer = target as ResourceChanger;
            if (!Utilities.IsValid(changer))
            {
                return;
            }
            if (!Utilities.IsValid(changer.assigner))
            {
                EditorGUILayout.LabelField("Setup Required");
                EditorGUILayout.HelpBox(
@"Please set up a player object pool in the scene and then use the Setup button below
", MessageType.Info);

                EditorGUILayout.Space();

                if (GUILayout.Button(new GUIContent("Setup")))
                {
                    ResourceChangerEditor.SetupChanger(target as ResourceChanger);
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
    public class ResourceChanger : UdonSharpBehaviour
    {
        [System.NonSerialized]
        public int resourceId = -1001;
        public string resourceName;
        [Tooltip("Check to increment and decrement the resource instead of setting it to a set number. Use a negative number as the value to make the resource decrement.")]
        public bool incrementByValue = true;
        public int value;
        [HideInInspector]
        public Cyan.PlayerObjectPool.CyanPlayerObjectAssigner assigner;
        [System.NonSerialized]
        Player localPlayer;
        void Start()
        {
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset()
        {
            ResourceChangerEditor.SetupChanger(this);
        }
#endif

        public void Change()
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
            ChangePlayer(localPlayer);
        }

        public void ChangePlayer(Player player)
        {
            if (incrementByValue)
            {
                if (Utilities.IsValid(player))
                {
                    if (resourceId < 0)
                    {
                        resourceId = player.GetResourceId(resourceName);
                    }
                    player.ChangeResourceValueById(resourceId, value);
                }
            } else
            {
                if (Utilities.IsValid(player))
                {
                    if (resourceId < 0)
                    {
                        resourceId = player.GetResourceId(resourceName);
                    }
                    player.SetResourceValueById(resourceId, value);
                }
            }
        }
    }
}
