
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Teleporter : UdonSharpBehaviour
    {
        public GameObject[] possible_destinations;

        [Header("If you want to only have 1 team be able to use this teleport, you gotta set the player handler")]
        public PlayerHandler player_handler;
        public int team = 0;
        void Start()
        {

        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (player != null && player.IsValid() && player.isLocal)
            {
                Teleport();
            }
        }

        public void Teleport()
        {
            if (possible_destinations.Length > 0 && (team == 0 || player_handler == null || !player_handler.teams || player_handler._localPlayer.team == team))
            {
                int random = Random.Range(0, possible_destinations.Length);
                Networking.LocalPlayer.TeleportTo(possible_destinations[random].transform.position, possible_destinations[random].transform.rotation);
            }
        }
    }
}