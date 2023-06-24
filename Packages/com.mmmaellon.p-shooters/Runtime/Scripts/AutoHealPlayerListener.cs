
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.P_Shooters
{
    public class AutoHealPlayerListener : PlayerListener
    {
        public bool healHealth = false;
        public bool healShield = true;
        public float postDamageCooldown = 10f;
        public int healAmount = 3;
        public float healInterval = 1f;
        float lastDamage = -1001f;
        Player localPlayer;
        public override void OnDecreaseHealth(Player attacker, Player receiver, int value)
        {
            if (!receiver.IsOwnerLocal())
            {
                return;
            }
            localPlayer = receiver;
            lastDamage = Time.timeSinceLevelLoad;
            lastHeal = -1001f;
        }

        public override void OnDecreaseShield(Player attacker, Player receiver, int value)
        {
            if (!receiver.IsOwnerLocal())
            {
                return;
            }
            localPlayer = receiver;
            lastDamage = Time.timeSinceLevelLoad;
            lastHeal = -1001f;
        }
        public override void OnMaxHealth(Player healer, Player receiver, int value)
        {
            if (!receiver.IsOwnerLocal())
            {
                return;
            }
            localPlayer = receiver;
        }
        public override void OnMaxShield(Player healer, Player receiver, int value)
        {
            if (!receiver.IsOwnerLocal())
            {
                return;
            }
            localPlayer = receiver;
        }

        float lastHeal = -1001f;
        public void Update()
        {
            if (!Utilities.IsValid(localPlayer))
            {
                return;
            }
            if (lastHeal + healInterval > Time.timeSinceLevelLoad)
            {
                return;
            }
            if (lastDamage + postDamageCooldown > Time.timeSinceLevelLoad)
            {
                return;
            }
            lastHeal = Time.timeSinceLevelLoad;
            if (healHealth && localPlayer.health < localPlayer.maxHealth)
            {
                localPlayer.health = Mathf.Min(localPlayer.maxShield, localPlayer.health + healAmount);
                if (localPlayer.maxHealth - localPlayer.health < healAmount && healShield && localPlayer.shield < localPlayer.maxShield)
                {
                    localPlayer.shield = Mathf.Min(localPlayer.maxShield, localPlayer.shield + (healAmount - localPlayer.maxHealth + localPlayer.health));
                }
                return;
            }
            if (healShield && localPlayer.shield < localPlayer.maxShield)
            {
                localPlayer.shield = Mathf.Min(localPlayer.maxShield, localPlayer.shield + healAmount);
            }
        }
    }
}