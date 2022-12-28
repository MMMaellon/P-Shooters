
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PickupEventsRelay : UdonSharpBehaviour
{
    public UdonBehaviour relay_target;
    public bool take_ownership = true;

    override public void OnPickup()
    {
        if(!relay_target.enabled){
            relay_target.enabled = true;
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(EnableTarget));
        }
        if (take_ownership)
        {
            Networking.SetOwner(Networking.LocalPlayer, relay_target.gameObject);
        }
        relay_target.SendCustomEvent("_OnPickup");
        
    }

    public void EnableTarget(){
        relay_target.enabled = true;
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
