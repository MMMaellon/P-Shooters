
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class HeadFollower : UdonSharpBehaviour
{
    void Start()
    {
    }

    public void LateUpdate()
    {
        VRCPlayerApi.TrackingData headData = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
        transform.position = headData.position;
        transform.rotation = headData.rotation;
    }
}
