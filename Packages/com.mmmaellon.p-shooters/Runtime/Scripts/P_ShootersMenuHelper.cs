
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEngine;
using VRC.SDKBase;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using VRC.SDKBase.Editor.BuildPipeline;


namespace MMMaellon.P_Shooters
{
    [InitializeOnLoad]
    public class Helper
    {
        public static bool IsEditable(Component component)
        {
            return !EditorUtility.IsPersistent(component.transform.root.gameObject) && !(component.gameObject.hideFlags == HideFlags.NotEditable || component.gameObject.hideFlags == HideFlags.HideAndDontSave);
        }

        public static void ErrorLog(Object context, string message)
        {
            Debug.LogErrorFormat(context, "<color=red>[P-Shooters AutoSetup]: ERROR</color> {0}", message);
        }
        public static void InfoLog(Object context, string message)
        {
            Debug.LogFormat(context, "<color=blue>[P-Shooters AutoSetup]: INFO</color> {0}", message);
        }
    } 
}
#endif