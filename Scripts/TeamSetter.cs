
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TeamSetter : UdonSharpBehaviour
{
    public Scoreboard scoreboard;
    public int team;
    public int random_min;
    public int random_max;

    void Start()
    {
        
    }


#if !COMPILER_UDONSHARP && UNITY_EDITOR
    public void Reset()
    {
        if (scoreboard == null)
        {
            scoreboard = transform.GetComponentInParent<Scoreboard>();
        }
    }
#endif

    public void SetTeam()
    {
        scoreboard.player_handler._localPlayer.team = team;
    }
    public void SetRandomTeam()
    {
        scoreboard.player_handler._localPlayer.team = Random.Range(random_min, random_max + 1);
    }
}
