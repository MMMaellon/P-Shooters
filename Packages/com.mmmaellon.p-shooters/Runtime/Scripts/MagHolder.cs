
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    public class MagHolder : SmartObjectSyncListener
    {
        public Mag[] mags;

        public override void OnChangeOwner(SmartObjectSync sync, VRCPlayerApi oldOwner, VRCPlayerApi newOwner)
        {
            
        }

        public override void OnChangeState(SmartObjectSync sync, int oldState, int newState)
        {
            
        }

        void Start()
        {

        }
    }
}