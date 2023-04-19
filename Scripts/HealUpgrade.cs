
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    public class HealUpgrade : UdonSharpBehaviour
    {
        public bool synced_cooldown;
        public bool drop_on_disable = true;
        public GameObject hideObjectOnDisable;
        public PlayerHandler player_handler;

        [Header("Requires a rigidbody if On_interact is off")]
        public bool on_interact = false;
        public int heal_amount = 50;
        public float heal_cooldown = 10.0f;

        [Header("If zero, it heals only once, otherwise it heals the full amount every second for the duration")]
        public float heal_over_time = 0.0f;
        public bool affects_shield = true;
        public bool affects_health = true;

        private float last_heal = -1001f;
        private float first_heal = -1001f;
        private Vector3 startPos;
        private Quaternion startRot;
        private bool start_ran = false;
        private bool heal_enabled = true;
        void Start()
        {
            if (player_handler == null)
            {
                player_handler = GameObject.Find("__GUN MANAGER__").GetComponentInChildren<PlayerHandler>();
            }
            if (!on_interact || GetComponent<VRC_Pickup>() != null)
            {
                DisableInteractive = true;
            }
            startPos = transform.localPosition;
            startRot = transform.localRotation;
            start_ran = true;
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset()
        {
            if (player_handler == null)
            {
                player_handler = GameObject.FindObjectOfType<PlayerHandler>();
            }
            if (hideObjectOnDisable == null)
            {
                hideObjectOnDisable = gameObject;
            }
        }
#endif

        public void OnEnable()
        {
            last_heal = -1001f;
            first_heal = -1001f;
            if (start_ran)
            {
                transform.localPosition = startPos;
                transform.localRotation = startRot;
            }
        }

        public void OnTriggerStay(Collider other)
        {
            if (player_handler == null || player_handler._localPlayer == null || (player_handler.scores != null && player_handler._localPlayer.team == 0) || !Utilities.IsValid(other) || other == null || on_interact)
            {
                return;
            }
            if (player_handler._localPlayer == other.GetComponent<Player>())
            {
                Heal();
            }
        }
        // public void OnCollisionEnter(Collision other)
        // {
        //     if (player_handler == null || player_handler._localPlayer == null || (player_handler.scores != null && player_handler._localPlayer.team == 0) || !Utilities.IsValid(other) || other == null || on_interact || other.collider == null)
        //     {
        //         return;
        //     }
        //     if (player_handler._localPlayer == other.collider.GetComponent<Player>())
        //     {
        //         Heal();
        //     }
        // }

        public override void Interact()
        {
            Heal();
        }

        public override void OnPickupUseDown()
        {
            Heal();
        }
        public void _OnPickupUseDown()
        {
            Heal();
        }

        public void Heal()
        {
            if (heal_over_time > 0 && last_heal + 1f > Time.timeSinceLevelLoad || !heal_enabled)
            {
                return;
            }
            bool healed = player_handler.IncreaseHealth(heal_amount, affects_health, affects_shield);
            if (!healed) //was already full health
            {
                return;
            }
            last_heal = Time.timeSinceLevelLoad;
            if (first_heal < 0)
            {
                first_heal = last_heal;
            }
            if (heal_cooldown > 0 && (on_interact || heal_over_time == 0 || (heal_over_time > 0 && first_heal + heal_over_time < last_heal)))
            {
                if (synced_cooldown)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(Disable));
                }
                else
                {
                    Disable();
                }
            }
        }

        public void Disable()
        {
            VRC_Pickup pickup = GetComponent<VRC_Pickup>();
            if (pickup != null && drop_on_disable)
            {
                pickup.Drop();
            }
            SendCustomEventDelayedSeconds(nameof(Enable), heal_cooldown);
            heal_enabled = false;
            if (hideObjectOnDisable != null)
            {
                hideObjectOnDisable.SetActive(false);
            }
        }

        public void Enable()
        {
            if (hideObjectOnDisable != null)
            {
                hideObjectOnDisable.SetActive(true);
            }
            heal_enabled = true;
        }
    }
}