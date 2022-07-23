
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class P_Shooter : UdonSharpBehaviour
{
    [System.NonSerialized][FieldChangeCallback(nameof(shoot_state))] [UdonSynced(UdonSyncMode.None)] public int _shoot_state = 0;
    [System.NonSerialized] public const int SHOOT_STATE_IDLE = 0;
    [System.NonSerialized] public const int SHOOT_STATE_SHOOT = 1;
    [System.NonSerialized] public const int SHOOT_STATE_RELOAD = 2;
    [System.NonSerialized] public const int SHOOT_STATE_RESPAWN = 3;

    public bool melee = false;
    public bool rare = false;
    public bool track_ammo_mag = false;
    public bool track_ammo_reserve = false;
    public bool auto_reload = true;
    public bool unique_gunshot_audio_sources = false;
    public int base_damage = 10;
    public float melee_speed_boost_multiplier = 2;
    [System.NonSerialized] [FieldChangeCallback(nameof(ammo_mag))][UdonSynced(UdonSyncMode.None)] public int _ammo_mag = -1;
    [System.NonSerialized] [FieldChangeCallback(nameof(ammo_mag_capacity))] [UdonSynced(UdonSyncMode.None)] public int _ammo_mag_capacity = -1;
    [System.NonSerialized] [FieldChangeCallback(nameof(ammo_reserve))] [UdonSynced(UdonSyncMode.None)] public int _ammo_reserve = -1;
    // [System.NonSerialized] [FieldChangeCallback(nameof(ammo_reserve_max))][UdonSynced(UdonSyncMode.None)] public int _ammo_reserve_max = -1;
    public int _starting_ammo_mag_capacity = -1;
    public int _starting_ammo_reserve = -1;
    [FieldChangeCallback(nameof(upgrades))] [UdonSynced(UdonSyncMode.None)] public string[] _upgrades = new string[0];//names of all the upgrades. These get applied to the animator as they get picked up (through SetBool). When the gun is reset, or an upgrade is lost, we pop it off the list
    [System.NonSerialized] public string[] local_upgrades = new string[0];//local copy so we can diff
    public Animator animator;
    public SmartPickupSync smartPickup;
    public ParticleSystem particle_shooter;
    public SecondGrip grip = null;
    public Scope scope = null;
    public AudioSource sound_source;
    public AudioClip sound_shoot;
    public AudioClip sound_melee_boost;
    public AudioClip sound_empty;
    public AudioClip sound_reload;
    public AudioClip sound_reload_stop;
    public AudioClip sound_upgrade;
    public AudioClip sound_bullet_impact_environment;

    public GameObject[] local_held_objects;

    private bool local_trigger = false;
    private bool local_reload = false;
    private bool local_load = false;
    [System.NonSerialized] public bool local_held = false;
    [System.NonSerialized] public Quaternion rest_local_rotation;
    private bool ran_start = false;
    [System.NonSerialized] public int id = -1;

    private GunManager manager;
    [System.NonSerialized] [UdonSynced(UdonSyncMode.None)] public int spawn_id = -1;

    //callbacks
    public int shoot_state
    {
        get => _shoot_state;
        set
        {
            _shoot_state = value;
            if (animator != null)
            {
                animator.SetInteger("shoot_state", _shoot_state);
            }
            if (melee && _shoot_state == SHOOT_STATE_SHOOT)
            {
                sound_source.clip = sound_melee_boost;
                sound_source.Play();

                if (Networking.LocalPlayer.IsOwner(gameObject) && manager != null && manager.player_handler != null)
                {
                    Networking.LocalPlayer.SetRunSpeed(manager.player_handler.run_speed * melee_speed_boost_multiplier);
                    Networking.LocalPlayer.SetStrafeSpeed(manager.player_handler.strafe_speed * melee_speed_boost_multiplier);
                    Networking.LocalPlayer.SetWalkSpeed(manager.player_handler.walk_speed * melee_speed_boost_multiplier);
                    Networking.LocalPlayer.SetJumpImpulse(manager.player_handler.jump_impulse * (1 + (1 - melee_speed_boost_multiplier) / 2));
                }
            } else if (melee)
            {
                sound_source.Stop();
                if (Networking.LocalPlayer.IsOwner(gameObject) && manager != null && manager.player_handler != null)
                {
                    Networking.LocalPlayer.SetRunSpeed(manager.player_handler.run_speed);
                    Networking.LocalPlayer.SetStrafeSpeed(manager.player_handler.strafe_speed);
                    Networking.LocalPlayer.SetWalkSpeed(manager.player_handler.walk_speed);
                    Networking.LocalPlayer.SetJumpImpulse(manager.player_handler.jump_impulse);
                }
            }
        }
    }
    public int ammo_mag
    {
        get => _ammo_mag;
        set
        {
            _ammo_mag = value;
            if (animator != null)
            {
                if (track_ammo_mag)
                {
                    animator.SetInteger("ammo_mag", _ammo_mag);
                } else
                {
                    animator.SetInteger("ammo_mag", 1);
                }
            }
        }
    }
    public int ammo_mag_capacity
    {
        get => _ammo_mag_capacity;
        set
        {
            _ammo_mag_capacity = value;
            if (animator != null)
            {
                if (track_ammo_mag)
                {
                    animator.SetInteger("ammo_mag_max", _ammo_mag_capacity);
                }
                else
                {
                    animator.SetInteger("ammo_mag_max", 1);
                }
            }
        }
    }
    public int ammo_reserve
    {
        get => _ammo_reserve;
        set
        {
            _ammo_reserve = value;
            if (animator != null)
            {
                if (track_ammo_reserve)
                {
                    animator.SetInteger("ammo_reserve", _ammo_reserve);
                }
                else
                {
                    animator.SetInteger("ammo_reserve", 999);
                }
            }
        }
    }
    // public int ammo_reserve_max
    // {
    //     get => _ammo_reserve_max;
    //     set
    //     {
    //         _ammo_reserve_max = value;
    //         if (animator != null)
    //         {
    //             if (track_ammo_reserve)
    //             {
    //                 animator.SetInteger("ammo_reserve_max", _ammo_reserve_max);
    //             }
    //             else
    //             {
    //                 animator.SetInteger("ammo_reserve_max", 999);
    //             }
    //         }
    //     }
    // }
    public string[] upgrades
    {
        get => _upgrades;
        set
        {
            _upgrades = value;
            if (animator != null)
            {
                for (int i = 0; i < local_upgrades.Length; i++)
                {
                    animator.SetBool(local_upgrades[i], false);
                }
                for (int i = 0; i < _upgrades.Length; i++)
                {
                    animator.SetBool(_upgrades[i], true);
                }
            }
            local_upgrades = _upgrades;
        }
    }
    void Start()
    {
        if (animator != null)
        {
            animator.SetBool("local", local_held);
        }

        shoot_state = shoot_state;
        ammo_mag = ammo_mag;
        ammo_mag_capacity = ammo_mag_capacity;
        ammo_reserve = ammo_reserve;
        upgrades = upgrades;
        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            _Reset();
            shoot_state = SHOOT_STATE_IDLE;
            RequestSerialization();
        }
        if (local_held_objects != null && local_held_objects.Length > 0)
        {
            foreach (GameObject obj in local_held_objects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }

        if (grip != null)
        {
            grip.RecordRestTransforms();
            grip.DisablePickup();
        }
        rest_local_rotation = transform.localRotation;
        ran_start = true;
    }

    public void _Register(GunManager gunManager, int new_id)
    {
        manager = gunManager;
        id = new_id;
    }

    public void _TakeOwnership()
    {
        if (!Networking.LocalPlayer.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        if (player != null && player.IsValid() && player.isLocal && !local_held && (shoot_state == SHOOT_STATE_SHOOT || shoot_state == SHOOT_STATE_RELOAD))
        {
            _OnDrop();
        }
    }

    public void _ResetLocalState()
    {
        local_trigger = false;
        local_reload = false;
    }

    public void _Reset()
    {
        _ResetLocalState();
        if (track_ammo_mag)
        {
            ammo_mag_capacity = _starting_ammo_mag_capacity;
            ammo_mag = ammo_mag_capacity;
        }
        if (track_ammo_reserve)
        {
            ammo_reserve = _starting_ammo_reserve;
        }
        upgrades = new string[0];
        shoot_state = SHOOT_STATE_RESPAWN;
        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            RequestSerialization();
        }
    }

    public void SetSpawnId(int id)
    {
        spawn_id = id;
    }

    public void ClearSpawnId()
    {
        manager.ClearSpawn(spawn_id);
        spawn_id = -1;
    }

    public void _OnPickup()
    {
        if (smartPickup != null && smartPickup.pickup != null)
        {
            if (manager == null || manager.player_handler == null || manager.player_handler._localPlayer == null)
            {
                smartPickup.pickup.Drop();
                return;
            }
            _TakeOwnership();
            local_held = true;
            animator.SetBool("local", local_held);
            shoot_state = SHOOT_STATE_IDLE;
            ClearSpawnId();
            if (Networking.LocalPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Left) == smartPickup.pickup)
            {
                manager.player_handler._localPlayer.left_pickup_index = id;
                manager.player_handler._localPlayer.RequestSerialization();

            }
            else
            {
                manager.player_handler._localPlayer.right_pickup_index = id;
                manager.player_handler._localPlayer.RequestSerialization();
            }
            RequestSerialization();

            if (local_held_objects != null && local_held_objects.Length > 0)
            {
                foreach (GameObject obj in local_held_objects)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                    }
                }
            }
            if (grip != null)
            {
                grip.AllowPickup();
            }
            if (scope != null)
            {
                scope.SetCameraActive(true);
            }
        }
    }

    public void _OnPickupUseDown()
    {
        if (manager == null || manager.player_handler == null || manager.player_handler._localPlayer == null || manager.player_handler._localPlayer.last_safe + 0.1f > Time.timeSinceLevelLoad)
        {
            return;
        }
        local_trigger = true;
        local_held = true;
        animator.SetBool("local", local_held);
        _ShootStart();
    }

    public void _OnPickupUseUp()
    {
        if (manager == null || manager.player_handler == null || manager.player_handler._localPlayer == null)
        {
            return;
        }
        local_trigger = false;
        _ShootStop();
    }

    public void _OnDrop()
    {
        if (manager == null || manager.player_handler == null || manager.player_handler._localPlayer == null)
        {
            return;
        }
        animator.SetBool("local", local_held);
        shoot_state = SHOOT_STATE_RESPAWN;
        _ResetLocalState();

        if (manager.player_handler._localPlayer.left_pickup_index == id)
        {
            manager.player_handler._localPlayer.left_pickup_index = -1;
            manager.player_handler._localPlayer.RequestSerialization();
        }
        else if (manager.player_handler._localPlayer.right_pickup_index == id)
        {
            manager.player_handler._localPlayer.right_pickup_index = -1;
            manager.player_handler._localPlayer.RequestSerialization();
        }
        
        RequestSerialization();
        local_held = false;

        if (local_held_objects != null && local_held_objects.Length > 0)
        {
            foreach (GameObject obj in local_held_objects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
        
        if (grip != null)
        {
            grip.DisablePickup();
            grip.Reset();
        }

        if (scope != null)
        {
            scope.SetCameraActive(false);
        }
    }

    public void LateUpdate()
    {
        if (local_held && !melee)
        {
            if (track_ammo_mag && (ammo_mag < ammo_mag_capacity) && !local_trigger && particle_shooter != null)
            {
                if (!local_reload)
                {
                    float down_angle = Vector3.Angle(Vector3.down, particle_shooter.transform.forward);
                    if (down_angle < 30 || down_angle > 150)
                    {
                        local_reload = true;
                        _ReloadStart();
                    }
                    if (!Networking.LocalPlayer.IsUserInVR() && Input.GetKey(KeyCode.E))
                    {
                        local_reload = true;
                        _ReloadStart();
                    }
                }
                else
                {
                    float down_angle = Vector3.Angle(Vector3.down, particle_shooter.transform.forward);
                    if (down_angle >= 30 && down_angle <= 150)
                    {
                        local_reload = false;
                        _ReloadStop();
                    }
                    if (!Networking.LocalPlayer.IsUserInVR() && !Input.GetKey(KeyCode.E))
                    {
                        local_reload = false;
                        _ReloadStop();
                    }
                }
            }
            else
            {
                if (local_reload)
                {
                    local_reload = false;
                    _ReloadStop();
                }
            }
        }
        if (grip != null && ran_start)
        {
            if (grip.reparented || grip.smartPickup.isHeld)
            {
                grip.transform.parent = null;
                if (grip.gunModel != null)
                {
                    grip.gunModel.localPosition = grip.gunModel.localPosition / 2f;
                    grip.gunModel.localRotation = Quaternion.Slerp(Quaternion.identity, grip.gunModel.localRotation, 0.5f);
                }
            }
            else
            {
                grip.transform.parent = transform.parent;
                grip.ApplyRestTransforms();
            }
            Vector3 parentPos = Vector3.zero;
            Quaternion parentRot = Quaternion.identity;
            Vector3 gripPos = parentPos;
            if (transform.parent != null)
            {
                if (smartPickup != null && smartPickup.pickup != null)
                {
                    gripPos = smartPickup.pickup.ExactGrip.position;
                }
                parentPos = transform.parent.position;
                parentRot = transform.parent.rotation;
            }
            float distance = Vector3.Distance(grip.transform.position, gripPos);
            Vector3 flat_z_rest = grip.rest_local_pos;
            flat_z_rest.z = 0;
            Vector3 targetPos = grip.transform.position - ((transform.rotation * Quaternion.Inverse(rest_local_rotation)) * flat_z_rest);
            // targetPos = grip.transform.position;
            Quaternion lookAtRotation = Quaternion.LookRotation(targetPos - transform.position, parentRot * Vector3.up);
        // transform.LookAt(targetPos, parentRot * Vector3.up);

            if (!Networking.LocalPlayer.IsOwner(gameObject) || !Input.GetKey(KeyCode.LeftAlt))
            {
                transform.rotation = lookAtRotation * rest_local_rotation;
            }
        }
    }
    public void _StopParticles()
    {
        if (local_held && !melee)
        {
            if (particle_shooter != null)
            {
                particle_shooter.Stop();
            }
            _EmptyFXCallback();
        }
    }
    
    //Local Player Actions
    public void _ShootStart()
    {
        if (shoot_state == SHOOT_STATE_IDLE || shoot_state == SHOOT_STATE_SHOOT || (shoot_state == SHOOT_STATE_RELOAD && ammo_mag > 0))
        {
            if (track_ammo_mag)
            {
                if (ammo_mag <= 0)
                {
                    if (auto_reload)
                    {
                        if (!track_ammo_reserve || ammo_reserve >= 0)
                        {
                            _ReloadStart();
                            return;
                        } else
                        {
                            _StopParticles();
                            ammo_mag = -1;
                            RequestSerialization();
                        }
                    } else
                    {
                        _StopParticles();
                        ammo_mag = -1;
                        RequestSerialization();
                    }
                }
            }
            if (shoot_state != SHOOT_STATE_SHOOT)
            {
                shoot_state = SHOOT_STATE_SHOOT;
                RequestSerialization();
            }
        }
    }

    public void _ShootStop()
    {
        if (shoot_state == SHOOT_STATE_SHOOT)
        {
            shoot_state = SHOOT_STATE_IDLE;
            RequestSerialization();
        }
    }

    public void _ReloadStart()
    {
        if ((shoot_state == SHOOT_STATE_IDLE) && !(track_ammo_reserve && ammo_reserve <= 0))
        {
            shoot_state = SHOOT_STATE_RELOAD;
            RequestSerialization();
        }
    }

    public void _ReloadStop()
    {
        if (shoot_state == SHOOT_STATE_RELOAD)
        {
            shoot_state = SHOOT_STATE_IDLE;
            RequestSerialization();
        }
    }

    //Animation Callbacks
    public void _ShootFXCallback()
    {
        if (!local_held || !track_ammo_mag || ammo_mag > 0)
        {
            if (particle_shooter != null)
            {
                particle_shooter.Play();
            }
            if (sound_source != null)
            {
                if (melee)
                {
                    sound_source.clip = sound_melee_boost;
                    sound_source.Play();
                } else
                {
                    if (unique_gunshot_audio_sources)
                    {
                        AudioSource.PlayClipAtPoint(sound_shoot, sound_source.transform.position);
                    }
                    else
                    {
                        sound_source.clip = sound_shoot;
                        sound_source.Play();
                    }
                }
            }
        }
    }
    public void _EmptyFXCallback()
    {
        if (sound_source != null)
        {
            sound_source.clip = sound_empty;
            sound_source.Play();
        }
    }
    public void _ReloadFXCallback()
    {
        if (sound_source != null)
        {
            sound_source.clip = sound_reload;
            sound_source.Play();
        }
    }
    public void _ReloadStopFXCallback()
    {
        if (sound_source != null)
        {
            sound_source.clip = sound_reload_stop;
            sound_source.Play();
        }
    }
    public void _UpgradeFXCallback()
    {
        if (sound_source != null)
        {
            sound_source.clip = sound_upgrade;
            sound_source.Play();
        }
    }
    public void _ReloadCallbackFull()
    {
        if (local_held)
        {
            if (ammo_mag < 0)
            {
                ammo_mag = 0;
            }
            if (track_ammo_mag && ammo_mag < ammo_mag_capacity)
            {
                if (track_ammo_reserve)
                {
                    if (ammo_reserve > 0)
                    {
                        int total = ammo_reserve + ammo_mag;
                        ammo_mag = Mathf.Min(ammo_mag_capacity, total);
                        ammo_reserve = total - ammo_mag;
                        RequestSerialization();
                    }
                }
                else
                {
                    ammo_mag = ammo_mag_capacity;
                    RequestSerialization();
                }
            }
            else
            {
                shoot_state = SHOOT_STATE_IDLE;
                RequestSerialization();
            }
        }
    }
    public void _ReloadCallbackSingle()
    {
        if (local_held)
        {
            if (ammo_mag < 0)
            {
                ammo_mag = 0;
            }
            if (track_ammo_mag && ammo_mag < ammo_mag_capacity)
            {
                if (track_ammo_reserve)
                {
                    if (ammo_reserve > 0 && ammo_mag < ammo_mag_capacity)
                    {
                        ammo_reserve -= 1;
                        ammo_mag += 1;
                        RequestSerialization();
                    }
                }
                else
                {
                    ammo_mag += 1;
                    RequestSerialization();
                }
            } else
            {
                shoot_state = SHOOT_STATE_IDLE;
                RequestSerialization();
            }
        }
    }
    public void _ShootCallbackFull()
    {
        if (local_held && track_ammo_mag && ammo_mag >= 0)
        {
            ammo_mag = -1;
            _StopParticles();
            RequestSerialization();
        }
    }
    public void _ShootCallbackSingle()
    {
        if (local_held && track_ammo_mag)
        {
            if (ammo_mag >= 0)
            {
                ammo_mag -= 1;
                RequestSerialization();
            } else
            {
                ammo_mag = -1;
                _StopParticles();
                RequestSerialization();
            }
        }
    }
    public void _UpgradeCallback()
    {
        if (sound_source != null)
        {
            sound_source.clip = sound_upgrade;
            sound_source.Play();
        }
    }

    public void _RespawnCallback()
    {
        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            if (smartPickup != null)
            {
                smartPickup.Respawn();
            }
            _Reset();
        }
    }

    // public override void OnDeserialization()
    // {
    //     shoot_state = shoot_state;
    //     ammo_mag = ammo_mag;
    //     ammo_mag_capacity = ammo_mag_capacity;
    //     ammo_reserve = ammo_reserve;
    // }

}
