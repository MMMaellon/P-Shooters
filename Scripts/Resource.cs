
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
    [CustomEditor(typeof(Resource))]
    public class ResourceEditor : Editor
    {

        public static void SetupResource()
        {
            Player[] players = GameObject.FindObjectsOfType<Player>();
            Resource[] resources = GameObject.FindObjectsOfType<Resource>();
            
            if (players.Length == 0)
            {
                Debug.LogError("<color=red>[P-Shooter Resource AUTOSETUP]: FAILED</color> Could not find players. Please set up a player object pool");
                return;
            }
            for (int i = 0; i < resources.Length; i++)
            {
                SerializedObject serialized = new SerializedObject(resources[i]);
                serialized.FindProperty("id").intValue = i;
                serialized.ApplyModifiedProperties();
            }
            foreach (Player player in players)
            {
                SerializedObject serialized = new SerializedObject(player);
                serialized.FindProperty("resources").ClearArray();
                for (int i = 0; i < resources.Length; i++)
                {
                    serialized.FindProperty("resources").InsertArrayElementAtIndex(i);
                    serialized.FindProperty("resources").GetArrayElementAtIndex(i).objectReferenceValue = resources[i];
                }
                serialized.ApplyModifiedProperties();
            }
            Debug.Log("[P-Shooter Resource AUTOSETUP]: Configured " + resources.Length + " Resources");
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("How to use Resources");
            EditorGUILayout.HelpBox(
    @"1) Place this somewhere in your scene (Only one per resource)
2) Give the resource a unique name
3) Set up your player object pool. Make sure there's one in your scene and it has the right max player count
4) Name your resource. The name must be unique among resources.
4) Press the setup button below. If you change the number of players in your player pool, you will have to press the setup button again.

Resources are things like ammo or coins that is stored on the player object as a number.
You can make this number go up and down with the `ResourceChanger` script and compare the number to another number with the `ResourceChecker` script.
With these tools you can make very simple scoreboards, money systems, and more.

Warning:
Think VERY CAREFULLY about if you need the resource to be synced as that will add extra network overhead.
For example, you probably don't need to sync ammo or coins because you usually can't see other players inventories, but you might need to sync points if you plan to build a global leaderboard.
", MessageType.Info);

            EditorGUILayout.Space();

            if (GUILayout.Button(new GUIContent("Set up All Resources")))
            {
                ResourceEditor.SetupResource();
            }
            EditorGUILayout.Space();
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            EditorGUILayout.Space();
            base.OnInspectorGUI();
        }
    }
#endif
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Resource : UdonSharpBehaviour
    {
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


#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset()
        {
            ResourceEditor.SetupResource();
        }
#endif

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

        public void _OnIncrease(Player player, int value)
        {
            foreach (ResourceListener listener in listeners)
            {
                listener.OnIncrease(player, this, value);
            }
        }
        public void _OnDecrease(Player player, int value)
        {
            foreach (ResourceListener listener in listeners)
            {
                listener.OnDecrease(player, this, value);
            }
        }
        public void _OnMax(Player player, int value)
        {
            foreach (ResourceListener listener in listeners)
            {
                listener.OnMax(player, this, value);
            }
        }
        public void _OnMin(Player player, int value)
        {
            foreach (ResourceListener listener in listeners)
            {
                listener.OnMin(player, this, value);
            }
        }
    }
}
