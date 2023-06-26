
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
        public float postDamageCooldown = 15f;
        public int healAmount = 10;
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
        int overheal;
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
                overheal = healAmount - (localPlayer.maxHealth - localPlayer.health);
                localPlayer.health = Mathf.Min(localPlayer.maxHealth, localPlayer.health + healAmount);
                if (overheal > 0 && healShield && localPlayer.shield < localPlayer.maxShield)
                {
                    localPlayer.shield = Mathf.Min(localPlayer.maxShield, localPlayer.shield + overheal);
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