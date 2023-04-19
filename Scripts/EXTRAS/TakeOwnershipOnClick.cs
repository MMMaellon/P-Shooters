
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    public class TakeOwnershipOnClick : UdonSharpBehaviour
    {
        public GameObject[] objects;

        public override void Interact()
        {
            foreach (GameObject obj in objects)
            {
                Networking.SetOwner(Networking.LocalPlayer, obj);
            }
        }
    }
}