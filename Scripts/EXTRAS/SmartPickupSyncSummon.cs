
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SmartPickupSyncSummon : UdonSharpBehaviour
{    
    //THIS IS THE OBJECT THAT YOU WANNA TELEPORT
    [Header("Only the owner can summon this object. Call the 'TakeOwnership' event or set ownership some other way")]
    public SmartPickupSync[] spawnObjects;
    public bool summon_only_if_idle = true;

    [Header("VR shortcut is both triggers. Desktop shortcut is F key by default")]
    public bool use_shortcuts = true;
    private bool leftTrigger = false;
    private bool rightTrigger = false;
    public KeyCode desktopShortcut = KeyCode.F;
    public bool use_player_as_spawn_origin = false;
    public bool use_player_as_spawn_origin_when_using_shortcuts = true;
    public Transform spawn_origin;
    public Vector3 spawn_point_offset;
    
    [HideInInspector, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(spawnIndex))]
    public int _spawnIndex = -1;
    private float lastSpawn = -1001f;
    private int lastSpawnIndex = 0;
    public float spawnCooldown = 0.5f;
    public int spawnIndex
    {
        get => _spawnIndex;
        set
        {
            _spawnIndex = value;
            if (value < 0 || value >= spawnObjects.Length)
            {
                return;
            }
            if (spawnObjects[value] != null && (!spawnObjects[value].gameObject.activeSelf || !spawnObjects[value].enabled))
            {
                spawnObjects[value].enabled = true;
                spawnObjects[value].gameObject.SetActive(true);
                lastSpawn = Time.timeSinceLevelLoad;
            }
            RequestSerialization();
        }
    }

    public void Start()
    {
    }


#if !COMPILER_UDONSHARP && UNITY_EDITOR
    public void Reset()
    {
        UnityEditor.SerializedObject serializedObject = new UnityEditor.SerializedObject(this);
        serializedObject.FindProperty("spawn_origin").objectReferenceValue = transform;
        serializedObject.ApplyModifiedProperties();
    }
#endif

    public override void InputUse(bool value, VRC.Udon.Common.UdonInputEventArgs args)
    {
        if(!use_shortcuts){
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

        if (leftTrigger && rightTrigger)
        {
            VRC_Pickup leftPickup = Networking.LocalPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Left);
            VRC_Pickup rightPickup = Networking.LocalPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Right);
            if (leftPickup == null && rightPickup == null)
            {
                SummonObject(use_player_as_spawn_origin || use_player_as_spawn_origin_when_using_shortcuts);
            }
        }
    }

    public void Update()
    {
        if (use_shortcuts && Input.GetKeyDown(desktopShortcut))
        {
            VRC_Pickup leftPickup = Networking.LocalPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Left);
            VRC_Pickup rightPickup = Networking.LocalPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Right);
            if (leftPickup == null && rightPickup == null)
            {
                SummonObject(use_player_as_spawn_origin || use_player_as_spawn_origin_when_using_shortcuts);
            }
        }
    }

    public void OnDisable()
    {
        foreach (SmartPickupSync spawnObject in spawnObjects)
        {
            spawnObject.pickup.Drop();
            spawnObject.gameObject.SetActive(false);//turn it off
        }
    }

    public void SummonObject(bool player_origin)
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        int newSpawnIndex = lastSpawnIndex;
        while (spawnObjects.Length > 0 && newSpawnIndex < spawnObjects.Length)
        {
            if (newSpawnIndex >= 0 && newSpawnIndex < spawnObjects.Length && spawnObjects[newSpawnIndex] != null)
            {
                if (!spawnObjects[newSpawnIndex].gameObject.activeSelf || !spawnObjects[newSpawnIndex].enabled || Networking.LocalPlayer.IsOwner(spawnObjects[newSpawnIndex].gameObject) || !summon_only_if_idle)
                {
                    break;
                } else
                {
                    newSpawnIndex = (newSpawnIndex + 1) % spawnObjects.Length;
                    if (newSpawnIndex == lastSpawnIndex)
                    {
                        break;
                    }
                }
            }
        }

        if (spawnIndex == newSpawnIndex)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(SpawnSameIndex));
        } else
        {
            spawnIndex = newSpawnIndex;
        }

        if (spawnIndex < 0 || spawnIndex >= spawnObjects.Length)
        {
            return;
        }
        SmartPickupSync spawnObject = spawnObjects[spawnIndex];
        if (spawnObject != null)
        {
            Networking.SetOwner(Networking.LocalPlayer, spawnObject.gameObject);
            lastSpawnIndex = spawnIndex;
            spawnObject.isHeld = false;
            Vector3 spawn_point = player_origin ? Networking.LocalPlayer.GetPosition() : spawn_origin ? spawn_origin.position : Vector3.zero;
            Quaternion spawn_rotation = player_origin ? Networking.LocalPlayer.GetRotation() : spawn_origin ? spawn_origin.rotation : Quaternion.identity;
            spawnObject.pos = spawn_point + (spawn_rotation * spawn_point_offset);
            spawnObject.rot = spawn_rotation;
            spawnObject.MoveToSyncedTransform();
            spawnObject.RequestSerialization();
            if (spawnObject.optimizer != null)
            {
                spawnObject.optimizer.BroadcastEnable();
            }
        }
    }

    public override void Interact()
    {
        SummonObject(use_player_as_spawn_origin);
    }

    public void SpawnSameIndex(){
        spawnIndex = spawnIndex;
    }
}
