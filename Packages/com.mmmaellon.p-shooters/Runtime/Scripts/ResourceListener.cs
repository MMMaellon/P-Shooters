using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    public abstract class ResourceListener : UdonSharpBehaviour
    {
        public abstract void OnChange(Player player, ResourceManager resource, int oldValue, int newValue);

    }
}