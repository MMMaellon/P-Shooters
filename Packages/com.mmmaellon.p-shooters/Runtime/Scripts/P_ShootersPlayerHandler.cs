
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
    [CustomEditor(typeof(P_ShootersPlayerHandler))]
    public class P_ShootersPlayerHandlerEditor : Editor
    {
        public static bool SetupPlayers()
        {
            Cyan.PlayerObjectPool.CyanPlayerObjectAssigner assigner = GameObject.FindObjectOfType<Cyan.PlayerObjectPool.CyanPlayerObjectAssigner>();
            assigner.GetComponent<Cyan.PlayerObjectPool.CyanPoolSetupHelper>().RespawnAllPoolObjects();
            Player[] players = GameObject.FindObjectsOfType<Player>();
            PlayerListener[] listeners = GameObject.FindObjectsOfType<PlayerListener>();
            P_ShootersPlayerHandler[] handlers = GameObject.FindObjectsOfType<P_ShootersPlayerHandler>();
            if (handlers.Length > 1)
            {
                Debug.LogError("<color=red>[P-Shooter Player Handler AUTOSETUP]: FAILED</color> Multiple P-Shooter Player Handlers found in scene. There should only be one.");
                return false;
            }
            if (handlers.Length == 0)
            {
                if (listeners.Length > 0)
                {
                    Debug.LogError("<color=red>[P-Shooter Player Handler AUTOSETUP]: FAILED</color> Player Listeners were found in scene, but no P-Shooters Player Handler was found. Listeners won't have any effect without any player handlers");
                    return true;
                }
                return true;
            }
            if ((listeners.Length > 0 || handlers.Length > 0) && players.Length == 0)
            {
                Debug.LogError("<color=red>[P-Shooter Player Handler AUTOSETUP]: FAILED</color> P-Shooter Player Handlers or Player Listeners found in scene, but no player objects were found. Did you forget to add a player object pool?");
                return true;
            }

            SerializedObject handler = new SerializedObject(handlers[0]);
            handler.FindProperty("assigner").objectReferenceValue = assigner;
            handler.FindProperty("players").ClearArray();
            for (int i = 0; i < players.Length; i++)
            {
                handler.FindProperty("players").InsertArrayElementAtIndex(i);
                handler.FindProperty("players").GetArrayElementAtIndex(i).objectReferenceValue = players[i];
            }
            handler.FindProperty("playerListeners").ClearArray();
            for (int i = 0; i < listeners.Length; i++)
            {
                handler.FindProperty("playerListeners").InsertArrayElementAtIndex(i);
                handler.FindProperty("playerListeners").GetArrayElementAtIndex(i).objectReferenceValue = listeners[i];

                SerializedObject serializedListener = new SerializedObject(listeners[i]);
                serializedListener.FindProperty("playerHandler").objectReferenceValue = handlers[0];
                serializedListener.ApplyModifiedProperties();
            }
            handler.ApplyModifiedProperties();

            foreach (Player player in players)
            {
                SerializedObject playerSerialized = new SerializedObject(player);
                playerSerialized.FindProperty("playerHandler").objectReferenceValue = handlers[0];
                playerSerialized.ApplyModifiedProperties();
            }
            Debug.Log("[P-Shooter Player Handler AUTOSETUP]: Configured " + players.Length + " Players and " + listeners.Length + " Player Listeners");
            return true;
        }
        
        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        public static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.ExitingEditMode) return;
            SetupPlayers();
        }
        public int callbackOrder => 0;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            return SetupPlayers();
        }
    }
#endif
    public class P_ShootersPlayerHandler : UdonSharpBehaviour
    {
        [HideInInspector]
        public Player[] players;
        [System.NonSerialized]
        public Player localPlayer;
        public float respawnDamageCooldown;
        public PlayerListener[] playerListeners;
        public Cyan.PlayerObjectPool.CyanPlayerObjectAssigner assigner;
        void Start()
        {
            if (Utilities.IsValid(players))
            {
                for (int i = 0; i < players.Length; i++)
                {
                    players[i].id = i;
                }
            }
        }
    }
}