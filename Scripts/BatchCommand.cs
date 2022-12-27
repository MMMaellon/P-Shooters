
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class BatchCommand : UdonSharpBehaviour
{
    public UdonBehaviour[] eventTargets;
    public string eventName;
    public UdonBehaviour chainEventTarget;
    public string chainEventName;
    void Start()
    {
        
    }

    public void Trigger()
    {
        foreach(UdonBehaviour udon in eventTargets)
        {
            if (udon != null)
            {
                udon.SendCustomEvent(eventName);
            }
        }

        if (chainEventTarget != null)
        {
            chainEventTarget.SendCustomEvent(chainEventName);
        }
    }
}
