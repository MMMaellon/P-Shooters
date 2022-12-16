
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Player : UdonSharpBehaviour
{

    // Who is the current owner of this object. Null if object is not currently in use. 
    [PublicAPI, System.NonSerialized]
    public VRCPlayerApi Owner;
    private int id = -1;
    [System.NonSerialized][UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(health))] public int _health = 100;
    [System.NonSerialized][UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(shield))] public int _shield = 300;
    [System.NonSerialized] public int local_health = 100;
    [System.NonSerialized] public int local_shield = 300;
    private float last_damage = -99;
    [System.NonSerialized][UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(damage))] public float _damage = 1f;
    [System.NonSerialized][UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(death_spot))] public Vector3 _death_spot = Vector3.zero;
    [System.NonSerialized][UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(kills))] public int _kills = 0;
    [System.NonSerialized][UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(team))] public int _team = 1;
    private Vector3 _local_death_spot = Vector3.zero;
    [System.NonSerialized][UdonSynced(UdonSyncMode.None)] public int left_pickup_index = -1;
    [System.NonSerialized][UdonSynced(UdonSyncMode.None)] public int right_pickup_index = -1;

    public ParticleSystem death_particles;

    private Animator animator;
    private int max_shield = 500;
    private float _speed = 1.0f;
    private float _jump = 1.0f;
    [System.NonSerialized] public float last_safe = -99;
    [System.NonSerialized] public float last_death = -99;

    public int health
    {
        get => _health;
        set
        {
            if (value >= local_health && player_handler != null)
            {
                if (value == player_handler.starting_health)
                {
                    local_health = value;
                    HealFX();
                }
            } else
            {
                local_health = value;
                last_damage = Time.timeSinceLevelLoad;
                DamageFX();
            }
            if (_health != value)
            {
                if (value >= _health)
                {
                    local_health = value;
                    HealFX();
                }
                _health = value;
                if (Owner != null && Owner.IsValid() && Owner.isLocal)
                {
                    local_health = value;
                    RequestSerialization();
                }
            }
        }
    }

    public int kills
    {
        get => _kills;
        set
        {
            _kills = value;
            RequestSerialization();

            if (player_handler != null && player_handler.scores != null)
            {
                player_handler.scores.UpdateScores();
            }
        }
    }
    public int team
    {
        get => _team;
        set
        {
            _team = value;
            RequestSerialization();

            if (player_handler != null && player_handler.scores != null)
            {
                player_handler.scores.UpdateScores();
            }
        }
    }

    public int shield
    {
        get => _shield;
        set
        {
            if (value >= local_shield && player_handler != null)
            {
                if (value == player_handler.starting_shield || last_damage + player_handler.shield_regen_delay/2f < Time.timeSinceLevelLoad)
                {
                    local_shield = value;
                }
            }
            else
            {
                local_shield = value;
                last_damage = Time.timeSinceLevelLoad;

                if (value <= 0)
                {
                    ShieldBreakFX();
                }
                else
                {
                    ShieldFX();
                }
            }
            if (_shield != value)
            {
                if (value >= _shield)
                {
                    local_shield = value;
                    HealFX();
                }
                _shield = value;
                if (Owner != null && Owner.IsValid() && Owner.isLocal)
                {
                    local_shield = value;
                    RequestSerialization();
                }
            }
        }
    }
    
    public float damage
    {
        get => _damage;
        set
        {
            if (_damage != value)
            {
                _damage = value;
                if (Owner != null && Owner.IsValid() && Owner.isLocal)
                {
                    RequestSerialization();
                }
            }
        }
    }
    
    // public float speed
    // {
    //     get => _speed;
    //     set
    //     {
    //         if (Owner != null && Owner.IsValid() && Owner.isLocal)
    //         {
    //             _speed = value;
    //             Networking.LocalPlayer.SetRunSpeed(player_handler.starting_speed * _speed);
    //             Networking.LocalPlayer.SetStrafeSpeed(player_handler.starting_speed * _speed);
    //             Networking.LocalPlayer.SetWalkSpeed(player_handler.starting_speed * _speed * 0.5f);
    //         }
    //     }
    // }

    // public float jump
    // {
    //     get => _jump;
    //     set
    //     {
    //         if (Owner != null && Owner.IsValid() && Owner.isLocal)
    //         {
    //             _jump = value;
    //             Networking.LocalPlayer.SetJumpImpulse(player_handler.starting_jump * _jump);
    //         }
    //     }
    // }

    public Vector3 death_spot
    {
        get => _death_spot;
        set
        {
            if (_death_spot != value)
            {
                _death_spot = value;
                if (Owner != null && Owner.IsValid() && Owner.isLocal)
                {
                    RequestSerialization();
                }
            }
            if (_local_death_spot != _death_spot)
            {
                _local_death_spot = death_spot;
                DeathFX();
            }
        }
    }

    private PlayerHandler player_handler;
    private CapsuleCollider player_collider;

    // This method will be called on all clients when the object is enabled and the Owner has been assigned.
    [PublicAPI]
    public void _OnOwnerSet()
    {
        // Initialize the object here
        if (player_collider != null)
        {
            // player_collider.enabled = !(Owner != null && Owner.IsValid() && Owner.isLocal);
            // player_collider.enabled = true;
            if (Owner != null && Owner.IsValid() && Owner.isLocal)
            {
                player_collider.radius = 0.001f;
            } else
            {
                player_collider.radius = 0.2f;
            }
        }

        if (player_handler != null && player_handler.scores != null)
        {
            player_handler.scores.DelayedUpdateScores();
        }
    }

    // This method will be called on all clients when the original owner has left and the object is about to be disabled.
    [PublicAPI]
    public void _OnCleanup()
    {
        // Cleanup the object here
        if (player_handler != null && player_handler.scores != null)
        {
            player_handler.scores.DelayedUpdateScores();
        }
    }
    
    public void Start()
    {
        player_collider = (CapsuleCollider)GetComponent(typeof(CapsuleCollider));
        animator = (Animator)GetComponent(typeof(Animator));
        // player_collider.enabled = Owner != null && Owner.IsValid() && Owner.isLocal;
        player_collider.enabled = true;
        health = health;
        shield = shield;
    }

    public void _Register(PlayerHandler players, int new_id)
    {
        player_handler = players;
        id = new_id;
    }

    public void Reset()
    {
        last_death = Time.timeSinceLevelLoad;
        health = player_handler.starting_health;
        shield = player_handler.starting_shield;
        max_shield = shield;
        // jump = 1f;
        // speed = 1f;
    }

    public void LateUpdate()
    {
        if (Owner != null && Owner.IsValid())
        {
            Vector3 feet_pos = Vector3.Lerp(Owner.GetBonePosition(HumanBodyBones.LeftFoot), Owner.GetBonePosition(HumanBodyBones.RightFoot), 0.5f);
            feet_pos = feet_pos != Vector3.zero ? feet_pos : Owner.GetPosition();
            Vector3 head_pos = Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            float headheight = Vector3.Distance(head_pos, feet_pos);
            float minimum = Mathf.Max(1f, headheight) + 0.2f;
            float vertical_minimum = Mathf.Max(0.5f, headheight) + 0.2f;
            transform.position = feet_pos;
            transform.localScale = new Vector3(minimum, vertical_minimum, minimum);
            transform.rotation = Quaternion.FromToRotation(Vector3.up, head_pos - feet_pos);
        }
    }

    public void OnParticleCollision(GameObject other)
    {
        if (player_handler == null || player_handler._localPlayer == null || other == null || other.transform.parent == null || other.transform.parent.parent == null || last_death + 3f > Time.timeSinceLevelLoad)
        {
            return;
        }
        if (Utilities.IsValid(other) && player_handler != null && player_handler._localPlayer != null && Owner.IsValid() && !(Owner.isLocal))
        {
            if (player_handler.damage_layers == (player_handler.damage_layers | (1 << other.gameObject.layer)))
            {
                if ( player_handler.scores != null && player_handler.scores.teams && player_handler._localPlayer.team == team)
                {
                    return;
                }
                VRC_Pickup leftPickup = Networking.LocalPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Left);
                VRC_Pickup rightPickup = Networking.LocalPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Right);
                int damage = 0;
                if (leftPickup != null && (leftPickup.gameObject == other.transform.parent.parent.gameObject || (other.transform.parent.parent.parent != null && leftPickup.gameObject == other.transform.parent.parent.parent.gameObject)))
                {
                    if (Owner.isLocal)
                    {
                        P_Shooter shooter = GetLeftShooter();
                        if (shooter != null && !shooter.self_damage)
                        {
                            return;
                        }
                    }
                    damage = player_handler._localPlayer.CalcDamage(true);
                    if (shield > 0)
                    {
                        shield -= damage;
                    }
                    else
                    {
                        health -= damage;
                    }
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(ShotBy) + "Left" + player_handler._localPlayer.id);
                    TriggerHitFX(damage);
                }
                else if (rightPickup != null && (rightPickup.gameObject == other.transform.parent.parent.gameObject || (other.transform.parent.parent.parent != null && rightPickup.gameObject == other.transform.parent.parent.parent.gameObject)))
                {
                    if (Owner.isLocal)
                    {
                        P_Shooter shooter = GetRightShooter();
                        if (shooter != null && !shooter.self_damage)
                        {
                            return;
                        }
                    }
                    damage = player_handler._localPlayer.CalcDamage(false);
                    if (shield > 0)
                    {
                        shield -= damage;
                    }
                    else
                    {
                        health -= damage;
                    }
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(ShotBy) + "Right" + player_handler._localPlayer.id);
                    TriggerHitFX(damage);
                }
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning("OnTriggerEnter");
        if (player_handler == null || player_handler._localPlayer == null || other == null)
        {
            return;
        }

        if (Utilities.IsValid(other) && Owner.IsValid() && !(Owner.isLocal))
        {
            Debug.LogWarning("other layer is " + other.gameObject.layer);
            if (player_handler.damage_layers == (player_handler.damage_layers | (1 << other.gameObject.layer)))
            {
                Debug.LogWarning("instakill layer");
                VRC_Pickup leftPickup = Networking.LocalPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Left);
                VRC_Pickup rightPickup = Networking.LocalPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Right);
                int damage = 0;
                Debug.LogWarning("got pickups");
                if (leftPickup != null && (leftPickup.gameObject == other.gameObject))
                {
                    damage = player_handler._localPlayer.CalcDamage(true);
                    if (shield > 0)
                    {
                        shield -= damage;
                    }
                    else
                    {
                        health -= damage;
                    }
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(ShotBy) + "Left" + player_handler._localPlayer.id);
                    TriggerHitFX(damage);
                }
                else if (rightPickup != null && (rightPickup.gameObject == other.gameObject))
                {
                    Debug.LogWarning("right hand");
                    damage = player_handler._localPlayer.CalcDamage(false);
                    if (shield > 0)
                    {
                        shield -= damage;
                    } else
                    {
                        health -= damage;
                    }
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(ShotBy) + "Right" + player_handler._localPlayer.id);
                    TriggerHitFX(damage);
                }
                // if ((leftPickup != null && leftPickup.gameObject == other.gameObject) || (rightPickup != null && rightPickup.gameObject == other.gameObject))
                // {
                //     SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(InstakillBy) + player_handler._localPlayer.id);
                //     // TriggerHitFX(99999);
                // }
            }
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (player_handler == null || last_safe + 0.9f > Time.timeSinceLevelLoad)
        {
            return;
        }
        if (Utilities.IsValid(other) && player_handler.safety_layers == (player_handler.safety_layers | (1 << other.gameObject.layer)))
        {
            last_safe = Time.timeSinceLevelLoad;
        }
    }

    public void TriggerHitFX(int damage)
    {
        if (last_death + 3f > Time.timeSinceLevelLoad || last_safe + 1f > Time.timeSinceLevelLoad)//invincible for 3 seconds
        {
            return;
        }
        if (damage >= shield && shield > 0)
        {
            // SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ShieldBreakFX));
            player_handler.ShieldBreakFX();
        }
        else if (shield > 0)
        {
            // SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ShieldFX));
            player_handler.ShieldHitFX();
        }
        else
        {
            // SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(DamageFX));
            player_handler.PlayerHitFX();
        }
    }

    public P_Shooter GetLeftShooter()
    {
        if (left_pickup_index >= 0 && left_pickup_index < player_handler.gun_manager.shooters.Length)
        {
            return player_handler.gun_manager.shooters[left_pickup_index];
        }
        return null;
    }
    public P_Shooter GetRightShooter()
    {
        if (right_pickup_index >= 0 && right_pickup_index < player_handler.gun_manager.shooters.Length)
        {
            return player_handler.gun_manager.shooters[right_pickup_index];
        }
        return null;
    }

    public int CalcDamage(bool left_hand)
    {
        int effective_damage = 0;
        P_Shooter shooter = null;
        if (left_hand && left_pickup_index >= 0 && left_pickup_index < player_handler.gun_manager.shooters.Length)
        {
            shooter = player_handler.gun_manager.shooters[left_pickup_index];
        }
        else if (!left_hand && right_pickup_index >= 0 && right_pickup_index < player_handler.gun_manager.shooters.Length)
        {
            shooter = player_handler.gun_manager.shooters[right_pickup_index];
        }
        if (shooter != null)
        {
            effective_damage = shooter.base_damage;
        }
        effective_damage = Mathf.RoundToInt(effective_damage * damage);
        return effective_damage;
    }

    public void ShieldFX()
    {
        animator.SetTrigger("shield");
    }
    public void ShieldBreakFX()
    {
        animator.SetTrigger("shield_break");
    }

    public void DamageFX()
    {
        animator.SetTrigger("damage");
    }
    public void HealFX()
    {
        animator.SetTrigger("upgrade");
    }
    public void DeathFX()
    {
        death_particles.transform.position = death_spot;
        death_particles.Play();
        last_death = Time.timeSinceLevelLoad;
    }

    public void GotKill()
    {
        if (player_handler != null)
        {
            player_handler.KillFX();
        }
        kills++;
    }

    public void ResetKills()
    {
        kills = 0;
    }

    public void ShotBy(int other_id, bool left_hand)
    {
        if (last_death + 3f > Time.timeSinceLevelLoad || last_safe + 1f > Time.timeSinceLevelLoad)//invincible for 3 seconds
        {
            return;
        }
        if (player_handler != null && other_id >= 0 && other_id < player_handler.players.Length)
        {
            int damage = 0;
            Player attacker = player_handler.players[other_id];
            if (attacker != null && attacker.Owner != null && attacker.Owner.IsValid())
            {
                damage = attacker.CalcDamage(left_hand);
                if (damage > 0)
                {
                    bool died = player_handler.LowerHealth(damage, false);
                    if (died)
                    {
                        attacker.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(GotKill));
                    }
                }
            }
            
        }
    }
    
    public void InstakillBy(int other_id){
        if (last_death + 3f > Time.timeSinceLevelLoad || last_safe + 1f > Time.timeSinceLevelLoad)//invincible for 3 seconds
        {
            return;
        }
        if (player_handler != null && other_id >= 0 && other_id < player_handler.players.Length)
        {
            Player attacker = player_handler.players[other_id];
            if (attacker != null && attacker.Owner != null && attacker.Owner.IsValid())
            {
                player_handler.LowerHealth(999999, true);
                attacker.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(GotKill));
            }

        }
    }

    public void InstakillBy0()
    {
        InstakillBy(0);
    }
    public void InstakillBy1()
    {
        InstakillBy(1);
    }
    public void InstakillBy2()
    {
        InstakillBy(2);
    }
    public void InstakillBy3()
    {
        InstakillBy(3);
    }
    public void InstakillBy4()
    {
        InstakillBy(4);
    }
    public void InstakillBy5()
    {
        InstakillBy(5);
    }
    public void InstakillBy6()
    {
        InstakillBy(6);
    }
    public void InstakillBy7()
    {
        InstakillBy(7);
    }
    public void InstakillBy8()
    {
        InstakillBy(8);
    }
    public void InstakillBy9()
    {
        InstakillBy(9);
    }
    public void InstakillBy10()
    {
        InstakillBy(10);
    }
    public void InstakillBy11()
    {
        InstakillBy(11);
    }
    public void InstakillBy12()
    {
        InstakillBy(12);
    }
    public void InstakillBy13()
    {
        InstakillBy(13);
    }
    public void InstakillBy14()
    {
        InstakillBy(14);
    }
    public void InstakillBy15()
    {
        InstakillBy(15);
    }
    public void InstakillBy16()
    {
        InstakillBy(16);
    }
    public void InstakillBy17()
    {
        InstakillBy(17);
    }
    public void InstakillBy18()
    {
        InstakillBy(18);
    }
    public void InstakillBy19()
    {
        InstakillBy(19);
    }
    public void InstakillBy20()
    {
        InstakillBy(20);
    }
    public void InstakillBy21()
    {
        InstakillBy(21);
    }
    public void InstakillBy22()
    {
        InstakillBy(22);
    }
    public void InstakillBy23()
    {
        InstakillBy(23);
    }
    public void InstakillBy24()
    {
        InstakillBy(24);
    }
    public void InstakillBy25()
    {
        InstakillBy(25);
    }
    public void InstakillBy26()
    {
        InstakillBy(26);
    }
    public void InstakillBy27()
    {
        InstakillBy(27);
    }
    public void InstakillBy28()
    {
        InstakillBy(28);
    }
    public void InstakillBy29()
    {
        InstakillBy(29);
    }
    public void InstakillBy30()
    {
        InstakillBy(30);
    }
    public void InstakillBy31()
    {
        InstakillBy(31);
    }
    public void InstakillBy32()
    {
        InstakillBy(32);
    }
    public void InstakillBy33()
    {
        InstakillBy(33);
    }
    public void InstakillBy34()
    {
        InstakillBy(34);
    }
    public void InstakillBy35()
    {
        InstakillBy(35);
    }
    public void InstakillBy36()
    {
        InstakillBy(36);
    }
    public void InstakillBy37()
    {
        InstakillBy(37);
    }
    public void InstakillBy38()
    {
        InstakillBy(38);
    }
    public void InstakillBy39()
    {
        InstakillBy(39);
    }
    public void InstakillBy40()
    {
        InstakillBy(40);
    }
    public void InstakillBy41()
    {
        InstakillBy(41);
    }
    public void InstakillBy42()
    {
        InstakillBy(42);
    }
    public void InstakillBy43()
    {
        InstakillBy(43);
    }
    public void InstakillBy44()
    {
        InstakillBy(44);
    }
    public void InstakillBy45()
    {
        InstakillBy(45);
    }
    public void InstakillBy46()
    {
        InstakillBy(46);
    }
    public void InstakillBy47()
    {
        InstakillBy(47);
    }
    public void InstakillBy48()
    {
        InstakillBy(48);
    }
    public void InstakillBy49()
    {
        InstakillBy(49);
    }
    public void InstakillBy50()
    {
        InstakillBy(50);
    }
    public void InstakillBy51()
    {
        InstakillBy(51);
    }
    public void InstakillBy52()
    {
        InstakillBy(52);
    }
    public void InstakillBy53()
    {
        InstakillBy(53);
    }
    public void InstakillBy54()
    {
        InstakillBy(54);
    }
    public void InstakillBy55()
    {
        InstakillBy(55);
    }
    public void InstakillBy56()
    {
        InstakillBy(56);
    }
    public void InstakillBy57()
    {
        InstakillBy(57);
    }
    public void InstakillBy58()
    {
        InstakillBy(58);
    }
    public void InstakillBy59()
    {
        InstakillBy(59);
    }
    public void InstakillBy60()
    {
        InstakillBy(60);
    }
    public void InstakillBy61()
    {
        InstakillBy(61);
    }
    public void InstakillBy62()
    {
        InstakillBy(62);
    }
    public void InstakillBy63()
    {
        InstakillBy(63);
    }
    public void InstakillBy64()
    {
        InstakillBy(64);
    }
    public void InstakillBy65()
    {
        InstakillBy(65);
    }
    public void InstakillBy66()
    {
        InstakillBy(66);
    }
    public void InstakillBy67()
    {
        InstakillBy(67);
    }
    public void InstakillBy68()
    {
        InstakillBy(68);
    }
    public void InstakillBy69()
    {
        InstakillBy(69);
    }
    public void InstakillBy70()
    {
        InstakillBy(70);
    }
    public void InstakillBy71()
    {
        InstakillBy(71);
    }
    public void InstakillBy72()
    {
        InstakillBy(72);
    }
    public void InstakillBy73()
    {
        InstakillBy(73);
    }
    public void InstakillBy74()
    {
        InstakillBy(74);
    }
    public void InstakillBy75()
    {
        InstakillBy(75);
    }
    public void InstakillBy76()
    {
        InstakillBy(76);
    }
    public void InstakillBy77()
    {
        InstakillBy(77);
    }
    public void InstakillBy78()
    {
        InstakillBy(78);
    }
    public void InstakillBy79()
    {
        InstakillBy(79);
    }
    public void InstakillBy80()
    {
        InstakillBy(80);
    }
    public void InstakillBy81()
    {
        InstakillBy(81);
    }
    public void InstakillBy82()
    {
        InstakillBy(82);
    }
    public void ShotByLeft0()
    {
        ShotBy(0, true);
    }
    public void ShotByLeft1()
    {
        ShotBy(1, true);
    }
    public void ShotByLeft2()
    {
        ShotBy(2, true);
    }
    public void ShotByLeft3()
    {
        ShotBy(3, true);
    }
    public void ShotByLeft4()
    {
        ShotBy(4, true);
    }
    public void ShotByLeft5()
    {
        ShotBy(5, true);
    }
    public void ShotByLeft6()
    {
        ShotBy(6, true);
    }
    public void ShotByLeft7()
    {
        ShotBy(7, true);
    }
    public void ShotByLeft8()
    {
        ShotBy(8, true);
    }
    public void ShotByLeft9()
    {
        ShotBy(9, true);
    }
    public void ShotByLeft10()
    {
        ShotBy(10, true);
    }
    public void ShotByLeft11()
    {
        ShotBy(11, true);
    }
    public void ShotByLeft12()
    {
        ShotBy(12, true);
    }
    public void ShotByLeft13()
    {
        ShotBy(13, true);
    }
    public void ShotByLeft14()
    {
        ShotBy(14, true);
    }
    public void ShotByLeft15()
    {
        ShotBy(15, true);
    }
    public void ShotByLeft16()
    {
        ShotBy(16, true);
    }
    public void ShotByLeft17()
    {
        ShotBy(17, true);
    }
    public void ShotByLeft18()
    {
        ShotBy(18, true);
    }
    public void ShotByLeft19()
    {
        ShotBy(19, true);
    }
    public void ShotByLeft20()
    {
        ShotBy(20, true);
    }
    public void ShotByLeft21()
    {
        ShotBy(21, true);
    }
    public void ShotByLeft22()
    {
        ShotBy(22, true);
    }
    public void ShotByLeft23()
    {
        ShotBy(23, true);
    }
    public void ShotByLeft24()
    {
        ShotBy(24, true);
    }
    public void ShotByLeft25()
    {
        ShotBy(25, true);
    }
    public void ShotByLeft26()
    {
        ShotBy(26, true);
    }
    public void ShotByLeft27()
    {
        ShotBy(27, true);
    }
    public void ShotByLeft28()
    {
        ShotBy(28, true);
    }
    public void ShotByLeft29()
    {
        ShotBy(29, true);
    }
    public void ShotByLeft30()
    {
        ShotBy(30, true);
    }
    public void ShotByLeft31()
    {
        ShotBy(31, true);
    }
    public void ShotByLeft32()
    {
        ShotBy(32, true);
    }
    public void ShotByLeft33()
    {
        ShotBy(33, true);
    }
    public void ShotByLeft34()
    {
        ShotBy(34, true);
    }
    public void ShotByLeft35()
    {
        ShotBy(35, true);
    }
    public void ShotByLeft36()
    {
        ShotBy(36, true);
    }
    public void ShotByLeft37()
    {
        ShotBy(37, true);
    }
    public void ShotByLeft38()
    {
        ShotBy(38, true);
    }
    public void ShotByLeft39()
    {
        ShotBy(39, true);
    }
    public void ShotByLeft40()
    {
        ShotBy(40, true);
    }
    public void ShotByLeft41()
    {
        ShotBy(41, true);
    }
    public void ShotByLeft42()
    {
        ShotBy(42, true);
    }
    public void ShotByLeft43()
    {
        ShotBy(43, true);
    }
    public void ShotByLeft44()
    {
        ShotBy(44, true);
    }
    public void ShotByLeft45()
    {
        ShotBy(45, true);
    }
    public void ShotByLeft46()
    {
        ShotBy(46, true);
    }
    public void ShotByLeft47()
    {
        ShotBy(47, true);
    }
    public void ShotByLeft48()
    {
        ShotBy(48, true);
    }
    public void ShotByLeft49()
    {
        ShotBy(49, true);
    }
    public void ShotByLeft50()
    {
        ShotBy(50, true);
    }
    public void ShotByLeft51()
    {
        ShotBy(51, true);
    }
    public void ShotByLeft52()
    {
        ShotBy(52, true);
    }
    public void ShotByLeft53()
    {
        ShotBy(53, true);
    }
    public void ShotByLeft54()
    {
        ShotBy(54, true);
    }
    public void ShotByLeft55()
    {
        ShotBy(55, true);
    }
    public void ShotByLeft56()
    {
        ShotBy(56, true);
    }
    public void ShotByLeft57()
    {
        ShotBy(57, true);
    }
    public void ShotByLeft58()
    {
        ShotBy(58, true);
    }
    public void ShotByLeft59()
    {
        ShotBy(59, true);
    }
    public void ShotByLeft60()
    {
        ShotBy(60, true);
    }
    public void ShotByLeft61()
    {
        ShotBy(61, true);
    }
    public void ShotByLeft62()
    {
        ShotBy(62, true);
    }
    public void ShotByLeft63()
    {
        ShotBy(63, true);
    }
    public void ShotByLeft64()
    {
        ShotBy(64, true);
    }
    public void ShotByLeft65()
    {
        ShotBy(65, true);
    }
    public void ShotByLeft66()
    {
        ShotBy(66, true);
    }
    public void ShotByLeft67()
    {
        ShotBy(67, true);
    }
    public void ShotByLeft68()
    {
        ShotBy(68, true);
    }
    public void ShotByLeft69()
    {
        ShotBy(69, true);
    }
    public void ShotByLeft70()
    {
        ShotBy(70, true);
    }
    public void ShotByLeft71()
    {
        ShotBy(71, true);
    }
    public void ShotByLeft72()
    {
        ShotBy(72, true);
    }
    public void ShotByLeft73()
    {
        ShotBy(73, true);
    }
    public void ShotByLeft74()
    {
        ShotBy(74, true);
    }
    public void ShotByLeft75()
    {
        ShotBy(75, true);
    }
    public void ShotByLeft76()
    {
        ShotBy(76, true);
    }
    public void ShotByLeft77()
    {
        ShotBy(77, true);
    }
    public void ShotByLeft78()
    {
        ShotBy(78, true);
    }
    public void ShotByLeft79()
    {
        ShotBy(79, true);
    }
    public void ShotByLeft80()
    {
        ShotBy(80, true);
    }
    public void ShotByLeft81()
    {
        ShotBy(81, true);
    }
    public void ShotByLeft82()
    {
        ShotBy(82, true);
    }

    public void ShotByRight0()
    {
        ShotBy(0, false);
    }
    public void ShotByRight1()
    {
        ShotBy(1, false);
    }
    public void ShotByRight2()
    {
        ShotBy(2, false);
    }
    public void ShotByRight3()
    {
        ShotBy(3, false);
    }
    public void ShotByRight4()
    {
        ShotBy(4, false);
    }
    public void ShotByRight5()
    {
        ShotBy(5, false);
    }
    public void ShotByRight6()
    {
        ShotBy(6, false);
    }
    public void ShotByRight7()
    {
        ShotBy(7, false);
    }
    public void ShotByRight8()
    {
        ShotBy(8, false);
    }
    public void ShotByRight9()
    {
        ShotBy(9, false);
    }
    public void ShotByRight10()
    {
        ShotBy(10, false);
    }
    public void ShotByRight11()
    {
        ShotBy(11, false);
    }
    public void ShotByRight12()
    {
        ShotBy(12, false);
    }
    public void ShotByRight13()
    {
        ShotBy(13, false);
    }
    public void ShotByRight14()
    {
        ShotBy(14, false);
    }
    public void ShotByRight15()
    {
        ShotBy(15, false);
    }
    public void ShotByRight16()
    {
        ShotBy(16, false);
    }
    public void ShotByRight17()
    {
        ShotBy(17, false);
    }
    public void ShotByRight18()
    {
        ShotBy(18, false);
    }
    public void ShotByRight19()
    {
        ShotBy(19, false);
    }
    public void ShotByRight20()
    {
        ShotBy(20, false);
    }
    public void ShotByRight21()
    {
        ShotBy(21, false);
    }
    public void ShotByRight22()
    {
        ShotBy(22, false);
    }
    public void ShotByRight23()
    {
        ShotBy(23, false);
    }
    public void ShotByRight24()
    {
        ShotBy(24, false);
    }
    public void ShotByRight25()
    {
        ShotBy(25, false);
    }
    public void ShotByRight26()
    {
        ShotBy(26, false);
    }
    public void ShotByRight27()
    {
        ShotBy(27, false);
    }
    public void ShotByRight28()
    {
        ShotBy(28, false);
    }
    public void ShotByRight29()
    {
        ShotBy(29, false);
    }
    public void ShotByRight30()
    {
        ShotBy(30, false);
    }
    public void ShotByRight31()
    {
        ShotBy(31, false);
    }
    public void ShotByRight32()
    {
        ShotBy(32, false);
    }
    public void ShotByRight33()
    {
        ShotBy(33, false);
    }
    public void ShotByRight34()
    {
        ShotBy(34, false);
    }
    public void ShotByRight35()
    {
        ShotBy(35, false);
    }
    public void ShotByRight36()
    {
        ShotBy(36, false);
    }
    public void ShotByRight37()
    {
        ShotBy(37, false);
    }
    public void ShotByRight38()
    {
        ShotBy(38, false);
    }
    public void ShotByRight39()
    {
        ShotBy(39, false);
    }
    public void ShotByRight40()
    {
        ShotBy(40, false);
    }
    public void ShotByRight41()
    {
        ShotBy(41, false);
    }
    public void ShotByRight42()
    {
        ShotBy(42, false);
    }
    public void ShotByRight43()
    {
        ShotBy(43, false);
    }
    public void ShotByRight44()
    {
        ShotBy(44, false);
    }
    public void ShotByRight45()
    {
        ShotBy(45, false);
    }
    public void ShotByRight46()
    {
        ShotBy(46, false);
    }
    public void ShotByRight47()
    {
        ShotBy(47, false);
    }
    public void ShotByRight48()
    {
        ShotBy(48, false);
    }
    public void ShotByRight49()
    {
        ShotBy(49, false);
    }
    public void ShotByRight50()
    {
        ShotBy(50, false);
    }
    public void ShotByRight51()
    {
        ShotBy(51, false);
    }
    public void ShotByRight52()
    {
        ShotBy(52, false);
    }
    public void ShotByRight53()
    {
        ShotBy(53, false);
    }
    public void ShotByRight54()
    {
        ShotBy(54, false);
    }
    public void ShotByRight55()
    {
        ShotBy(55, false);
    }
    public void ShotByRight56()
    {
        ShotBy(56, false);
    }
    public void ShotByRight57()
    {
        ShotBy(57, false);
    }
    public void ShotByRight58()
    {
        ShotBy(58, false);
    }
    public void ShotByRight59()
    {
        ShotBy(59, false);
    }
    public void ShotByRight60()
    {
        ShotBy(60, false);
    }
    public void ShotByRight61()
    {
        ShotBy(61, false);
    }
    public void ShotByRight62()
    {
        ShotBy(62, false);
    }
    public void ShotByRight63()
    {
        ShotBy(63, false);
    }
    public void ShotByRight64()
    {
        ShotBy(64, false);
    }
    public void ShotByRight65()
    {
        ShotBy(65, false);
    }
    public void ShotByRight66()
    {
        ShotBy(66, false);
    }
    public void ShotByRight67()
    {
        ShotBy(67, false);
    }
    public void ShotByRight68()
    {
        ShotBy(68, false);
    }
    public void ShotByRight69()
    {
        ShotBy(69, false);
    }
    public void ShotByRight70()
    {
        ShotBy(70, false);
    }
    public void ShotByRight71()
    {
        ShotBy(71, false);
    }
    public void ShotByRight72()
    {
        ShotBy(72, false);
    }
    public void ShotByRight73()
    {
        ShotBy(73, false);
    }
    public void ShotByRight74()
    {
        ShotBy(74, false);
    }
    public void ShotByRight75()
    {
        ShotBy(75, false);
    }
    public void ShotByRight76()
    {
        ShotBy(76, false);
    }
    public void ShotByRight77()
    {
        ShotBy(77, false);
    }
    public void ShotByRight78()
    {
        ShotBy(78, false);
    }
    public void ShotByRight79()
    {
        ShotBy(79, false);
    }
    public void ShotByRight80()
    {
        ShotBy(80, false);
    }
    public void ShotByRight81()
    {
        ShotBy(81, false);
    }
    public void ShotByRight82()
    {
        ShotBy(82, false);
    }
}
