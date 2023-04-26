
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncSpeedTest : UdonSharpBehaviour
{
    MeshRenderer mesh;
    public bool fast = false;
    [UdonSynced, FieldChangeCallback(nameof(red))] public bool _red = false;
    public bool red{
        get => _red;
        set
        {
            _red = value;
            if (Utilities.IsValid(mesh))
            {
                mesh.sharedMaterial.color = red ? Color.red : Color.blue;
            }
            if (value)
            {
                SendCustomEventDelayedSeconds(nameof(UnSyncFast), 2);
            }
        }
    }
    void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        mesh.material.color = Color.blue;
    }

    public void SendSync()
    {
        if (fast)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            red = true;
            RequestSerialization();
        } else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(Sync));
        }
    }

    public void Sync()
    {
        mesh.sharedMaterial.color = Color.red;
        SendCustomEventDelayedSeconds(nameof(UnSync), 2);
    }

    public void UnSync()
    {
        if (Utilities.IsValid(mesh))
        {
            mesh.sharedMaterial.color = Color.blue;
        }
    }

    public void UnSyncFast()
    {
        red = false;
        RequestSerialization();
    }
}
