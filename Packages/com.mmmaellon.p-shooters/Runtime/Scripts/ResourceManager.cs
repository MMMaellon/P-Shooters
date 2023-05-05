
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
    [CustomEditor(typeof(ResourceManager))]
    public class ResourceManagerEditor : Editor
    {

        public static bool SetupResources()
        {
            Player[] players = GameObject.FindObjectsOfType<Player>();
            resources = GameObject.FindObjectsOfType<ResourceManager>();
            BuildResourceIdMap();
            Cyan.PlayerObjectPool.CyanPlayerObjectAssigner sceneAssigner = GameObject.FindObjectOfType<Cyan.PlayerObjectPool.CyanPlayerObjectAssigner>();
            if (!Utilities.IsValid(sceneAssigner))
            {
                Debug.LogError("<color=red>[P-Shooter Resource AUTOSETUP]: FAILED</color> Could not find Cyan.PlayerObjectPool.CyanPlayerObjectAssigner object. Please set up a player object pool");
                return false;
            }

            if (players.Length == 0)
            {
                Debug.LogError("<color=red>[P-Shooter Resource AUTOSETUP]: FAILED</color> Could not find players. Please set up a player object pool");
                return false;
            }
            for (int i = 0; i < resources.Length; i++)
            {
                if (EditorUtility.IsPersistent(resources[i].transform.root.gameObject) || resources[i].hideFlags == HideFlags.NotEditable || resources[i].hideFlags == HideFlags.HideAndDontSave)
                {
                    continue;
                }
                SerializedObject serialized = new SerializedObject(resources[i]);
                serialized.FindProperty("assigner").objectReferenceValue = sceneAssigner;
                serialized.FindProperty("id").intValue = i;
                serialized.ApplyModifiedProperties();
            }
            foreach (Player player in players)
            {
                if (EditorUtility.IsPersistent(player.transform.root.gameObject) || player.hideFlags == HideFlags.NotEditable || player.hideFlags == HideFlags.HideAndDontSave)
                {
                    continue;
                }
                SerializedObject serialized = new SerializedObject(player);
                serialized.FindProperty(nameof(resources)).ClearArray();
                for (int i = 0; i < resources.Length; i++)
                {
                    serialized.FindProperty(nameof(resources)).InsertArrayElementAtIndex(i);
                    serialized.FindProperty(nameof(resources)).GetArrayElementAtIndex(i).objectReferenceValue = resources[i];
                }

                serialized.FindProperty(nameof(resourceIdMap)).ClearArray();
                for (int i = 0; i < resourceIdMap.Length; i++)
                {
                    serialized.FindProperty(nameof(resourceIdMap)).InsertArrayElementAtIndex(i);
                    serialized.FindProperty(nameof(resourceIdMap)).GetArrayElementAtIndex(i).intValue = resourceIdMap[i];
                }
                serialized.FindProperty(nameof(reverseSyncResourceIdMap)).ClearArray();
                for (int i = 0; i < reverseSyncResourceIdMap.Length; i++)
                {
                    serialized.FindProperty(nameof(reverseSyncResourceIdMap)).InsertArrayElementAtIndex(i);
                    serialized.FindProperty(nameof(reverseSyncResourceIdMap)).GetArrayElementAtIndex(i).intValue = reverseSyncResourceIdMap[i];
                }
                serialized.FindProperty(nameof(reverseLocalResourceIdMap)).ClearArray();
                for (int i = 0; i < reverseLocalResourceIdMap.Length; i++)
                {
                    serialized.FindProperty(nameof(reverseLocalResourceIdMap)).InsertArrayElementAtIndex(i);
                    serialized.FindProperty(nameof(reverseLocalResourceIdMap)).GetArrayElementAtIndex(i).intValue = reverseLocalResourceIdMap[i];
                }
                serialized.FindProperty("_syncedResources").ClearArray();
                for (int i = 0; i < syncedResources.Length; i++)
                {
                    serialized.FindProperty("_syncedResources").InsertArrayElementAtIndex(i);
                    serialized.FindProperty("_syncedResources").GetArrayElementAtIndex(i).intValue = syncedResources[i];
                }
                serialized.FindProperty("_localResources").ClearArray();
                for (int i = 0; i < localResources.Length; i++)
                {
                    serialized.FindProperty("_localResources").InsertArrayElementAtIndex(i);
                    serialized.FindProperty("_localResources").GetArrayElementAtIndex(i).intValue = localResources[i];
                }
                
                serialized.ApplyModifiedProperties();
            }
            Debug.Log("[P-Shooter Resource AUTOSETUP]: Configured " + resources.Length + " Resources");
            return true;
        }
        public static ResourceManager[] resources;
        [System.NonSerialized]
        public static int[] resourceIdMap;
        [System.NonSerialized]
        public static int[] reverseSyncResourceIdMap;
        [System.NonSerialized]
        public static int[] reverseLocalResourceIdMap;
        public static int[] syncedResources;
        public static int[] localResources;
        public static void BuildResourceIdMap()
        {
            int syncedRCount = 0;
            int localRCount = 0;
            resourceIdMap = new int[resources.Length];
            for (int i = 0; i < resources.Length; i++)
            {
                if (resources[i].synced)
                {
                    resourceIdMap[i] = syncedRCount;
                    syncedRCount++;
                }
                else
                {
                    resourceIdMap[i] = localRCount;
                    localRCount++;
                }
            }
            reverseSyncResourceIdMap = new int[syncedRCount];
            syncedResources = new int[syncedRCount];
            reverseLocalResourceIdMap = new int[localRCount];
            localResources = new int[localRCount];

            syncedRCount = 0;
            localRCount = 0;
            for (int i = 0; i < resources.Length; i++)
            {
                if (resources[i].synced)
                {
                    reverseSyncResourceIdMap[syncedRCount] = i;
                    syncedRCount++;
                }
                else
                {
                    reverseLocalResourceIdMap[localRCount] = i;
                    localRCount++;
                }
            }
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("How to use Resources");
            EditorGUILayout.HelpBox(
    @"1) Place this somewhere in your scene (Only one per resource type)
2) Give the resource a unique name
3) Set up your player object pool. Make sure there's one in your scene and it has the appropriate max player count and that it has a P-ShootersPlayerHandler on it somewhere.
4) Name your resource. The name must be unique among resources.

