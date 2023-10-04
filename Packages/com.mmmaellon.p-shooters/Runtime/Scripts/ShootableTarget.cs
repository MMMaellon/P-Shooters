using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;

namespace MMMaellon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ShootableTarget : UdonSharpBehaviour
    
    {
        public Animator animator;
        public int maxHealth = 10;
        [UdonSynced(UdonSyncMode.None), System.NonSerialized, FieldChangeCallback(nameof(health))]
        public int _health = 10;
        public bool AllowClick = true;
        float lastHealthChange = 0;
        public int health
        {
            get => _health;
            set
            {
                lastHealthChange = Time.timeSinceLevelLoad;
                animator.enabled = true;
                if (value < _health)
                {
                    animator.SetTrigger("hit");
                }
                _health = maxHealth > 0 ? Mathf.Min(maxHealth, value) : value;
                if (maxHealth > 0)
                {
                    animator.SetFloat("health", (float)_health / (float)maxHealth);
                }
                else
                {
                    animator.SetFloat("health", _health > 0 ? 1.0f : -1001f);
                }
                if (Networking.LocalPlayer.IsOwner(gameObject))
                {
                    RequestSerialization();
                }
                if (value <= 0 && resetTimer > 0)
                {
                    SendCustomEventDelayedSeconds(nameof(ResetHealthCallback), resetTimer);
                }
            }
        }

        public float resetTimer = 10;
        void Start()
        {
            ResetHealth();
        }

        public override void Interact()
        {
            if (AllowClick)
            {
                ToggleDestroyed();
            }
        }

        public void ResetHealth()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            health = maxHealth;
        }

        public void ResetHealthCallback()
        {
            if (!Networking.LocalPlayer.IsOwner(gameObject) || Mathf.Abs(lastHealthChange + resetTimer - Time.timeSinceLevelLoad) > 0.1f)
            {
                return;
            }
            ResetHealth();
        }

        public void ToggleDestroyed()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            if (health <= 0)
            {
                ResetHealth();
            }
            else
            {
                health = -1001;
            }
        }

        int damage;
        public void OnParticleCollision(GameObject other)
        {
            if (!Utilities.IsValid(other))
            {
                return;
            }

            P_Shooters.P_Shooter shooter = other.GetComponentInParent<P_Shooters.P_Shooter>();
            if (!Utilities.IsValid(shooter) || !shooter.sync.IsLocalOwner())
            {
                return;
            }

            damage = shooter.damage;

            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            if (damage > health)
            {
                health = 0;
            }
            else
            {
                health -= damage;
            }
        }

        public void OnCollisionEnter(Collision other)
        {
            if (!Utilities.IsValid(other.collider))
            {
                return;
            }
            P_Shooters.P_Shooter shooter = other.collider.GetComponentInParent<P_Shooters.P_Shooter>();
            if (!Utilities.IsValid(shooter) || !shooter.sync.IsLocalOwner())
            {
                return;
            }

            damage = shooter.damage;

            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            if (damage > health)
            {
                health = 0;
            }
            else
            {
                health -= damage;
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (!Utilities.IsValid(other))
            {
                return;
            }

            P_Shooters.P_Shooter shooter = other.GetComponentInParent<P_Shooters.P_Shooter>();
            if (!Utilities.IsValid(shooter) || !shooter.sync.IsLocalOwner())
            {
                return;
            }

            damage = shooter.damage;

            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            if (damage > health)
            {
                health = 0;
            }
            else
            {
                health -= damage;
            }
        }

        public void DisableAnimator()
        {
            animator.enabled = false;
        }
    }
}