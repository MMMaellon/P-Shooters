
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    public class ExamplePlayerListener : PlayerListener
    {
        public override void OnDecreaseHealth(Player player, int value)
        {
            
        }

        public override void OnDecreaseShield(Player player, int value)
        {
            
        }

        public override void OnIncreaseHealth(Player player, int value)
        {
            
        }

        public override void OnIncreaseShield(Player player, int value)
        {
            
        }

        public override void OnMaxHealth(Player player, int value)
        {
            
        }

        public override void OnMaxShield(Player player, int value)
        {
            
        }

        public override void OnMinHealth(Player player, int value)
        {
            if (gameObject.activeInHierarchy && Utilities.IsValid(player.Owner) && player.Owner.isLocal)
            {
                player.ResetPlayer();
                player.Owner.Respawn();
            }
        }

        public override void OnMinShield(Player player, int value)
        {
            
        }

        void Start()
        {

        }
    }
}