
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DamageZone : UdonSharpBehaviour
{
    public PlayerHandler player_handler;
    private float last_damage = -1001;
    public float damage_interval = 1.0f;
    public int damage_amount = 10;
    public bool damage_ignores_shield = false;
    void Start()
    {
        if (player_handler == null)
        {
            player_handler = GameObject.Find("__GUN MANAGER__").GetComponentInChildren<PlayerHandler>();
        }
    }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    public void Reset()
    {
        if (player_handler == null)
        {
            player_handler = GameObject.FindObjectOfType<PlayerHandler>();
        }
    }
#endif

    public void OnTriggerStay(Collider other)
    {
        if (player_handler == null || (player_handler.scores != null && (player_handler._localPlayer.team == 0 || !player_handler.scores.game_active)) || last_damage + damage_interval > Time.timeSinceLevelLoad || !Utilities.IsValid(other) || other == null)
        {
            return;
        }
        if (player_handler._localPlayer == other.GetComponent<Player>())
        {
            last_damage = Time.timeSinceLevelLoad;
            player_handler.LowerHealth(damage_amount, damage_ignores_shield);
        }
    }
}
