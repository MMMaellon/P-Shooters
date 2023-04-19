
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
    [CustomEditor(typeof(GunManager))]
    public class GunManagerEditor : Editor
    {

        public static void SetupGuns()
        {
            int count = 0;
            GunManager manager = null;
            foreach (GunManager go in GameObject.FindObjectsOfType(typeof(GunManager)) as GunManager[])
            {
                if (go != null)
                {
                    manager = go;
                    break;
                }
            }
            if (manager == null)
            {
                Debug.LogError($"[<color=#FF00FF>UdonSharp</color>] Could not find a Gun Manager. Did you drag the prefab into your scene?");
                return;
            }

            SerializedObject serializedManager = new SerializedObject(manager);
            serializedManager.FindProperty("shooters").ClearArray();
            serializedManager.ApplyModifiedProperties();
            foreach (P_Shooter shooter in GameObject.FindObjectsOfType(typeof(P_Shooter)) as P_Shooter[])
            {
                int index = serializedManager.FindProperty("shooters").arraySize;
                serializedManager.FindProperty("shooters").InsertArrayElementAtIndex(index);
                serializedManager.ApplyModifiedProperties();
                serializedManager.FindProperty("shooters").GetArrayElementAtIndex(index).objectReferenceValue = shooter;
                serializedManager.ApplyModifiedProperties();
                count++;
            }
            if (count > 0)
            {
                Debug.Log($"Set up {count} guns");
            }
            else
            {
                Debug.Log($"No guns were set up");
            }
        }

        public static void SetupLayers()
        {
            UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if ((asset != null) && (asset.Length > 0))
            {
                SerializedObject serializedObject = new SerializedObject(asset[0]);
                SerializedProperty layers = serializedObject.FindProperty("layers");
                while (layers.arraySize < 26)
                {
                    layers.InsertArrayElementAtIndex(layers.arraySize);
                }
                layers.GetArrayElementAtIndex(22).stringValue = "PlayerCollider";
                layers.GetArrayElementAtIndex(23).stringValue = "Upgrades";
                layers.GetArrayElementAtIndex(24).stringValue = "SafeZone";
                layers.GetArrayElementAtIndex(25).stringValue = "Damage";


                for (int i = 0; i < 26; i++)
                {
                    Physics.IgnoreLayerCollision(i, 22, true);
                    Physics.IgnoreLayerCollision(i, 23, true);
                    Physics.IgnoreLayerCollision(i, 24, true);
                    Physics.IgnoreLayerCollision(i, 25, true);
                }
                Physics.IgnoreLayerCollision(22, 23, false);
                Physics.IgnoreLayerCollision(22, 24, false);
                Physics.IgnoreLayerCollision(22, 25, false);

                serializedObject.ApplyModifiedProperties();
            }
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("GUNS SETUP");
            EditorGUILayout.HelpBox(
    @"1) Drag a 'gun_pickup' prefab into your scene. There are many kinds in the '__GUN PICKUP PREFABS__' folder

2) Add a custom gun mesh to the 'model' object and line it up so the center of the 'model' object is near the center of gravity of your mesh.

3) Line up the shooter object to the barrel of your gun.

4) Under the 'model' object. Line up the ammo light (light that comes on when out of ammo), the grip position (where your hand snaps to when picking up the gun), and the mag parent (center of gravity of the magazine) with the appropriate points on your mesh.

5) Drag the mesh for the mag onto the object called 'place_mag_mesh_here' on the 'mag_parent' object for the reload animation.

6) Edit values on the P_Shooter object to control the damage, mag size, and SFX.

7) Change the box collider size on the root of the prefab to cover your entire gun. This makes it easier to grab in VRChat.

8) Press the setup button below", MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("SCOPES SETUP");
            EditorGUILayout.HelpBox(
    @"1) Drag a 'scope_manager' prefab into your scene. If you used the prefab, skip this step. There's already one that's a child of this object.

2) Drag 'scope' prefabs onto the root of your gun prefab.

3) Line up the scope object so that it's centered and touching the lens on your scope or the iron sights if your mesh doesn't have a scope.

4) For scopes with a zoom lens, make sure to the ZoomScopeLens material is applied to lens of the scope and that you set the zoom level on the scope object

5) For red/green dot sights, make sure the Stencil object is covering the lens or that another shader covering the lens is writing '69' to the stencil layer.

6) (Optional) If there's a scope mesh on your gun you can define it in the 'lens_mesh' field. Defining the lens_mesh will turn off that mesh when the gun is not being held. The camera is still rendering, so it doesn't save that much performance, but you do save at least one draw call.

7) Press the setup button below or on the scope manager object", MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("SECONDARY GRIPS SETUP");
            EditorGUILayout.HelpBox(
    @"1) Drag 'secondary_grip' prefabs onto your guns. It should be a child of the object with the 'VRC_Pickup' component.
2) Press the setup button below or on the secondary grip objects", MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("PLAYERS SETUP");
            EditorGUILayout.HelpBox(
    @"1) Drag a 'players' prefab into your scene. If you used the prefab, skip this step. There's already one that's a child of this object.

2) Set the health and shield and the safety layers and the instakill layers

