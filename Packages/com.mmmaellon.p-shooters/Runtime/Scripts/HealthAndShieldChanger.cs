
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
    [CustomEditor(typeof(HealthAndShieldChanger))]
    public class HealthAndShieldChangerEditor : Editor
    {
        public static void SetupHealthAndShieldChanger(HealthAndShieldChanger healthAndShield)
        {
            if (!Utilities.IsValid(healthAndShield))
            {
                Debug.LogError("<color=red>[P-Shooter Damage And Heal AUTOSETUP]: FAILED</color> No Damage And Heal Found");
                return;
            }
            Cyan.PlayerObjectPool.CyanPlayerObjectAssigner assigner = GameObject.FindObjectOfType<Cyan.PlayerObjectPool.CyanPlayerObjectAssigner>();
            if (!Utilities.IsValid(assigner))
            {
                Debug.LogError("<color=red>[P-Shooter Damage And Heal AUTOSETUP]: FAILED</color> Could not find player object pool. Please set up a player object pool");
                return;
            }
            if (!Utilities.IsValid(assigner.GetComponentInChildren<Player>()))
            {
                Debug.LogError("<color=red>[P-Shooter Damage And Heal AUTOSETUP]: FAILED</color> Could not find players in player object pool. Please make sure your player prefab uses a Player script on the root object");
                return;
            }
            SerializedObject serialized = new SerializedObject(healthAndShield);
            serialized.FindProperty("assigner").objectReferenceValue = assigner;
            serialized.ApplyModifiedProperties();
        }
        public override void OnInspectorGUI()
        {
            HealthAndShieldChanger healthAndShield = target as HealthAndShieldChanger;
            if (!Utilities.IsValid(healthAndShield))
            {
                return;
            }
            if (!Utilities.IsValid(healthAndShield.assigner))
            {
                EditorGUILayout.LabelField("Setup Required");
                EditorGUILayout.HelpBox(
@"Please set up a player object pool in the scene and then use the Setup button below
", MessageType.Info);

                EditorGUILayout.Space();

                if (GUILayout.Button(new GUIContent("Set up")))
                {
                    HealthAndShieldChangerEditor.SetupHealthAndShieldChanger(target as HealthAndShieldChanger);
                }
                EditorGUILayout.Space();
            }
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            EditorGUILayout.Space();
            base.OnInspectorGUI();
        }
    }
#endif

    public class HealthAndShieldChanger : UdonSharpBehaviour
    {
        public bool affectInvincibilePlayers = false;
        public bool affectHealth = true;
        public bool affectShield = true;
        public bool incrementByValue = true;
        [Tooltip("When incrementByValue is true, positive values heal players, negative values damage players. Otherwise, it just sets the value")]
        public int value;
        [HideInInspector]
        public Cyan.PlayerObjectPool.CyanPlayerObjectAssigner assigner;
        [System.NonSerialized]
        Player localPlayer;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset()
        {
            HealthAndShieldChangerEditor.SetupHealthAndShieldChanger(this);
        }
#endif
        void Start()
        {
        }

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
            if (!Utilities.IsValid(player))
            {
                return;
            }
            if (!affectInvincibilePlayers && !player.CanTakeDamage())
            {
                return;
            }
            if (value > 0)
            {
                player.lastHealer = player;
            } else
            {
                player.lastAttacker = player;
            }
            if (incrementByValue)
            {
                if (affectHealth && affectShield)
                {
                    if (value > 0)
                    {
                        player.ReceiveHealth(value, false);
                    }
                    else
                    {
                        player.ReceiveDamage(-value, false);
                    }
                }
                else if (affectShield)
                {
                    player.shield += value;
                }
                else if (affectHealth)
                {
                    player.health += value;
                }
            } else
            {
                if (affectHealth && affectShield)
                {
                    player.health = value;
                    player.shield = value - player.maxHealth;
                }
                else if (affectShield)
                {
                    player.shield = value;
                }
                else if (affectHealth)
                {
                    player.health = value;
                }
            }
        }
    }
}
