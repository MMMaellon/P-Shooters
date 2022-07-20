
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Teleporter : UdonSharpBehaviour
{
    public GameObject[] possible_destinations;
    void Start()
    {
        
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player != null && player.IsValid() && player.isLocal)
        {
            Teleport();
        }
    }

    public void Teleport()
    {
        if (possible_destinations.Length > 0)
        {
            int random = Random.Range(0, possible_destinations.Length);
            Networking.LocalPlayer.TeleportTo(possible_destinations[random].transform.position, possible_destinations[random].transform.rotation);
        }
    }
}
