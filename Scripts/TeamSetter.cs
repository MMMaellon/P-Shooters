
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    public class TeamSetter : UdonSharpBehaviour
    {
        public Scoreboard scoreboard;
        [Header("Set Team to 0 to exit game")]
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
            if (scoreboard.game_active && scoreboard.preventJoiningMidgame && team != 0)
            {
                return;
            }
            scoreboard.player_handler._localPlayer.team = team;
        }
        public void SetRandomTeam()
        {
            if (scoreboard.game_active && scoreboard.preventJoiningMidgame)
            {
                return;
            }
            scoreboard.player_handler._localPlayer.team = Random.Range(random_min, random_max + 1);
        }
    }
}