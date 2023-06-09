
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.P_Shooters
{
    public class TimedToggle : UdonSharpBehaviour
    {
        public GameObject[] objs;
        public float time = 15f;
        bool[] startState;
        bool toggledOff = false;
        public void Start()
        {
            startState = new bool[objs.Length];
            for (int i = 0; i < objs.Length; i++)
            {
                startState[i] = objs[i].activeSelf;
            }
        }
        public void Toggle()
        {
            if (toggledOff)
            {
                return;
            }
            toggledOff = true;
            for (int i = 0; i < objs.Length; i++)
            {
                if (Utilities.IsValid(objs[i]))
                {
                    objs[i].SetActive(!startState[i]);
                }
            }
            SendCustomEventDelayedSeconds(nameof(ResetToggle), time);
        }

        public void ResetToggle()
        {
            toggledOff = false;
            for (int i = 0; i < objs.Length; i++)
            {
                if (Utilities.IsValid(objs[i]))
                {
                    objs[i].SetActive(startState[i]);
                }
            }
        }
    }
}