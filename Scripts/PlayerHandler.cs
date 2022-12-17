
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
    public int starting_health = 100;
    public int starting_shield = 500;
    public float run_speed = 6.0f;
    public float strafe_speed = 4.0f;
    public float walk_speed = 4.0f;
    public float jump_impulse = 6.0f;
    // public float starting_speed = 10f;
    // public float starting_jump = 10f;

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
    public int shield_regen_rate = 10;//per second
    public float hud_hide_delay = 3;//per second
    public Scoreboard scores;
    public void _OnLocalPlayerAssigned()
    {
        // Get the local player's pool object so we can later perform operations on it.
        _localPlayer = (Player)objectPoolAssigner._GetPlayerPooledUdon(Networking.LocalPlayer);
        if (animator != null && _localPlayer != null)
        {
            Networking.SetOwner(Networking.LocalPlayer, _localPlayer.gameObject);
            animator.SetTrigger("loaded");
        }
        _localPlayer.Reset();
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
    }

    public void _Register(GunManager guns)
    {
        gun_manager = guns;
    }

    public void ResetKills()
    {
        if (_localPlayer != null)
        {
            _localPlayer.ResetKills();
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
            } else
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
            if(affects_health && _localPlayer.health < starting_health){
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
            } else if (affects_shield)
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
            _localPlayer.Reset();
            if (scores != null)
            {
                scores.OnPlayerDie();
            } else
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
        } else
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
            if (animator != null)
            {
                VRCPlayerApi.TrackingData headData = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
                animator.transform.position = headData.position;
                animator.transform.rotation = Quaternion.Slerp(headData.rotation, Networking.LocalPlayer.GetRotation(), 0.25f);
                animator.SetFloat("Shield", (((float)_localPlayer.shield)/(float)starting_shield));
                animator.SetFloat("Health", (((float)_localPlayer.health)/(float)starting_health));
                animator.SetBool("Hide", last_damage + shield_regen_delay + hud_hide_delay < Time.timeSinceLevelLoad && last_heal + hud_hide_delay < Time.timeSinceLevelLoad);
                // animator.SetBool("Hide", last_damage + shield_regen_delay + hud_hide_delay < Time.timeSinceLevelLoad && last_heal + hud_hide_delay < Time.timeSinceLevelLoad && _localPlayer.left_pickup_index < 0 && _localPlayer.right_pickup_index < 0);
                if (shield_regen_delay > 0 && last_damage + shield_regen_delay < Time.timeSinceLevelLoad)
                {
                    if (Mathf.RoundToInt(Time.timeSinceLevelLoad - Time.deltaTime) < Mathf.RoundToInt(Time.timeSinceLevelLoad) && _localPlayer.shield < starting_shield)
                    {
                        _localPlayer.shield = Mathf.Min(_localPlayer.shield + shield_regen_rate, starting_shield);
                        _localPlayer.RequestSerialization();
                        last_heal = Time.timeSinceLevelLoad;
                    }
                }
            }
        }
    }
}
