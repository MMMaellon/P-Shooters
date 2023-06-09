
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
    public class P_ShootersPlayerHandler : UdonSharpBehaviour
    {
        [HideInInspector]
        public Player[] players;
        [System.NonSerialized]
        public Player localPlayer;
        public int startingMaxHealth = 100;
        public int startingMaxShield = 100;
        public int startingShield = 100;
        public int startingHealth = 100;
        public LayerMask meleeLayer;
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
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public static bool SetupPlayers()
        {
            Cyan.PlayerObjectPool.CyanPlayerObjectAssigner assigner = GameObject.FindObjectOfType<Cyan.PlayerObjectPool.CyanPlayerObjectAssigner>();
            assigner.GetComponent<Cyan.PlayerObjectPool.CyanPoolSetupHelper>().RespawnAllPoolObjects();
            Player[] players = GameObject.FindObjectsOfType<Player>();
            PlayerListener[] listeners = GameObject.FindObjectsOfType<PlayerListener>();
            P_ShootersPlayerHandler[] handlers = GameObject.FindObjectsOfType<P_ShootersPlayerHandler>();
            if (handlers.Length > 1)
            {
                Helper.ErrorLog(handlers[0],"Multiple P-Shooter Player Handlers found in scene. There should only be one.");
                return false;
            }
            if (handlers.Length == 0)
            {
                if (listeners.Length > 0)
                {
                    Helper.ErrorLog(listeners[0],"Player Listeners were found in scene, but no P-Shooters Player Handler was found. Listeners won't have any effect without any player handlers");
                    return true;
                }
                return true;
            }
            if ((listeners.Length > 0 || handlers.Length > 0) && players.Length == 0)
            {
                Helper.ErrorLog(handlers[0],"P-Shooter Player Handlers or Player Listeners found in scene, but no player objects were found. Did you forget to add a player object pool?");
                return true;
            }
            if (!Helper.IsEditable(handlers[0]))
            {
                Helper.ErrorLog(handlers[0],"P-Shooter Player Handler is not editable");
                return true;
            }
            SerializedObject handler = new SerializedObject(handlers[0]);
            handler.FindProperty("assigner").objectReferenceValue = assigner;
            if (handlers[0].players != players)
            {
                handler.FindProperty("players").ClearArray();
                for (int i = 0; i < players.Length; i++)
                {
                    handler.FindProperty("players").InsertArrayElementAtIndex(i);
                    handler.FindProperty("players").GetArrayElementAtIndex(i).objectReferenceValue = players[i];
                }
            } else {
                Helper.InfoLog(handlers[0], "Players already configured. Skipping AutoSetup.");
            }
            if (handlers[0].playerListeners != listeners)
            {
                handler.FindProperty("playerListeners").ClearArray();
                for (int i = 0; i < listeners.Length; i++)
                {
                    handler.FindProperty("playerListeners").InsertArrayElementAtIndex(i);
                    handler.FindProperty("playerListeners").GetArrayElementAtIndex(i).objectReferenceValue = listeners[i];
                }
            } else {
                Helper.InfoLog(handlers[0], "PlayerListeners already configured. Skipping AutoSetup.");
            }
            handler.ApplyModifiedProperties();
            foreach (Player player in players)
            {
                if (player.playerHandler == handlers[0] && player.capsuleCollider != null && player.capsuleCollider.gameObject == player.gameObject)
                {
                    continue;
                }
                if (!Helper.IsEditable(player))
                {
                    Helper.ErrorLog(player, "Player is not editable");
                    continue;
                }
                SerializedObject playerSerialized = new SerializedObject(player);
                playerSerialized.FindProperty("playerHandler").objectReferenceValue = handlers[0];
                playerSerialized.FindProperty("capsuleCollider").objectReferenceValue = player.GetComponent<CapsuleCollider>();
                playerSerialized.ApplyModifiedProperties();
            }
            foreach( PlayerListener listener in listeners)
            {
                if (listener.playerHandler == handlers[0])
                {
                    continue;
                }
                if (!Helper.IsEditable(listener))
                {
                    Helper.ErrorLog(listener, "Listener is not editable");
                    continue;
                }
                SerializedObject serializedListener = new SerializedObject(listener);
                serializedListener.FindProperty("playerHandler").objectReferenceValue = handlers[0];
                serializedListener.ApplyModifiedProperties();
            }

            ResourceManager[] resources = GameObject.FindObjectsOfType<ResourceManager>();
            if (resources.Length > 0)
            {
                int[] resourceIdMap;
                int[] reverseSyncResourceIdMap;
                int[] reverseLocalResourceIdMap;
                int[] syncedResources;
                int[] localResources;
                BuildResourceIdMap(out resourceIdMap, out reverseSyncResourceIdMap, out reverseLocalResourceIdMap, out syncedResources, out localResources, resources);

                for (int i = 0; i < resources.Length; i++)
                {
                    if (!Utilities.IsValid(resources[i]) || (resources[i].playerHandler == handlers[0] && resources[i].id == i))
                    {
                        //null or already set up
                        continue;
                    }
                    if (!Helper.IsEditable(resources[i]))
                    {
                        Helper.ErrorLog(resources[i], "Resource is not editable");
                        continue;
                    }
                    SerializedObject serialized = new SerializedObject(resources[i]);
                    serialized.FindProperty("playerHandler").objectReferenceValue = handlers[0];
                    serialized.FindProperty("id").intValue = i;
                    serialized.ApplyModifiedProperties();
                }

                foreach (Player player in players)
                {
                    if (!Helper.IsEditable(player))
                    {
                        // Helper.ErrorLog(player, "Player is not editable");
                        // Would have already printed this message so we skip printing it again
                        continue;
                    }
                    SerializedObject serialized = new SerializedObject(player);
                    if (player.resources != resources)
                    {
                        serialized.FindProperty("resources").ClearArray();
                        for (int i = 0; i < resources.Length; i++)
                        {
                            serialized.FindProperty("resources").InsertArrayElementAtIndex(i);
                            serialized.FindProperty("resources").GetArrayElementAtIndex(i).objectReferenceValue = resources[i];
                        }
                    }

                    if (player.resourceIdMap != resourceIdMap)
                    {
                        serialized.FindProperty("resourceIdMap").ClearArray();
                        for (int i = 0; i < resourceIdMap.Length; i++)
                        {
                            serialized.FindProperty("resourceIdMap").InsertArrayElementAtIndex(i);
                            serialized.FindProperty("resourceIdMap").GetArrayElementAtIndex(i).intValue = resourceIdMap[i];
                        }
                    }
                    if (player.reverseSyncResourceIdMap != reverseSyncResourceIdMap)
                    {
                        serialized.FindProperty("reverseSyncResourceIdMap").ClearArray();
                        for (int i = 0; i < reverseSyncResourceIdMap.Length; i++)
                        {
                            serialized.FindProperty("reverseSyncResourceIdMap").InsertArrayElementAtIndex(i);
                            serialized.FindProperty("reverseSyncResourceIdMap").GetArrayElementAtIndex(i).intValue = reverseSyncResourceIdMap[i];
                        }
                    }
                    if (player.reverseLocalResourceIdMap != reverseLocalResourceIdMap)
                    {
                        serialized.FindProperty("reverseLocalResourceIdMap").ClearArray();
                        for (int i = 0; i < reverseLocalResourceIdMap.Length; i++)
                        {
                            serialized.FindProperty("reverseLocalResourceIdMap").InsertArrayElementAtIndex(i);
                            serialized.FindProperty("reverseLocalResourceIdMap").GetArrayElementAtIndex(i).intValue = reverseLocalResourceIdMap[i];
                        }
                    }
                    if (player._syncedResources != syncedResources)
                    {
                        serialized.FindProperty("_syncedResources").ClearArray();
                        for (int i = 0; i < syncedResources.Length; i++)
                        {
                            serialized.FindProperty("_syncedResources").InsertArrayElementAtIndex(i);
                            serialized.FindProperty("_syncedResources").GetArrayElementAtIndex(i).intValue = syncedResources[i];
                        }
                    }
                    if (player._localResources != localResources)
                    {
                        serialized.FindProperty("_localResources").ClearArray();
                        for (int i = 0; i < localResources.Length; i++)
                        {
                            serialized.FindProperty("_localResources").InsertArrayElementAtIndex(i);
                            serialized.FindProperty("_localResources").GetArrayElementAtIndex(i).intValue = localResources[i];
                        }
                    }
                    serialized.ApplyModifiedProperties();
                }
            }

            Helper.InfoLog(handlers[0], "Configured " + players.Length + " Players, " + listeners.Length + " Player Listeners, and " + resources.Length + " Resources");
            return true;
        }

        public static void BuildResourceIdMap(out int[] resourceIdMap, out int[] reverseSyncResourceIdMap, out int[] reverseLocalResourceIdMap, out int[] syncedResources, out int[] localResources, ResourceManager[] resources = null)
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


#endif
    }
}