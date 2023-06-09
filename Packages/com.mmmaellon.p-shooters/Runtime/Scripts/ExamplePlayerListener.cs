
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.P_Shooters
{
    public class ExamplePlayerListener : PlayerListener
    {
        public float respawnInvincibilityTime = 3f;
        public AudioSource receiveDamageAudio;
        public AudioSource onKillConfirmedAudio;

        [System.NonSerialized]
        public float[] respawnTimes;
        void Start()
        {
            respawnTimes = new float[playerHandler.players.Length];
            for (int i = 0; i < respawnTimes.Length; i++)
            {
                respawnTimes[i] = -1001f;
            }
        }

        public override bool CanDealDamage(Player attacker, Player receiver)
        {
            if (gameObject.activeInHierarchy && respawnTimes[receiver.id] + respawnInvincibilityTime >= Time.timeSinceLevelLoad)
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
            if (gameObject.activeInHierarchy && receiver.IsOwnerLocal())
            {
                receiver.ResetPlayer();
                receiver.Owner.Respawn();
                respawnTimes[receiver.id] = Time.timeSinceLevelLoad;
                attacker.ConfirmNormalKill();
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
    }
}