3) Set the hard cap max number of players on the 'PlayerObjectPool' Object.

4) Press the setup button below or on the players object", MessageType.Info);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (GUILayout.Button(new GUIContent("Set up ALL Guns")))
            {
                GunManagerEditor.SetupGuns();
            }
            if (GUILayout.Button(new GUIContent("Set up ALL Scopes")))
            {
                ScopeManagerEditor.SetupScopes();
            }
            if (GUILayout.Button(new GUIContent("Set up ALL Grips")))
            {
                SecondGripEditor.SetupGrips();
            }
            if (GUILayout.Button(new GUIContent("Set up Players")))
            {
                PlayerHandlerEditor.SetupPlayers();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("COLLIDER LAYERS SETUP");
            EditorGUILayout.HelpBox(
    @"This prefab puts player colliders on layer 22, upgrade colliders on layer 23, safe zone colliders on 24, and weapon colliders on layer 25

The button below edits the collision matrix so these layers only interact with eachother.
It is VERY important that these layers do not collide with the player (layer 13) otherwise you will be yeeted into space.", MessageType.Info);
            if (GUILayout.Button(new GUIContent("Setup Collisions on Layers 22-25")))
            {
                GunManagerEditor.SetupLayers();
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            EditorGUILayout.Space();
            base.OnInspectorGUI();
        }
    }
#endif

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class GunManager : UdonSharpBehaviour
    {
        [Tooltip("All guns need to be here. Pro-tip: Clear this array by setting 'Size' to 0. Lock this window (by clicking the lock in the top right). Type 'P_Shooter' into heirarchy searchbar. Drag all results onto the word 'Shooters'.")]
        public P_Shooter[] shooters;
        [Tooltip("All spawn points need to be here. Pro-tip: Clear this array by setting 'Size' to 0. Lock this window (by clicking the lock in the top right). Type 'GunSpawn' into heirarchy searchbar. Drag all results onto the word 'Spawn Points'.")]
        private GunSpawn[] spawnPoints;

        public PlayerHandler player_handler;
        public ScopeManager scope_manager;

        private int[] free_spawns;
        void Start()
        {
            if (spawnPoints != null)
            {
                free_spawns = new int[spawnPoints.Length];
            }
            if (player_handler != null)
            {
                player_handler._Register(this);
            }
            for (int i = 0; i < shooters.Length; i++)
            {
                shooters[i]._Register(this, i);
            }
            if (scope_manager != null)
            {
                scope_manager._Register(this);
            }
        }

        public void RandomGunSpawn(P_Shooter shooter)
        {
            int free_count = 0;
            float total_chance = 0;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (!spawnPoints[i].occupied)
                {
                    free_spawns[free_count] = i;
                    free_count++;
                    if (shooter.rare)
                    {
                        total_chance += spawnPoints[i].rare_spawn_chance;
                    }
                    else
                    {
                        total_chance += spawnPoints[i].spawn_chance;
                    }
                }
            }

            if (free_count > 0 && total_chance > 0)
            {
                bool spawned = false;
                float random = Random.Range(0f, total_chance);
                Debug.LogWarning("Free Count: " + free_count);
                Debug.LogWarning("Random Selected: " + random + " / " + total_chance);
                for (int i = 0; i < free_count; i++)
                {
                    GunSpawn spawn = spawnPoints[free_spawns[i]];
                    if (shooter.rare)
                    {
                        random -= spawn.rare_spawn_chance;
                    }
                    else
                    {
                        random -= spawn.spawn_chance;
                    }
                    if (random <= 0)
                    {
                        spawned = true;
                        SpawnGun(shooter, free_spawns[i]);
                        break;
                    }
                }

                if (!spawned)
                {
                    SpawnGun(shooter, free_spawns[Random.Range(0, free_count)]);
                }
            }
        }

        public void SpawnGun(P_Shooter shooter, int spawn_index)
        {
            GunSpawn spawn = spawnPoints[free_spawns[spawn_index]];
            spawn.SetOccupied(true);
            shooter._TakeOwnership();
            shooter.SetSpawnId(spawn_index);
            Rigidbody rigid = (Rigidbody)shooter.transform.parent.transform.GetComponent(typeof(Rigidbody));
            rigid.MovePosition(spawn.transform.position);
            rigid.MoveRotation(spawn.transform.rotation);
            rigid.transform.position = spawn.transform.position;
            rigid.transform.rotation = spawn.transform.rotation;
        }

        public void ClearSpawn(int spawn_id)
        {
            if (spawn_id >= 0 && spawn_id < spawnPoints.Length)
            {
                spawnPoints[spawn_id].SetOccupied(false);
            }
        }
    }
}