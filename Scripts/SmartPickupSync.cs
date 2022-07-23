
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SmartPickupSync : UdonSharpBehaviour
{
    public VRC_Pickup pickup;
    public float maxDistanceErr = 0.05f;
    public float maxRotationErr = 5f;
    public float syncInterval = 0.5f;
    public bool localTransforms = false;
    public bool force_gun_orientation_in_vr = false;
    public bool allow_theft_from_self = false;
    [System.NonSerialized, UdonSynced(UdonSyncMode.None)] public Vector3 relativePos;
    [System.NonSerialized, UdonSynced(UdonSyncMode.None)] public Quaternion relativeRot;
    [System.NonSerialized, UdonSynced(UdonSyncMode.None)] public Vector3 pos;
    [System.NonSerialized, UdonSynced(UdonSyncMode.None)] public Quaternion rot;
    [System.NonSerialized, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(isHeld))] public bool _isHeld;
    [System.NonSerialized, UdonSynced(UdonSyncMode.None)] public bool rightHand;
    [System.NonSerialized] public float sinceLastRequest;
    [System.NonSerialized] public Vector3 posBuff;
    [System.NonSerialized] public Quaternion rotBuff;
    private bool start_ran = false;
    [System.NonSerialized] public Vector3 restPos;
    [System.NonSerialized] public Quaternion restRot;

    public bool isHeld
    {
        get => _isHeld;
        set
        {
            _isHeld = value;
            bool is_owner = Networking.LocalPlayer.IsOwner(gameObject);
            if (pickup != null && ((pickup.DisallowTheft && !is_owner) || (!allow_theft_from_self && is_owner)))
            {
                pickup.pickupable = !_isHeld;
            }
        }
    }
    void Start()
    {
        if (!localTransforms)
        {
            pos = pickup.transform.position;
            rot = pickup.transform.rotation;
        }
        else
        {
            pos = pickup.transform.localPosition;
            rot = pickup.transform.localRotation;
        }
        if (!force_gun_orientation_in_vr && Networking.LocalPlayer.IsUserInVR())
        {
            pickup.orientation = VRC_Pickup.PickupOrientation.Any;
        }
        restPos = pos;
        restRot = rot;
        start_ran = true;
    }

    public override void OnDrop()
    {
        pickup.pickupable = true;
        SendCustomEventDelayedSeconds(nameof(OnDrop_Delayed), 0.1f, VRC.Udon.Common.Enums.EventTiming.Update);
        OnDrop_Delayed();
    }

    public void OnDrop_Delayed()
    {
        Debug.LogWarning("On drop delayed");
        if (!localTransforms)
        {
            pos = pickup.transform.position;
            rot = pickup.transform.rotation;
        }
        else
        {
            pos = pickup.transform.localPosition;
            rot = pickup.transform.localRotation;
        }
        isHeld = false;
        sinceLastRequest = 0;
        RequestSerialization();
    }

    public override void OnPickup()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        isHeld = true;
        SendCustomEventDelayedSeconds(nameof(OnPickup_Delayed), 0.5f, VRC.Udon.Common.Enums.EventTiming.Update);
        OnPickup_Common();
        pickup.pickupable = false;
    }
    public void OnPickup_Delayed()
    {
        isHeld = true;
        OnPickup_Common();
    }

    public void OnPickup_Common()
    {
        Vector3 leftBone = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.LeftHand);
        Vector3 rightBone = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.RightHand);

        if (leftBone == Vector3.zero)
        {
            rightHand = false;
            Quaternion inverseRot = Quaternion.Inverse(Networking.LocalPlayer.GetRotation());
            relativePos = inverseRot * (pickup.transform.position - Networking.LocalPlayer.GetPosition());
            relativeRot = inverseRot * pickup.transform.rotation;
        }
        else
        {
            if (pickup.currentHand == VRC_Pickup.PickupHand.None)
            {
                rightHand = Vector3.Distance(leftBone, pickup.transform.position) >= Vector3.Distance(rightBone, pickup.transform.position);
            }
            else
            {
                rightHand = pickup.currentHand == VRC_Pickup.PickupHand.Right;
            }

            if (rightHand)
            {
                Quaternion inverseRightBoneRot = Quaternion.Inverse(Networking.LocalPlayer.GetBoneRotation(HumanBodyBones.RightHand));
                relativePos = inverseRightBoneRot * (pickup.transform.position - rightBone);
                relativeRot = inverseRightBoneRot * pickup.transform.rotation;
            }
            else
            {
                Quaternion inverseLeftBoneRot = Quaternion.Inverse(Networking.LocalPlayer.GetBoneRotation(HumanBodyBones.LeftHand));
                relativePos = inverseLeftBoneRot * (pickup.transform.position - leftBone);
                relativeRot = inverseLeftBoneRot * pickup.transform.rotation;
            }
        }
        RequestSerialization();
    }

    public void MoveToSyncedTransform()
    {
        if (isHeld)
        {
            if (rightHand)
            {
                VRCPlayerApi player = Networking.GetOwner(gameObject);
                Quaternion rightBoneRot = player.GetBoneRotation(HumanBodyBones.RightHand);
                Vector3 rightBone = player.GetBonePosition(HumanBodyBones.RightHand);
                if (rightBone == Vector3.zero)
                {
                    pickup.transform.SetPositionAndRotation(player.GetPosition() + (player.GetRotation() * relativePos), player.GetRotation() * relativeRot);
                }
                else
                {
                    pickup.transform.SetPositionAndRotation(rightBone + (rightBoneRot * relativePos), rightBoneRot * relativeRot);
                }
            }
            else
            {
                VRCPlayerApi player = Networking.GetOwner(gameObject);
                Quaternion leftBoneRot = player.GetBoneRotation(HumanBodyBones.LeftHand);
                Vector3 leftBone = player.GetBonePosition(HumanBodyBones.LeftHand);
                if (leftBone == Vector3.zero)
                {
                    pickup.transform.SetPositionAndRotation(player.GetPosition() + (player.GetRotation() * relativePos), player.GetRotation() * relativeRot);
                }
                else
                {
                    pickup.transform.SetPositionAndRotation(leftBone + (leftBoneRot * relativePos), leftBoneRot * relativeRot);
                }
            }
        }
        else
        {
            if (!localTransforms)
            {
                pickup.transform.SetPositionAndRotation(pos, rot);
            }
            else
            {
                pickup.transform.localPosition = pos;
                pickup.transform.localRotation = rot;
            }
        }
    }

    public void Update()
    {
        sinceLastRequest += Time.deltaTime;
        if (Networking.LocalPlayer != null)
        {
            if (Networking.LocalPlayer.IsOwner(gameObject))
            {
                if (isHeld)
                {
                    if (pickup.IsHeld)
                    {
                        if (rightHand)
                        {
                            Quaternion inverseRightBoneRot = Quaternion.Inverse(Networking.LocalPlayer.GetBoneRotation(HumanBodyBones.RightHand));
                            Vector3 rightBone = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.RightHand);
                            posBuff = inverseRightBoneRot * (pickup.transform.position - rightBone);
                            rotBuff = inverseRightBoneRot * pickup.transform.rotation;
                        }
                        else
                        {
                            Quaternion inverseLeftBoneRot = Quaternion.Inverse(Networking.LocalPlayer.GetBoneRotation(HumanBodyBones.LeftHand));
                            Vector3 rightBone = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.LeftHand);
                            posBuff = inverseLeftBoneRot * (pickup.transform.position - rightBone);
                            rotBuff = inverseLeftBoneRot * pickup.transform.rotation;
                        }

                        if ((Vector3.Distance(relativePos, posBuff) > maxDistanceErr || Quaternion.Angle(relativeRot, rotBuff) > maxRotationErr) && (sinceLastRequest > syncInterval))
                        {
                            sinceLastRequest = 0;
                            relativePos = posBuff;
                            relativeRot = rotBuff;
                            RequestSerialization();
                        }
                    }
                    else
                    {
                        OnDrop_Delayed();
                    }
                } else
                {
                    if (localTransforms)
                    {
                        posBuff = pickup.transform.localPosition;
                        rotBuff = pickup.transform.localRotation;
                    } else
                    {
                        posBuff = pickup.transform.position;
                        rotBuff = pickup.transform.rotation;
                    }
                    if ((Vector3.Distance(pos, posBuff) > maxDistanceErr || Quaternion.Angle(rot, rotBuff) > maxRotationErr) && (sinceLastRequest > syncInterval))
                    {
                        sinceLastRequest = 0;
                        pos = posBuff;
                        rot = rotBuff;
                        RequestSerialization();
                    }
                }
            }
            else
            {
                MoveToSyncedTransform();
            }
        }
    }

    public void Respawn()
    {
        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            pickup.Drop();
            isHeld = false;
            pos = restPos;
            rot = restRot;
            MoveToSyncedTransform();
            RequestSerialization();
        }
    }

    // public override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner)
    // {
    //     if (requestedOwner != null && requestedOwner.isLocal && isHeld)
    //     {
    //         if (pickup != null)
    //         {
    //             pickup.Drop();
    //             OnDrop();
    //         }
    //     }
    //     return true;
    // }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        if (player != null && player.IsValid() && !player.isLocal && pickup != null)
        {
            pickup.Drop();
        }
    }

}
