
using JetBrains.Annotations;
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
    [CustomEditor(typeof(PlayerHandler))]
    public class PlayerHandlerEditor : Editor
    {
        public static void SetupPlayers()
        {
            GunManager manager = null;
            foreach (GunManager go in GameObject.FindObjectsOfType(typeof(GunManager)) as GunManager[])
            {
                if (go != null)
                {
                    manager = go;
                    break;
                }
            }
            if (manager == null)
            {
                Debug.LogError($"[<color=#FF00FF>UdonSharp</color>] Could not find a Gun Manager. Did you drag the prefab into your scene?");
                return;
            }


            PlayerHandler handler = null;
            foreach (PlayerHandler go in GameObject.FindObjectsOfType(typeof(PlayerHandler)) as PlayerHandler[])
            {
                if (go != null)
                {
                    handler = go;
                    break;
                }
            }
            if (handler == null)
            {
                Debug.LogError($"[<color=#FF00FF>UdonSharp</color>] Could not find a Player Handler. Did you drag the prefab into your scene?");
                return;
            }

            SerializedObject serializedManager = new SerializedObject(manager);
            serializedManager.FindProperty("player_handler").objectReferenceValue = handler;
            serializedManager.ApplyModifiedProperties();
            Debug.Log($"Players were set up");
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("PLAYER SETUP");
            EditorGUILayout.HelpBox(
    @"1) Drag a 'players' prefab into your scene. There should only be one. Make sure it's players as in the plural version

2) Press the setup button below or the setup button on the players object you just dragged in

3) Call me if it broke. lol", MessageType.Info);
            if (GUILayout.Button(new GUIContent("Set up ALL Players")))
            {
                PlayerHandlerEditor.SetupPlayers();
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

    [RequireComponent(typeof(Animator))]
    public class PlayerHandler : UdonSharpBehaviour
    {
        public Cyan.PlayerObjectPool.CyanPlayerObjectAssigner objectPoolAssigner;
        [System.NonSerialized] public Player _localPlayer;
        public Player[] players;
        // This event is called when the local player's pool object has been assigned.
        [PublicAPI]
        [FieldChangeCallbackAttribute(nameof(starting_health))] public int _starting_health = 100;
        [FieldChangeCallbackAttribute(nameof(starting_shield))] public int _starting_shield = 500;
        [FieldChangeCallbackAttribute(nameof(run_speed))] public float _run_speed = 6.0f;
        [FieldChangeCallbackAttribute(nameof(strafe_speed))] public float _strafe_speed = 4.0f;
        [FieldChangeCallbackAttribute(nameof(walk_speed))] public float _walk_speed = 4.0f;
        [FieldChangeCallbackAttribute(nameof(jump_impulse))] public float _jump_impulse = 6.0f;

        [FieldChangeCallback(nameof(health_multiplier))] private float _health_multiplier = 1.0f;
        [FieldChangeCallback(nameof(shield_multiplier))] private float _shield_multiplier = 1.0f;
        private float shield_regen_amount_multiplier = 1.0f;
        private float speed_multiplier = 1.0f;
        private float jump_multiplier = 1.0f;
        public float health_multiplier
        {
            get
            {
                return _health_multiplier;
            }
            set
            {
                int multiplied_value = value <= 0 ? 0 : Mathf.CeilToInt(value * _starting_health); //with the underscore
                if (multiplied_value != starting_health)//without the underscore
                {
                    if (_localPlayer != null)
                    {
                        _localPlayer.health = Mathf.Max(1, Mathf.Min(multiplied_value, Mathf.CeilToInt((_localPlayer.health * multiplied_value) / Mathf.Max(1, starting_health))));
                        //Max of 1 to prevent from instakilling
                    }
                }
                _health_multiplier = value;
            }
        }
        public float shield_multiplier
        {
            get
            {
                return _shield_multiplier;
            }
            set
            {
                int multiplied_value = value <= 0 ? 0 : Mathf.CeilToInt(value * _starting_shield); //with the underscore
                if (multiplied_value != starting_shield)//without the underscore
                {
                    if (_localPlayer != null)
                    {
                        _localPlayer.shield = Mathf.Min(multiplied_value, Mathf.CeilToInt((_localPlayer.shield * multiplied_value) / Mathf.Max(1, starting_shield)));
                    }
                }
                _shield_multiplier = value;
            }
        }

        public int starting_health
        {
            get
            {
                return health_multiplier <= 0 ? 0 : Mathf.CeilToInt(_starting_health * health_multiplier);
            }
            set
            {
                _starting_health = value;
            }
        }
        public int starting_shield
        {
            get
            {
                return shield_multiplier <= 0 ? 0 : Mathf.CeilToInt(_starting_shield * shield_multiplier);
            }
            set
            {
                _starting_shield = value;
            }
        }
        public float run_speed
        {
            get
            {
                return _run_speed * speed_multiplier;
            }
            set
            {
                _run_speed = value;
            }
        }
        public float strafe_speed
        {
            get
            {
                return _strafe_speed * speed_multiplier;
            }
            set
            {
                _strafe_speed = value;
            }
        }
        public float walk_speed
        {
            get
            {
                return _walk_speed * speed_multiplier;
            }
            set
            {
                _walk_speed = value;
            }
        }
        public float jump_impulse
        {
            get
            {
                return _jump_impulse * jump_multiplier;
            }
            set
            {
                _jump_impulse = value;
            }
        }

        public LayerMask safety_layers;
        public LayerMask damage_layers;

        public AudioSource global_sound_fx;
        public AudioSource global_sound_fx_2;
        public AudioClip hit_other_player_sound;
        public AudioClip hit_other_player_shield_sound;
        public AudioClip break_other_player_shield_sound;
        public AudioClip take_damage_sound;
        public AudioClip take_damage_shield_sound;
        public AudioClip lose_shield_sound;
        public AudioClip kill_other_player_sound;
        public AudioClip die_sound;
        public AudioClip heal_sound;
        public Animator animator;
        [System.NonSerialized] public GunManager gun_manager;

        private float last_damage = -99;
        private float last_heal = -99;
        public float shield_regen_delay = 5;
        public int _shield_regen_amount = 10;//per second
        public float hud_hide_delay = 3;//per second


        public int shield_regen_amount
        {
            get
            {
                return Mathf.CeilToInt(_shield_regen_amount * shield_regen_amount_multiplier);
            }
            set
            {
                _shield_regen_amount = value;
            }
        }

        public UnityEngine.UI.Toggle world_owner_toggle;
        [UdonSyncedAttribute(UdonSyncMode.None), FieldChangeCallback(nameof(world_owner_only))] public bool _world_owner_only = true;
        public bool world_owner_only
        {
            get => _world_owner_only;
            set
            {
                _world_owner_only = value;
                RequestSerialization();
                if (world_owner_toggle != null)
                {
                    world_owner_toggle.isOn = _world_owner_only;
                }
            }
        }
        public Scoreboard scores
        {
            get
            {
                if (scoreboards == null || selected_scoreboard < 0 || selected_scoreboard >= scoreboards.Length)
                {
                    return null;
                }
                return scoreboards[selected_scoreboard];
            }
            set
            {
                if (value == null)
                {
                    selected_scoreboard = -1;
                    return;
                }
                for (int i = 0; i < scoreboards.Length; i++)
                {
                    if (scoreboards[i] == value)
                    {
                        selected_scoreboard = i;
                        break;
                    }
                }
            }
        }
        [UdonSyncedAttribute(UdonSyncMode.None), FieldChangeCallback(nameof(teams))] public bool _teams = false;
        public bool teams
        {
            get => _teams;
            set
            {
                _teams = value;
                RequestSerialization();
                if (scores != null)
                {
                    scores.UpdateScores();
                }
            }
        }

        [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(selected_scoreboard))] public int _selected_scoreboard = -1;
        public int selected_scoreboard
        {
            get => _selected_scoreboard;
            set
            {
                _selected_scoreboard = value;

                for (int i = 0; i < scoreboards.Length; i++)
                {
                    if (scoreboards[i] != null)
                    {
                        scoreboards[i].gameObject.SetActive(i == value);
                        if (i == value)
                        {
                            if (scoreboards[i].forceTeams)
                            {
                                teams = true;
                            }
                            else if (scoreboards[i].forceNoTeams)
                            {
                                teams = false;
                            }
                        }
                    }
                }
                RequestSerialization();
            }
        }
        public Scoreboard[] scoreboards;
        public void _OnLocalPlayerAssigned()
        {
            // Get the local player's pool object so we can later perform operations on it.
            _localPlayer = (Player)objectPoolAssigner._GetPlayerPooledUdon(Networking.LocalPlayer);
            if (animator != null && _localPlayer != null)
            {
                Networking.SetOwner(Networking.LocalPlayer, _localPlayer.gameObject);
                animator.SetTrigger("loaded");
            }
            _localPlayer._Reset();
            _localPlayer.RequestSerialization();
        }
        void Start()
        {
            if (animator != null && _localPlayer != null)
            {
                animator.SetTrigger("loaded");
            }
            for (int i = 0; i < players.Length; i++)
            {
                players[i]._Register(this, i);
            }
            Networking.LocalPlayer.SetRunSpeed(run_speed);
            Networking.LocalPlayer.SetStrafeSpeed(strafe_speed);
            Networking.LocalPlayer.SetWalkSpeed(walk_speed);
            Networking.LocalPlayer.SetJumpImpulse(jump_impulse);
            selected_scoreboard = selected_scoreboard;
            world_owner_only = world_owner_only;
        }

        public void _Register(GunManager guns)
        {
            gun_manager = guns;
        }

        public void ResetScore()
        {
            if (_localPlayer != null)
            {
                _localPlayer.ResetScore();
            }
        }



        public void RequestToggleWorldOwner()
        {
            if (Networking.LocalPlayer.IsOwner(gameObject))
            {
                world_owner_only = !world_owner_only;
            }
            else
            {
                world_owner_only = world_owner_only;
            }
        }

        public bool LowerHealth(int amount, bool ignoreShield)
        {
            bool died = false;
            if (_localPlayer != null)
            {
                if (_localPlayer.shield > 0 && !ignoreShield)
                {
                    TakeDamageShieldFX();
                    _localPlayer.shield -= amount;
                    if (_localPlayer.shield <= 0)
                    {
                        _localPlayer.health += _localPlayer.shield;
                        _localPlayer.shield = 0;
                        LoseShieldFX();
                    }
                }
                else
                {
                    TakeDamageFX();
                    _localPlayer.health -= amount;
                }
                if (_localPlayer.health < 0)
                {
                    DieFX();
                    died = true;
                    Respawn();
                }
            }
            return died;
        }

        public bool IncreaseHealth(int amount, bool affects_health, bool affects_shield)
        {
            bool healed = false;
            if (_localPlayer != null)
            {
                if (affects_health && _localPlayer.health < starting_health)
                {
                    _localPlayer.health += amount;
                    HealFX();
                    last_heal = Time.timeSinceLevelLoad;
                    healed = true;
                    if (_localPlayer.health >= starting_health)
                    {
                        if (affects_shield)
                        {
                            _localPlayer.shield += _localPlayer.health - starting_health;
                            if (_localPlayer.shield >= starting_shield)
                            {
                                _localPlayer.shield = starting_shield;
                            }
                        }
                        _localPlayer.health = starting_health;
                    }
                }
                else if (affects_shield)
                {
                    _localPlayer.shield += amount;
                    HealFX();
                    last_heal = Time.timeSinceLevelLoad;
                    healed = true;
                    if (_localPlayer.shield >= starting_shield)
                    {
                        _localPlayer.shield = starting_shield;
                    }
                }
            }
            return healed;
        }

        public void Respawn()
        {
            if (_localPlayer != null)
            {
                _localPlayer.death_spot = Networking.LocalPlayer.GetPosition();
                _localPlayer._Reset();
                if (scores != null)
                {
                    scores.OnPlayerDie();
                }
                else
                {
                    Networking.LocalPlayer.Respawn();
                }
            }
        }

        public override void OnPlayerRespawn(VRCPlayerApi player)
        {
            if (scores != null && player != null && player.isLocal)
            {
                scores.OnPlayerRespawned();
            }
        }

        public void ShieldBreakFX()
        {
            global_sound_fx.clip = break_other_player_shield_sound;
            global_sound_fx.Play();
        }
        public void ShieldHitFX()
        {
            if (!global_sound_fx.isPlaying)
            {
                global_sound_fx.clip = hit_other_player_shield_sound;
                global_sound_fx.Play();
            }
            else
            {
                global_sound_fx_2.clip = hit_other_player_shield_sound;
                global_sound_fx_2.Play();
            }
            // AudioSource.PlayClipAtPoint(hit_other_player_shield_sound, transform.position);
        }
        public void PlayerHitFX()
        {
            if (!global_sound_fx.isPlaying)
            {
                global_sound_fx.clip = hit_other_player_sound;
                global_sound_fx.Play();
            }
            else
            {
                global_sound_fx_2.clip = hit_other_player_sound;
                global_sound_fx_2.Play();
            }
            // AudioSource.PlayClipAtPoint(hit_other_player_sound, transform.position);
        }
        public void KillFX()
        {
            if (_localPlayer != null && _localPlayer.last_death + 2f < Time.timeSinceLevelLoad)
            {
                global_sound_fx.clip = kill_other_player_sound;
                global_sound_fx.Play();
            }
        }
        public void HealFX()
        {
            if (_localPlayer != null && _localPlayer.last_death + 2f < Time.timeSinceLevelLoad)
            {
                global_sound_fx.clip = heal_sound;
                global_sound_fx.Play();
            }
        }

        public void LoseShieldFX()
        {
            if (_localPlayer != null && _localPlayer.last_death + 2f < Time.timeSinceLevelLoad)
            {
                global_sound_fx.clip = lose_shield_sound;
                global_sound_fx.Play();
            }
        }

        public void TakeDamageFX()
        {
            if (_localPlayer != null && _localPlayer.last_death + 2f < Time.timeSinceLevelLoad)
            {
                global_sound_fx.clip = take_damage_sound;
                global_sound_fx.Play();
            }
            if (animator != null)
            {
                animator.SetTrigger("Damage");
            }
            last_damage = Time.timeSinceLevelLoad;
        }

        public void TakeDamageShieldFX()
        {
            if (_localPlayer != null && _localPlayer.last_death + 2f < Time.timeSinceLevelLoad)
            {
                global_sound_fx.clip = take_damage_shield_sound;
                global_sound_fx.Play();
            }
            if (animator != null)
            {
                animator.SetTrigger("ShieldDamage");
            }
            last_damage = Time.timeSinceLevelLoad;
        }

        public void DieFX()
        {
            global_sound_fx.clip = die_sound;
            global_sound_fx.Play();
            last_damage = -99;
            last_heal = Time.timeSinceLevelLoad;
        }

        public void Update()
        {
            if (_localPlayer != null)
            {
                P_Shooter leftShooter = _localPlayer.GetLeftShooter();
                P_Shooter rightShooter = _localPlayer.GetRightShooter();
                float new_health_multiplier = 1f;
                float new_shield_multiplier = 1f;
                float new_shield_regen_speed_multiplier = 1f;
                float new_speed_multiplier = 1f;
                float new_jump_multiplier = 1f;
                float melee_speed_boost_multiplier = 1.0f;
                if (leftShooter != null && leftShooter.gunUpgrades != null)
                {
                    new_health_multiplier = leftShooter.gunUpgrades.health_multiplier;
                    new_shield_multiplier = leftShooter.gunUpgrades.shield_multiplier;
                    new_shield_regen_speed_multiplier = leftShooter.gunUpgrades.shield_regen_amount_multiplier;
                    new_speed_multiplier = leftShooter.gunUpgrades.speed_multiplier;
                    new_jump_multiplier = leftShooter.gunUpgrades.jump_multiplier;
                }

                if (rightShooter != null && rightShooter.gunUpgrades != null)
                {
                    new_health_multiplier = new_health_multiplier < 1.0f || rightShooter.gunUpgrades.health_multiplier < 1.0f ? Mathf.Min(new_health_multiplier, rightShooter.gunUpgrades.health_multiplier) : Mathf.Max(new_health_multiplier, rightShooter.gunUpgrades.health_multiplier);
                    new_shield_multiplier = new_shield_multiplier < 1.0f || rightShooter.gunUpgrades.shield_multiplier < 1.0f ? Mathf.Min(new_shield_multiplier, rightShooter.gunUpgrades.shield_multiplier) : Mathf.Max(new_shield_multiplier, rightShooter.gunUpgrades.shield_multiplier);
                    new_shield_regen_speed_multiplier = new_shield_regen_speed_multiplier < 1.0f || rightShooter.gunUpgrades.shield_regen_amount_multiplier < 1.0f ? Mathf.Min(new_shield_regen_speed_multiplier, rightShooter.gunUpgrades.shield_regen_amount_multiplier) : Mathf.Max(new_shield_regen_speed_multiplier, rightShooter.gunUpgrades.shield_regen_amount_multiplier);
                    new_speed_multiplier = new_speed_multiplier < 1.0f || rightShooter.gunUpgrades.speed_multiplier < 1.0f ? Mathf.Min(new_speed_multiplier, rightShooter.gunUpgrades.speed_multiplier) : Mathf.Max(new_speed_multiplier, rightShooter.gunUpgrades.speed_multiplier);
                    new_jump_multiplier = new_jump_multiplier < 1.0f || rightShooter.gunUpgrades.jump_multiplier < 1.0f ? Mathf.Min(new_jump_multiplier, rightShooter.gunUpgrades.jump_multiplier) : Mathf.Max(new_jump_multiplier, rightShooter.gunUpgrades.jump_multiplier);
                }

                melee_speed_boost_multiplier = leftShooter != null && leftShooter.melee && leftShooter.shoot_state == P_Shooter.SHOOT_STATE_SHOOT ? leftShooter.melee_speed_boost_multiplier : melee_speed_boost_multiplier;
                if (rightShooter != null && rightShooter.melee && rightShooter.shoot_state == P_Shooter.SHOOT_STATE_SHOOT)
                {
                    melee_speed_boost_multiplier = melee_speed_boost_multiplier <= 0 ? Mathf.Min(melee_speed_boost_multiplier, rightShooter.melee_speed_boost_multiplier) : Mathf.Max(melee_speed_boost_multiplier, rightShooter.melee_speed_boost_multiplier);
                }

                health_multiplier = new_health_multiplier;
                shield_multiplier = new_shield_multiplier;
                shield_regen_amount_multiplier = new_shield_regen_speed_multiplier;
                speed_multiplier = new_speed_multiplier;
                jump_multiplier = new_jump_multiplier;

                Networking.LocalPlayer.SetRunSpeed(run_speed * melee_speed_boost_multiplier);
                Networking.LocalPlayer.SetStrafeSpeed(strafe_speed * melee_speed_boost_multiplier);
                Networking.LocalPlayer.SetWalkSpeed(walk_speed * melee_speed_boost_multiplier);
                Networking.LocalPlayer.SetJumpImpulse(jump_impulse);

                if (animator != null)
                {
                    VRCPlayerApi.TrackingData headData = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
                    animator.transform.position = headData.position;
                    animator.transform.rotation = Quaternion.Slerp(headData.rotation, Networking.LocalPlayer.GetRotation(), 0.25f);
                    animator.SetFloat("Shield", (((float)_localPlayer.shield) / (float)starting_shield));
                    animator.SetFloat("Health", (((float)_localPlayer.health) / (float)starting_health));
                    animator.SetBool("Hide", last_damage + shield_regen_delay + hud_hide_delay < Time.timeSinceLevelLoad && last_heal + hud_hide_delay < Time.timeSinceLevelLoad);
                    // animator.SetBool("Hide", last_damage + shield_regen_delay + hud_hide_delay < Time.timeSinceLevelLoad && last_heal + hud_hide_delay < Time.timeSinceLevelLoad && _localPlayer.left_pickup_index < 0 && _localPlayer.right_pickup_index < 0);
                    if (shield_regen_delay > 0 && last_damage + shield_regen_delay < Time.timeSinceLevelLoad)
                    {
                        if (Mathf.RoundToInt(Time.timeSinceLevelLoad - Time.deltaTime) < Mathf.RoundToInt(Time.timeSinceLevelLoad) && _localPlayer.shield < starting_shield)
                        {
                            _localPlayer.shield = Mathf.Min(_localPlayer.shield + shield_regen_amount, starting_shield);
                            _localPlayer.RequestSerialization();
                            last_heal = Time.timeSinceLevelLoad;
                        }
                    }
                }
            }
        }
    }
}