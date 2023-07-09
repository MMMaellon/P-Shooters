
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor;
using UdonSharpEditor;
using System.Collections.Immutable;
#endif

namespace MMMaellon.P_Shooters
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ResourceChanger : UdonSharpBehaviour
    {
        public ResourceManager resource;
        [Tooltip("Check to increment and decrement the resource instead of setting it to a set number. Use a negative number as the value to make the resource decrement.")]
        public bool incrementByValue = true;
        public int value;

        public void Change()
        {
            ChangePlayer(resource.localPlayerObject);
        }

        public void ChangePlayer(Player player)
        {
            if (incrementByValue)
            {
                if (Utilities.IsValid(player))
                {
                    player.ChangeResourceValueById(resource.id, value);
                }
            } else
            {
                if (Utilities.IsValid(player))
                {
                    player.SetResourceValueById(resource.id, value);
                }
            }
        }
    }
}
