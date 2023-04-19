
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    public class CTFCapturePoint : UdonSharpBehaviour
    {
        public Collider flag;

        [Header("If team is set to 0 or a negative number, teams are ignored")]
        public int team = 0;
        public PlayerHandler playerHandler;
        public UdonBehaviour eventTarget;
        public string eventName = "IncrementScore";
        public void OnTriggerEnter(Collider other)
        {
            if (flag != null && other == flag && Networking.LocalPlayer.IsOwner(flag.gameObject))
            {
                if (team > 0 && playerHandler != null && playerHandler.teams && playerHandler._localPlayer.team != team)//you scored for the wrong team lmao
                {
                    return;
                }
                SmartPickupSync pickup = flag.GetComponent<SmartPickupSync>();
                if (pickup != null)
                {
                    pickup.Respawn();
                }
                eventTarget.SendCustomEvent(eventName);
            }
        }
    }
}