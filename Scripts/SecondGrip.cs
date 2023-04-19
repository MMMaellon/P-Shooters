
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


namespace MMMaellon
{
#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(SecondGrip))]
    public class SecondGripEditor : Editor
    {
        public static void SetupGrips()
        {
            int count = 0;
            foreach (SecondGrip grip in GameObject.FindObjectsOfType(typeof(SecondGrip)) as SecondGrip[])
            {
                if (grip.transform.parent != null)
                {
                    foreach (P_Shooter shooter in grip.transform.parent.GetComponentsInChildren<P_Shooter>())
                    {
                        SerializedObject serializedShooter = new SerializedObject(shooter);
                        serializedShooter.FindProperty("grip").objectReferenceValue = grip;
                        serializedShooter.ApplyModifiedProperties();

                        SerializedObject serializedGrip = new SerializedObject(grip);
                        serializedGrip.FindProperty("gunModel").objectReferenceValue = shooter.transform.Find("model");
                        serializedGrip.ApplyModifiedProperties();
                        break;
                    }
                }
                count++;
            }
            if (count > 0)
            {
                Debug.Log($"Set up {count} grips");
            }
            else
            {
                Debug.Log($"No guns were set up");
            }
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("SECONDARY GRIPS SETUP");
            EditorGUILayout.HelpBox(
    @"Look at your GunManager Object for instructions", MessageType.Info);
            if (GUILayout.Button(new GUIContent("Set up ALL Grips")))
            {
                SecondGripEditor.SetupGrips();
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            EditorGUILayout.Space();
            base.OnInspectorGUI();
        }
    }
#endif

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SecondGrip : UdonSharpBehaviour
    {
        public Collider pickup_collider;
        public SmartPickupSync smartPickup;
        public Transform gunModel;

        [System.NonSerialized] public Vector3 rest_local_pos = Vector3.zero;
        [System.NonSerialized] public Quaternion rest_local_rot = Quaternion.identity;
        [System.NonSerialized] public bool reparented = false;
        void Start()
        {
            RecordRestTransforms();
        }

        public void RecordRestTransforms()
        {
            rest_local_pos = transform.localPosition;
            rest_local_rot = transform.localRotation;
        }

        public void ApplyRestTransforms()
        {
            transform.localPosition = rest_local_pos;
            transform.localRotation = rest_local_rot;
        }

        override public void OnPickup()
        {
            reparented = true;
        }

        override public void OnDrop()
        {
            reparented = false;
            Reset();
        }

        public void GlobalReset()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Reset");
        }

        public void Reset()
        {
            // if (!Networking.LocalPlayer.IsOwner(gameObject))
            // {
            //     Networking.SetOwner(Networking.LocalPlayer, gameObject);
            // }
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            if (Networking.LocalPlayer.IsOwner(gameObject))
            {
                smartPickup.SendCustomEventDelayedFrames("OnDrop_Delayed", 1);
            }
        }

        public void ForceDrop()
        {
            if (smartPickup != null && smartPickup.pickup != null)
            {
                smartPickup.pickup.Drop();
            }
            // pickup.DisallowTheft = true;
            // pickup.pickupable = false;
        }

        public void AllowPickup()
        {
            if (smartPickup != null && smartPickup.pickup != null)
            {
                smartPickup.pickup.pickupable = true;
                pickup_collider.enabled = true;
            }
        }

        public void DisablePickup()
        {
            if (smartPickup != null && smartPickup.pickup != null)
            {
                smartPickup.pickup.Drop();
                smartPickup.pickup.pickupable = false;
                pickup_collider.enabled = false;
            }
        }
    }

}