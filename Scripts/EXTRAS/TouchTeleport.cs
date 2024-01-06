using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    public class TouchTeleport : UdonSharpBehaviour
    {
        public Transform teleportDestination;

        public void OnTriggerEnter(Collider collider)
        {
            if (!Utilities.IsValid(collider))
            {
                return;
            }
            Player player = collider.GetComponent<Player>();
            if (!Networking.LocalPlayer.IsOwner(gameObject) && Utilities.IsValid(player) && Utilities.IsValid(player.Owner) && player.Owner.isLocal)
            {
                player.Owner.TeleportTo(teleportDestination.position, teleportDestination.rotation);
            }
        }

        public override void OnPickup()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }
}