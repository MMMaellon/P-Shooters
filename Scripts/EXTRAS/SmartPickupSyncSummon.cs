
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SmartPickupSyncSummon : UdonSharpBehaviour
{    
    //THIS IS THE OBJECT THAT YOU WANNA TELEPORT
    public SmartPickupSync teleportObject;
    private bool leftTrigger = false;
    private bool rightTrigger = false;
    public KeyCode desktopShortcut = KeyCode.F;
    public bool use_head_as_spawn_point = true;
    public Vector3 spawn_point_offset = new Vector3(0, 0, 0.5f);

    public void Start()
    {
    }

    public override void InputUse(bool value, VRC.Udon.Common.UdonInputEventArgs args)
    {
        if (args.handType == VRC.Udon.Common.HandType.LEFT)
        {
            leftTrigger = value;
        }
        else
        {
            rightTrigger = value;
        }

        VRC_Pickup leftPickup = Networking.LocalPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Left);
        VRC_Pickup rightPickup = Networking.LocalPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Right);

        if (leftTrigger && rightTrigger && leftPickup == null && rightPickup == null)
        {
            SummonObject();
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(desktopShortcut))
        {
            SummonObject();
        }
    }

    public void OnDisable()
    {
        teleportObject.pickup.Drop();
        teleportObject.gameObject.SetActive(false);//turn it off
    }

    public void SummonObject()
    {
        Networking.SetOwner(Networking.LocalPlayer, teleportObject.gameObject);
        teleportObject.gameObject.SetActive(true);//turn it on
        Vector3 spawn_point = use_head_as_spawn_point ? Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position : Networking.LocalPlayer.GetPosition();
        teleportObject.pos = spawn_point + (Networking.LocalPlayer.GetRotation() * spawn_point_offset);
        teleportObject.rot = Networking.LocalPlayer.GetRotation();
        teleportObject.MoveToSyncedTransform();
        teleportObject.RequestSerialization();
    }
}
