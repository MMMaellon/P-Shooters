using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    public abstract class ResourceListener : UdonSharpBehaviour
    {
        public abstract void OnIncrease(Player player, Resource resource, int value);
        public abstract void OnDecrease(Player player, Resource resource, int value);
        public abstract void OnMax(Player player, Resource resource, int value);
        public abstract void OnMin(Player player, Resource resource, int value);

    }
}