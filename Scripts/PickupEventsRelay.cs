
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PickupEventsRelay : UdonSharpBehaviour
{
    public UdonBehaviour relay_target;

    override public void OnPickup()
    {
        relay_target.SendCustomEvent("_OnPickup");
    }

    override public void OnPickupUseDown()
    {
        relay_target.SendCustomEvent("_OnPickupUseDown");
    }

    override public void OnPickupUseUp()
    {
        relay_target.SendCustomEvent("_OnPickupUseUp");
    }

    override public void OnDrop()
    {
        relay_target.SendCustomEvent("_OnDrop");
    }
    void Start()
    {
        
    }
}
