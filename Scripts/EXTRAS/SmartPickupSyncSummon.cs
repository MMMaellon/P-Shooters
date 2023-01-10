
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SmartPickupSyncSummon : UdonSharpBehaviour
{    
    //THIS IS THE OBJECT THAT YOU WANNA TELEPORT
    [Header("Only the owner can summon this object. Call the 'TakeOwnership' event or set ownership some other way")]
    public SmartPickupSync teleportObject;
    public bool summon_only_if_idle = true;

    [Header("VR shortcut is both triggers. Desktop shortcut is F key by default")]
    public bool use_shortcuts = true;
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
        if(!Networking.LocalPlayer.IsOwner(gameObject) || !use_shortcuts){
            return;
        }
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
        if (use_shortcuts && Input.GetKeyDown(desktopShortcut))
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
        if (!Networking.LocalPlayer.IsOwner(gameObject))
        {
            return;
        }
        Networking.SetOwner(Networking.LocalPlayer, teleportObject.gameObject);
        teleportObject.gameObject.SetActive(true);//turn it on
        Vector3 spawn_point = use_head_as_spawn_point ? Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position : Networking.LocalPlayer.GetPosition();
        teleportObject.isHeld = false;
        teleportObject.pos = spawn_point + (Networking.LocalPlayer.GetRotation() * spawn_point_offset);
        teleportObject.rot = Networking.LocalPlayer.GetRotation();
        teleportObject.MoveToSyncedTransform();
        teleportObject.RequestSerialization();
        if (teleportObject.optimizer != null)
        {
            teleportObject.optimizer.BroadcastEnable();
        }
    }

    public void TakeOwnership()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }

    public override void Interact()
    {
        if (!summon_only_if_idle || teleportObject.enabled == false)
        {
            TakeOwnership();
            SummonObject();
        }
    }
}
