
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    [RequireComponent(typeof(Rigidbody))]
    public class EventPerSecondTrigger : UdonSharpBehaviour
    {
        public UdonBehaviour eventTarget;
        public string eventName = "IncrementScore";
        public float interval = 1f;
        private float last_event = -1001f;
        public override void OnPlayerTriggerStay(VRCPlayerApi player)
        {
            if (last_event + interval > Time.timeSinceLevelLoad || player == null || !player.IsValid() || !player.isLocal)
            {
                return;
            }
            last_event = Time.timeSinceLevelLoad;
            eventTarget.SendCustomEvent(eventName);
        }
    }
}