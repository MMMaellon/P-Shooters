
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
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual), RequireComponent(typeof(SmartObjectSync)), RequireComponent(typeof(Animator))]
    public class P_Shooter : SmartObjectSyncListener
    {
        public int damage = 15;
        public bool damageOnTriggerEnter = false;
        public bool damageOnParticleCollision = true;
        public bool damageWhenNotHeld = false;
        public bool selfDamage = false;
        public bool toggleableDamage = false;
        public bool toggleOffOnDrop = true;
        [FieldChangeCallback(nameof(shootSpeed))]
        public float _shootSpeed = 1.0f;
        [FieldChangeCallback(nameof(reloadSpeed))]
        public float _reloadSpeed = 1.0f;
        public float shootSpeed {
            get => _shootSpeed;
            set {
                    _shootSpeed = value;
                    if(Utilities.IsValid(animator)){
                        animator.SetFloat("shoot_speed", value);
                    }
                }
            }
        public float reloadSpeed {
            get => _reloadSpeed;
            set {
                    _reloadSpeed = value;
                    if(Utilities.IsValid(animator)){
                        animator.SetFloat("reload_speed", value);
                    }
                }
            }
        public bool printDebugMessages = true;
        public void _print(string message)
        {
            if (printDebugMessages)
            {
                Debug.Log("<color=yellow>[P-Shooters P_Shooter.cs] " + name + ": </color>" + message);
            }
        }

        [System.NonSerialized, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(state))]
        public int _state = STATE_IDLE;
        //negative values constitute custom states. mainly used for multi-step reloading
        [System.NonSerialized]
        public const int STATE_IDLE = 0;//gun is just sitting there
        [System.NonSerialized]
        public const int STATE_SHOOT = 1;//gun is shooting. Can only get here from the idle state if we had ammo
        [System.NonSerialized]
        public const int STATE_EMPTY = 2;//gun is trying to shoot, but is out of ammo.
        [System.NonSerialized]
        public const int STATE_RELOAD = 3;//gun is not able to shoot because you're stuck in a reload animation
        [System.NonSerialized]
        public const int STATE_DISABLED = 4;//gun jammed or overheated or something
        public ParticleSystem shootParticles;
        public ParticleSystem onHitPlayerParticles;

        [Header("Required Components")]
        public SmartObjectSync sync;
        public Animator _animator;
        public Animator animator{
            get
            {
#if COMPILER_UDONSHARP || !UNITY_EDITOR
                _animator.enabled = true;
#endif
                return _animator;
            }
            set
            {
                _animator = value;
            }
        }
        public Transform gunParent;

        [HideInInspector]
        public AmmoTracker ammo;

        [Header("Sounds")]
        public AudioSource gunshotSource;
        public AudioClip[] gunshots;
        public AudioSource onHitPlayerSource;
        public AudioClip[] onHitPlayerSounds;
        [Tooltip("There's a hard limit to how many audio clips can be playing at one time. If you have too many, background sounds and players might become muted until you rejoin the world.")]
        public bool overlapSoundEffects = false;
        public int state{
            get => _state;
            set
            {
                if (_state != value)
                {
                    if (Utilities.IsValid(ammo))
                    {
                        switch (_state)
                        {
                            case (STATE_RELOAD):
                                {
                                    ammo.ReloadEndFX();
                                    break;
                                }
                        }

                        switch (value)
                        {
                            case (STATE_RELOAD):
                                {
                                    ammo.ReloadFX();
                                    break;
                                }
                            case (STATE_EMPTY):
                                {
                                    ammo.OutOfAmmoFX();
                                    break;
                                }
                        }
                    }
                    _state = value;
                    if (sync.IsLocalOwner())
                    {
                        RequestSerialization();
                    }
                }
                animator.SetInteger("state", _state);
                _print("STATE: " + StateToString(state));
            }
        }

        public override void OnChangeState(SmartObjectSync s, int oldState, int newState)
        {
            _print("OnChangeState");
            animator.SetInteger("pickup_state", newState);
            if (newState == SmartObjectSync.STATE_TELEPORTING)
            {
                animator.SetTrigger("teleport");
            }
            if (Utilities.IsValid(ammo))
            {
                ammo.loop = sync.IsHeld();
            }
        } 

        public override void OnChangeOwner(SmartObjectSync s, VRCPlayerApi oldPlayer, VRCPlayerApi newPlayer)
        {
            animator.SetBool("local", sync.IsLocalOwner());
        }


