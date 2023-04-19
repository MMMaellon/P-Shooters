
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SmartPickupSyncOptimizer : UdonSharpBehaviour
    {
        [System.NonSerialized] public SmartPickupSync pickup;
        [Header("How long after being dropped to optimize the smart pickup sync")]
        public float optimizationTimer = 25f;
        private float last_drop = 0f;
        void Start()
        {
            pickup = GetComponent<SmartPickupSync>();
            pickup.optimizer = this;
        }
        public override void OnPickup()
        {
            BroadcastEnable();
        }

        public void BroadcastEnable()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(EnablePickup));
        }

        // public override void OnDrop()
        // {
        //     BroadcastEnable();
        // }

        public void EnablePickup()
        {
            last_drop = -1001f;
            pickup.enabled = true;
        }

        public void StartDropTimer()
        {
            last_drop = Time.timeSinceLevelLoad;
            pickup.enabled = true;
            SendCustomEventDelayedSeconds(nameof(DisablePickup), optimizationTimer);
        }

        public void DisablePickup()
        {
            if (last_drop >= 0 && last_drop + optimizationTimer - 0.1f < Time.timeSinceLevelLoad)
            {
                pickup.enabled = false;
            }
        }
    }
}