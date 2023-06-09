
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
    public class HealthAndShieldChanger : UdonSharpBehaviour
    {
        public bool affectInvincibilePlayers = false;
        public bool affectHealth = true;
        public bool affectShield = true;
        public bool adjustDamageWithPlayerListeners = true;
        public bool incrementByValue = true;
        [Tooltip("When incrementByValue is true, positive values heal players, negative values damage players. Otherwise, it just sets the value")]
        public int value;
        [HideInInspector]
        public P_ShootersPlayerHandler playerHandler;
        [System.NonSerialized]
        Player localPlayer;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset()
        {
            SetupHealthAndShieldChanger(this);
        }
        public static void SetupHealthAndShieldChanger(HealthAndShieldChanger healthAndShield)
        {
            if (!Utilities.IsValid(healthAndShield) || (Utilities.IsValid(healthAndShield.playerHandler)))
            {
                //was null or was already set up
                return;
            }
            if (!Helper.IsEditable(healthAndShield))
            {
                Helper.ErrorLog(healthAndShield, "Health And Shield Changer is not editable");
                return;
            }
            P_ShootersPlayerHandler playerHandler = GameObject.FindObjectOfType<P_ShootersPlayerHandler>();
            if (!Utilities.IsValid(playerHandler))
            {
                Helper.ErrorLog(healthAndShield, "Could not find a P-Shooters Player Handler in the scene");
                return;
            }
            SerializedObject serialized = new SerializedObject(healthAndShield);
            serialized.FindProperty("playerHandler").objectReferenceValue = playerHandler;
            serialized.ApplyModifiedProperties();
        }
#endif

        public void Change()
        {
            ChangePlayer(playerHandler.localPlayer);
        }

        int damage;
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
            if (incrementByValue)
            {
                damage = value;
                if (adjustDamageWithPlayerListeners)
                {
                    damage = player.AdjustDamage(damage);
                }
                if (damage > 0)
                {
                    player.lastHealer = player;
                }
                else
                {
                    player.lastAttacker = player;
                }
                if (affectHealth && affectShield)
                {
                    if (damage > 0)
                    {
                        player.ReceiveHealth(damage, false);
                    }
                    else
                    {
                        player.ReceiveDamage(-damage, false);
                    }
                }
                else if (affectShield)
                {
                    player.shield += damage;
                }
                else if (affectHealth)
                {
                    player.health += damage;
                }
            } else
            {
                player.lastHealer = player;
                player.lastAttacker = player;
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
