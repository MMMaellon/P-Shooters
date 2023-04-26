using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    public abstract class PlayerListener : UdonSharpBehaviour
    {
        public abstract void OnIncreaseShield(Player player, int value);
        public abstract void OnDecreaseShield(Player player, int value);
        public abstract void OnMaxShield(Player player, int value);
        public abstract void OnMinShield(Player player, int value);
        public abstract void OnIncreaseHealth(Player player, int value);
        public abstract void OnDecreaseHealth(Player player, int value);
        public abstract void OnMaxHealth(Player player, int value);
        public abstract void OnMinHealth(Player player, int value);

    }
}