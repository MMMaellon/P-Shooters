
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Scoreboard : UdonSharpBehaviour
{
    public PlayerHandler player_handler;
    public ScoreboardEntry[] entries;
    private bool unsorted = true;
    private int[] sorted;
    void Start()
    {
        if (player_handler == null)
        {
            player_handler = GameObject.Find("__GUN MANAGER__").GetComponentInChildren<PlayerHandler>();
        }
        player_handler.scores = this;
        sorted = new int[player_handler.players.Length];
        for (int i = 0; i < sorted.Length; i++)
        {
            sorted[i] = i;
        }
        UpdateScores();
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

    public void ResetKills()
    {
        if (player_handler == null)
        {
            player_handler.BroadcastResetKills();
        }
    }

    public void DelayedUpdateScores()
    {
        SendCustomEventDelayedFrames(nameof(UpdateScores), 5);
    }

    public void UpdateScores()
    {
        unsorted = true;
        Sort();
    }

    public void Sort()
    {
        if (!unsorted)
        {
            return;
        }
        unsorted = false;
        for (int i = 1; i < sorted.Length; i++)//start at 1 so we can compare backwards
        {
            Player prev = player_handler.players[sorted[i - 1]];
            Player next = player_handler.players[sorted[i]];
            if ((next.gameObject.activeSelf && !prev.gameObject.activeSelf) || (next.Owner != null && prev.Owner == null) || (next.kills > prev.kills) || (next.Owner != null && System.String.Compare(next.Owner.displayName, prev.Owner.displayName) < 0)){
                unsorted = true;
                int temp = sorted[i];
                sorted[i] = sorted[i - 1];
                sorted[i - 1] = temp;
            }
        }

        if (unsorted)
        {
            SendCustomEventDelayedFrames(nameof(Sort), 1);
        } else
        {
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i].DisplayScore(player_handler.players[sorted[i]]);
            }
        }
    }
}
