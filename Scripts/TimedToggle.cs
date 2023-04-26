
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TimedToggle : UdonSharpBehaviour
{
    public GameObject[] objs;
    public float time = 15f;
    public void Toggle()
    {
        foreach (GameObject obj in objs)
        {
            if (Utilities.IsValid(obj))
            {
                obj.SetActive(false);
            }
        }
        SendCustomEventDelayedSeconds(nameof(ResetToggle), time);
    }

    public void ResetToggle()
    {
        foreach (GameObject obj in objs)
        {
            if (Utilities.IsValid(obj))
            {
                obj.SetActive(true);
            }
        }
    }
}