Resources are things like ammo or coins that is stored on the player object as a number.
You can make this number go up and down with the `ResourceChanger` script and compare the number to another number with the `ResourceChecker` script.
With these tools you can make very simple scoreboards, money systems, and more.

Warning:
Think VERY CAREFULLY about if you need the resource to be synced as that will add extra network overhead.
For example, you probably don't need to sync ammo or coins because you usually can't see other players inventories, but you might need to sync points if you plan to build a global leaderboard.
", MessageType.Info);

            EditorGUILayout.Space();

            // if (GUILayout.Button(new GUIContent("Set up All Resources")))
            // {
            //     ResourceManagerEditor.SetupResources();
            // }
            EditorGUILayout.Space();
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            EditorGUILayout.Space();
            base.OnInspectorGUI();
        }

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.ExitingEditMode) return;
            SetupResources();
        }

        public int callbackOrder => 0;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            return SetupResources();
        }
    }
#endif
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ResourceManager : UdonSharpBehaviour
    {
        public Cyan.PlayerObjectPool.CyanPlayerObjectAssigner assigner;
        public bool synced;
        public string resourceName;
        [System.NonSerialized]
        public int defaultValue = 0;
        public int maxValue = 100;
        public int minValue = 0;
        public bool allowOverflow = false;
        public ResourceListener[] listeners;
        [Tooltip("When checked, the players' stats animator and event animator will receive stats and events for this resource. The events the event animator receives will be trigger parameters with names like \"OnIncrease____\", \"OnDecrease____\", \"OnMax____\", \"OnMin____\" where the blanks are this resource's name")]
        public bool setAnimationParameterOnPlayers = false;
        [Tooltip("When unchecked a integer parameter on the players' stat animator with this resources name gets set to the value of the resource on that player. When checked, a float parameter is set instead and the value is the resource value as a ratio where 0.0 is the minimum value and 1.0 is the maximum value")]
        public bool setAnimationParameterAsRatio = true;

        [HideInInspector]
        public int id = -1;


        [System.NonSerialized, FieldChangeCallback(nameof(localPlayerObject))]
        public Player _localPlayerObject;
        public Player localPlayerObject
        {
            get
            {
                if (!Utilities.IsValid(_localPlayerObject))
                {
                    GameObject playerObj = assigner._GetPlayerPooledObject(Networking.LocalPlayer);
                    if (!Utilities.IsValid(playerObj))
                    {
                        return _localPlayerObject;
                    }
                    _localPlayerObject = playerObj.GetComponent<Player>();
                }
                return _localPlayerObject;
            }
            set
            {
                _localPlayerObject = value;
            }
        }

        public void _Register(int newId)
        {
            id = newId;
        }

        public int GetValue(Player player)
        {
            return player.GetResourceValueById(id);
        }

        public void SetValue(Player player, int value)
        {
            player.SetResourceValueById(id, value);
        }

        public void ChangeValue(Player player, int change)
        {
            player.SetResourceValueById(id, player.GetResourceValueById(id) + change);
        }

        public void ResetValue(Player player)
        {
            player.SetResourceValueById(id, defaultValue);
        }

        public void OnChange(Player player, int oldValue, int newValue)
        {
            foreach (ResourceListener listener in listeners)
            {
                listener.OnChange(player, this, oldValue, newValue);
            }
        }
    }
}