#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void Reset(){
            P_Shooter.SetupShooter(this);
        }

        public static void SetupShooter(P_Shooter shooter)
        {
            if (!Utilities.IsValid(shooter) || (Utilities.IsValid(shooter.sync) && Utilities.IsValid(shooter.animator) && shooter.sync.gameObject == shooter.gameObject && shooter.animator.gameObject == shooter.gameObject))
            {
                //null or already set up
                return;
            }
            if (!Helper.IsEditable(shooter))
            {
                Helper.ErrorLog(shooter, "P-Shooter is not editable");
                return;
            }
            SerializedObject serialized = new SerializedObject(shooter);
            serialized.FindProperty("sync").objectReferenceValue = shooter.GetComponent<SmartObjectSync>();//must exist because we depend on it
            serialized.FindProperty("_animator").objectReferenceValue = shooter.GetComponent<Animator>();
            serialized.ApplyModifiedProperties();
        }
#endif
        [System.NonSerialized]
        public VRCPlayerApi _localPlayer;
        
        void Start()
        {
            _localPlayer = Networking.LocalPlayer;
            sync.AddListener(this);
            shootSpeed = shootSpeed;
            reloadSpeed = reloadSpeed;
        }

        public void EnableAnimator()
        {
            _animator.enabled = true;
        }

        public void DisableAnimator()
        {
            _animator.enabled = false;
        }

        public void ShootFX()
        {
            if (Utilities.IsValid(ammo) && sync.IsLocalOwner())
            {
                if (!ammo.ConsumeAmmo())
                {
                    state = STATE_EMPTY;
                    ammo.OutOfAmmoFX();
                }
            }
            if (state != STATE_SHOOT && state != STATE_IDLE)//no round was chambered during the shoot animation or we're not shooting
            {
                //might get disabled or run out of bullets during a burst fire
                return;
            }
            _print("ShootFX");
            if (Utilities.IsValid(shootParticles))
            {
                shootParticles.Play();
            }
            if (Utilities.IsValid(gunshotSource) && gunshots.Length > 0)
            {
                if (overlapSoundEffects)
                {
                    gunshotSource.PlayOneShot(gunshots[Random.Range(0, gunshots.Length)]);
                } else
                {
                    gunshotSource.clip = gunshots[Random.Range(0, gunshots.Length)];
                    gunshotSource.Play();
                }
            }
        }

        public void OnHitPlayerFX()
        {
            _print("OnHitPlayerFX");
            if (Utilities.IsValid(onHitPlayerParticles))
            {
                onHitPlayerParticles.Play();
            }
            if (Utilities.IsValid(onHitPlayerSource) && onHitPlayerSounds.Length > 0)
            {
                if (overlapSoundEffects)
                {
                    onHitPlayerSource.PlayOneShot(onHitPlayerSounds[Random.Range(0, onHitPlayerSounds.Length)]);
                }
                else
                {
                    onHitPlayerSource.clip = onHitPlayerSounds[Random.Range(0, onHitPlayerSounds.Length)];
                    onHitPlayerSource.Play();
                }
            }
        }

        public override void OnPickupUseDown()
        {
            if (!toggleableDamage)
            {
                if (state != STATE_IDLE)
                {
                    return;
                }

                if (Utilities.IsValid(ammo))
                {
                    ammo.Shoot();
                }
                else
                {
                    state = STATE_SHOOT;
                }
            } else
            {
                if (state != STATE_IDLE && state != STATE_SHOOT)
                {
                    return;
                }

                else if (state == STATE_IDLE)
                {
                    if (Utilities.IsValid(ammo))
                    {
                        ammo.Shoot();
                    } else
                    {
                        state = STATE_SHOOT;
                    }
                } else
                {
                    state = STATE_IDLE;
                }
            }
        }
        public override void OnPickupUseUp()
        {
            if (!toggleableDamage)
            {
                if (state == STATE_SHOOT || state == STATE_EMPTY)
                {
                    state = STATE_IDLE;
                }
            }
        }

        public override void OnDrop()
        {
            if (toggleableDamage && toggleOffOnDrop && state == STATE_SHOOT)
            {
                state = STATE_IDLE;
            }
        }

        public string StateToString(int value)
        {
            switch (value)
            {
                case (STATE_IDLE):
                    {
                        return "STATE_ARMED";
                    }
                case (STATE_SHOOT):
                    {
                        return "STATE_SHOOT";
                    }
                case (STATE_EMPTY):
                    {
                        return "STATE_EMPTY";
                    }
                case (STATE_RELOAD):
                    {
                        return "STATE_RELOAD";
                    }
                case (STATE_DISABLED):
                    {
                        return "STATE_DISABLED";
                    }
                default:
                    {
                        return "INVALID STATE";
                    }
            }
        }

        public void EmptyEnd()
        {
            if (!sync.IsLocalOwner() || state != STATE_EMPTY)
            {
                return;
            }

            state = STATE_IDLE;
        }

        public int CalcDamage()
        {
            if (damageWhenNotHeld && !sync.IsHeld())
            {
                return 0;
            }
            return damage;
        }
    }
}