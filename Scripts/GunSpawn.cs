
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class GunSpawn : UdonSharpBehaviour
    {
        [System.NonSerialized][UdonSynced(UdonSyncMode.None)] public bool occupied = false;

        [Tooltip("Larger number = more chance something will spawn here. 0 means nothing will spawn here, there is no max")]
        public float spawn_chance = 100;

        [Tooltip("Guns with the 'rare' checkbox use this spawn chance instead")]
        public float rare_spawn_chance = 100;
        void Start()
        {

        }

        public void SetOccupied(bool value)
        {
            if (!Networking.LocalPlayer.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
            occupied = value;
            RequestSerialization();
        }
    }
}