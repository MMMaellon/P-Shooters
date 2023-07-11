
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.P_Shooters
{
    public class ExamplePlayerListener : PlayerListener
    {
        public float respawnDelayTime = 2f;
        public float respawnInvincibilityTime = 3f;
        public AudioSource receiveDamageAudio;
        public AudioSource onKillConfirmedAudio;

        [System.NonSerialized]
        public float[] respawnTimes;
        Player localplayer;
        void Start()
        {
            respawnTimes = new float[playerHandler.players.Length];
            for (int i = 0; i < respawnTimes.Length; i++)
            {
                respawnTimes[i] = -1001f;
            }
            Networking.LocalPlayer.CombatSetup();
            Networking.LocalPlayer.CombatSetRespawn(true, respawnDelayTime, null);
            Networking.LocalPlayer.CombatSetMaxHitpoints(100f);
            Networking.LocalPlayer.CombatSetCurrentHitpoints(100f);
            Networking.LocalPlayer.CombatSetDamageGraphic(null);
        }

        public override bool CanDealDamage(Player attacker, Player receiver)
        {
            if (gameObject.activeInHierarchy && respawnTimes[receiver.id] + respawnDelayTime + respawnInvincibilityTime >= Time.timeSinceLevelLoad)
            {
                return false;
            }
            return true;
        }

        public override void OnDecreaseHealth(Player attacker, Player receiver, int value)
        {
            if (gameObject.activeInHierarchy && receiver.IsOwnerLocal() && Utilities.IsValid(receiveDamageAudio))
            {
                receiveDamageAudio.transform.position = receiver.transform.position;
                receiveDamageAudio.Play();
            }
        }
        public override void OnDecreaseShield(Player attacker, Player receiver, int value)
        {
            if (gameObject.activeInHierarchy && receiver.IsOwnerLocal() && Utilities.IsValid(receiveDamageAudio))
            {
                receiveDamageAudio.transform.position = receiver.transform.position;
                receiveDamageAudio.Play();
            }
        }

        public override void OnMinHealth(Player attacker, Player receiver, int value)
        {
            if (gameObject.activeInHierarchy)
            {
                if (receiver.IsOwnerLocal())
                {
                    localplayer = receiver;
                    attacker.ConfirmNormalKill();
                    SendCustomEventDelayedSeconds(nameof(RespawnCallback), respawnDelayTime, VRC.Udon.Common.Enums.EventTiming.Update);
                    respawnTimes[receiver.id] = Time.timeSinceLevelLoad;
                    receiver.Owner.CombatSetCurrentHitpoints(0);
                }
            }
        }

        public override void OnReceiveNormalKillConfirmation(Player attacker)
        {
            if (gameObject.activeInHierarchy && Utilities.IsValid(onKillConfirmedAudio))
            {
                receiveDamageAudio.transform.position = attacker.transform.position;
                receiveDamageAudio.Play();
            }
        }

        public void RespawnCallback()
        {
            //we have to make sure this gets called on a normal update loop
            localplayer.ResetPlayer();
            localplayer.Owner.Respawn();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            base.OnPlayerJoined(player);
            player.CombatSetup();
        }
    }
}