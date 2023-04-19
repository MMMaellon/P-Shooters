
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class TacticalReload : UdonSharpBehaviour
    {
        public P_Shooter parent_gun;
        public Collider pickup_collider;
        public VRC_Pickup pickup;
        [System.NonSerialized, UdonSynced(UdonSyncMode.None)] public bool _isHeld = false;
        [System.NonSerialized, UdonSynced(UdonSyncMode.None)] public bool rightHand;
        Vector3 recordedPos;
        
        public bool isHeld{
            get => _isHeld;
            set
            {
                _isHeld = value;
                if (!value)
                {
                    RestPos();
                }
                return;
            }
        }

        [System.NonSerialized] public bool recorded = false;
        void Start()
        {
            HideCollider();
        }

        public override void OnPickup()
        {
            RecordHandPos();
            OnPickupDelayed();
            SendCustomEventDelayedSeconds(nameof(OnPickupDelayed), 0.5f, VRC.Udon.Common.Enums.EventTiming.Update);
        }

        public void OnPickupDelayed()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            if (parent_gun != null)
            {
                parent_gun.TacticalReload();
            }
            if (pickup != null)
            {
                rightHand = pickup.currentHand == VRC_Pickup.PickupHand.Right;
                isHeld = true;
                RequestSerialization();
            }
        }

        public override void OnDrop()
        {
            isHeld = false;
            RequestSerialization();
        }

        public void HideCollider()
        {
            if (pickup_collider != null)
            {
                pickup_collider.enabled = false;
            }
        }

        public void ShowCollider()
        {
            if (pickup_collider != null)
            {
                pickup_collider.enabled = true;
            }
        }

        public void ToggleCollider(bool shown)
        {
            if (pickup_collider != null)
            {
                pickup_collider.enabled = shown;
            }
        }

        public void RestPos()
        {
            transform.localPosition = Vector3.zero;
        }

        public void RecordHandPos()
        {
            VRCPlayerApi player = Networking.GetOwner(gameObject);
            if (player == null || !player.IsValid())
            {
                return;
            }
            if (rightHand)
            {
                VRCPlayerApi.TrackingData handTracking = player.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
                recordedPos = handTracking.position;
            }
            else
            {
                VRCPlayerApi.TrackingData handTracking = player.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
                recordedPos = handTracking.position;
            }
            recorded = true;
        }

        public void HandPos()
        {
            if (Networking.LocalPlayer.IsOwner(gameObject) && false)
            {
                RecordHandPos();
                transform.position = recordedPos;
            }
            else
            {
                if (recorded)
                {
                    transform.position = recordedPos;
                }
                recorded = false;
                SendCustomEventDelayedFrames(nameof(RecordHandPos), 1, VRC.Udon.Common.Enums.EventTiming.Update);
            }
        }
    }
}