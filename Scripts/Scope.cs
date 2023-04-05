
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Scope : UdonSharpBehaviour
{
    VRCPlayerApi _localPlayer;
    public GameObject scopeCam;
    VRC_Pickup parentPickup;
    void Start()
    {
        _localPlayer = Networking.LocalPlayer;
        parentPickup = GetComponentInParent<VRC_Pickup>();
    }

    VRC_Pickup leftPickup;
    VRC_Pickup rightPickup;
    VRCPlayerApi.TrackingData headData;
    VRCPlayerApi.TrackingData leftData;
    VRCPlayerApi.TrackingData rightData;
    public void Update()
    {
        //look at other scopes and see which one is closer to your face.
        if (!Utilities.IsValid(_localPlayer))
        {
            return;
        }

        leftPickup = _localPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Left);
        rightPickup = _localPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Right);
        headData = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
        leftData = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
        rightData = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);

        if (leftPickup == parentPickup)
        {
            scopeCam.gameObject.SetActive(rightPickup == null || Vector3.Distance(leftData.position, headData.position) < Vector3.Distance(rightData.position, headData.position));
        } else if (rightPickup == parentPickup)
        {
            scopeCam.gameObject.SetActive(leftPickup == null || Vector3.Distance(leftData.position, headData.position) >= Vector3.Distance(rightData.position, headData.position));
        } else
        {
            scopeCam.gameObject.SetActive(false);
        }
    }
}
