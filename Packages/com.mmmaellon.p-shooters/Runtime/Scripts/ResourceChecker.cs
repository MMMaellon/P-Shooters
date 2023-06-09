
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
    public class ResourceChecker : UdonSharpBehaviour
    {
        ResourceManager resource;
        public bool passCheckIfEqual;
        public bool passCheckIfGreaterThan;
        public bool passCheckIfLessThan;
        public int checkValue;
        public UdonBehaviour passUdon;
        public string passUdonEvent;
        public UdonBehaviour failUdon;
        public string failUdonEvent;
        void Start()
        {
            
        }

        public void Check()
        {
            if (!Utilities.IsValid(resource.localPlayerObject))
            {
                return;
            }

            CheckByPlayer(resource.localPlayerObject);
        }
        public void CheckByPlayer(Player player)
        {
            int value = player.GetResourceValueById(resource.id);
            if (value == checkValue && passCheckIfEqual)
            {
                PassCheck();
            }
            else if (value < checkValue && passCheckIfLessThan){
                PassCheck();
            } else if (value > checkValue && passCheckIfGreaterThan)
            {
                PassCheck();
            } else
            {
                FailCheck();
            }
        }

        public void PassCheck()
        {
            passUdon.SendCustomEvent(passUdonEvent);
        }
        public void FailCheck()
        {
            failUdon.SendCustomEvent(failUdonEvent);
        }
    }
}