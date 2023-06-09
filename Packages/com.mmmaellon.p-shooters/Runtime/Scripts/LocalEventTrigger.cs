
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
#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(LocalEventTrigger))]
    public class LocalEventTriggerEditor : Editor
    {
        public static void SetupLocalEventTrigger(LocalEventTrigger trigger)
        {
            if (!Utilities.IsValid(trigger))
            {
                return;
            }
            SerializedObject serialized = new SerializedObject(trigger);
            serialized.FindProperty("targetUdon").objectReferenceValue = trigger.GetComponent<UdonBehaviour>();
            serialized.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            LocalEventTrigger trigger = target as LocalEventTrigger;
            if (!Utilities.IsValid(trigger))
            {
                return;
            }
            if (!Utilities.IsValid(trigger.targetUdon))
            {
                EditorGUILayout.LabelField("Missing Target Udon");
                EditorGUILayout.HelpBox(
@"Script will fail if no target udon is specified
", MessageType.Info);
            }
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            EditorGUILayout.Space();
            base.OnInspectorGUI();
        }
    }
#endif
    public class LocalEventTrigger : UdonSharpBehaviour
    {
        public UdonBehaviour targetUdon;
        [Tooltip("The name of the event to trigger. Use \"Change\" for ResourceChanger and HealthAndShieldChanger and use \"Check\" for ResourceChecker")]
        public string targetUdonEvent;
        
        [Header("VRChat Events")]
        [Tooltip("Pressing Left Mouse or Trigger")]
        public bool onInteract;
        public bool onPickup;
        [Tooltip("Pressing Left Mouse or Trigger while holding the object")]
        public bool onPickupUseDown;
        [Tooltip("Letting go of Left Mouse or Trigger while holding the object")]
        public bool onPickupUseUp;
        public bool onDrop;
        
        [Header("P-Shooter Events")]
        public bool onShotByGun;
        public bool onShootPlayerWithParticle;

        [Header("Trigger Events")]
        public bool onPlayerTriggerEnter;
        public bool onPlayerTriggerStay;
        public bool onPlayerTriggerExit;

        [Header("Collider Events")]
        public bool onPlayerColliderEnter;
        public bool onPlayerColliderStay;
        public bool onPlayerColliderExit;
        [Header("Options")]
        private float enterTime = -1001f;
        [Tooltip("Instead of triggering OnColliderStay and OnTriggerStay events every frame, we only trigger at this regular interval in seconds")]
        public float stayTriggerInterval = 0.25f;
        void Start()
        {
            if (!onInteract)
            {
                DisableInteractive = true;
            }
        }


#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset()
        {
            LocalEventTriggerEditor.SetupLocalEventTrigger(this);
        }
#endif

        public void Trigger()
        {
            targetUdon.SendCustomEvent(targetUdonEvent);
        }

        public override void Interact()
        {
            if (onInteract)
            {
                Trigger();
            }
        }

        public override void OnPickup()
        {
            if (onPickup)
            {
                Trigger();
            }
        }

        public override void OnPickupUseDown()
        {
            if (onPickupUseDown)
            {
                Trigger();
            }
        }

        public override void OnPickupUseUp()
        {
            if (onPickupUseUp)
            {
                Trigger();
            }
        }

        public override void OnDrop()
        {
            if (onDrop)
            {
                Trigger();
            }
        }


        public override void OnPlayerCollisionEnter(VRCPlayerApi player)
        {
            if (Utilities.IsValid(player) && player.isLocal)
            {
                enterTime = Time.timeSinceLevelLoad;
                if (onPlayerColliderEnter)
                {
                    Trigger();
                }
            }
        }

        public override void OnPlayerCollisionExit(VRCPlayerApi player)
        {
            if (Utilities.IsValid(player) && player.isLocal)
            {
                enterTime = -1001f;
                if (onPlayerColliderExit)
                {
                    Trigger();
                }
            }
        }

        public override void OnPlayerCollisionStay(VRCPlayerApi player)
        {
            if (Utilities.IsValid(player) && player.isLocal && onPlayerColliderStay)
            {
                int intervals = calcIntervalsSinceLastFrame();
                for (int i = 0; i < intervals; i++)
                {
                    Trigger();
                }
            }
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (Utilities.IsValid(player) && player.isLocal)
            {
                enterTime = Time.timeSinceLevelLoad;
                if (onPlayerTriggerEnter)
                {
                    Trigger();
                }
            }
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (Utilities.IsValid(player) && player.isLocal)
            {
                enterTime = -1001f;
                if (onPlayerTriggerExit)
                {
                    Trigger();
                }
            }
        }

        public override void OnPlayerTriggerStay(VRCPlayerApi player)
        {
            if (Utilities.IsValid(player) && player.isLocal && onPlayerTriggerStay && onPlayerTriggerStay)
            {
                int intervals = calcIntervalsSinceLastFrame();
                for (int i = 0; i < intervals; i++)
                {
                    Trigger();
                }
            }
        }

        public void OnParticleCollision(GameObject other)
        {
            if (!Utilities.IsValid(other))
            {
                return;
            }
            if (onShootPlayerWithParticle)
            {
                Player player = other.GetComponent<Player>();
                if (Utilities.IsValid(player) && player.IsOwnerLocal())
                {
                    Trigger();
                    return;
                }
            }
            if (onShotByGun)
            {
                P_Shooter shooter = other.GetComponent<P_Shooter>();
                if (!Utilities.IsValid(shooter))
                {
                    shooter = other.GetComponentInParent<P_Shooter>();
                }

                if (Utilities.IsValid(shooter) && Networking.LocalPlayer.IsOwner(shooter.gameObject))
                {
                    Trigger();
                }
            }
        }
        private int triggeredIntervals = 0;
        private int intervalCount = 0;
        public int calcIntervalsSinceLastFrame()
        {
            if (stayTriggerInterval <= 0 || enterTime <= 0)
            {
                triggeredIntervals = 1;
                return 1;
            }
            intervalCount = Mathf.CeilToInt((Time.timeSinceLevelLoad - enterTime) / stayTriggerInterval);
            if (intervalCount - triggeredIntervals > 0)
            {
                int difference = intervalCount - triggeredIntervals;
                triggeredIntervals = intervalCount;
                return difference;
            }
            return 0;
        }
    }
}
