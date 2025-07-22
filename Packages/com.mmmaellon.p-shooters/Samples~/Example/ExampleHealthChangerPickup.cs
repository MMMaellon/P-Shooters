using UdonSharp;
using UnityEngine;
namespace MMMaellon.P_Shooters
{
    public class ExampleHealthChangerPickup : UdonSharpBehaviour
    {
        public P_ShootersPlayerHandler playerHandler;
        public float healthMultiplier = 1.0f;
        public float shieldMultiplier = 2.0f;

        int prevMaxHealth = 100;
        int prevMaxShield = 100;
        public override void OnPickup()
        {
            prevMaxHealth = playerHandler.localPlayer.maxHealth;
            prevMaxShield = playerHandler.localPlayer.maxShield;
            playerHandler.localPlayer.maxHealth = Mathf.CeilToInt(playerHandler.localPlayer.maxHealth * healthMultiplier);
            playerHandler.localPlayer.health = Mathf.CeilToInt(playerHandler.localPlayer.health * healthMultiplier);
            playerHandler.localPlayer.maxShield = Mathf.CeilToInt(playerHandler.localPlayer.maxShield * shieldMultiplier);
            playerHandler.localPlayer.shield = Mathf.CeilToInt(playerHandler.localPlayer.shield * shieldMultiplier);
        }

        public override void OnDrop()
        {
            playerHandler.localPlayer.maxHealth = prevMaxHealth;
            playerHandler.localPlayer.health = Mathf.CeilToInt(playerHandler.localPlayer.health * healthMultiplier);
            playerHandler.localPlayer.maxShield = prevMaxShield;
            playerHandler.localPlayer.shield = Mathf.CeilToInt(playerHandler.localPlayer.shield * shieldMultiplier);
        }
    }
}
