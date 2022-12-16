
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Scoreboard : UdonSharpBehaviour
{
    public PlayerHandler player_handler;
    public ScoreboardEntry[] entries;

    public UdonSharpBehaviour gameStartBehaviour;
    public string gameStartEvent;
    public UdonSharpBehaviour gameEndBehaviour;
    public string gameEndEvent;
    private bool unsorted = true;
    private int[] sorted;

    [HideInInspector] public int winningScore = 0;
    [HideInInspector] public string winningName = "No Winners Yet";
    

    [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(max_kills))] public int _max_kills = 25;

    public TMPro.TextMeshProUGUI winningScoreText;
    public TMPro.TextMeshProUGUI winningNameText;
    public TMPro.TextMeshProUGUI maxScoreText;

    public UnityEngine.UI.Toggle world_owner_toggle;

    private bool force_end = false;

    [UdonSyncedAttribute(UdonSyncMode.None), FieldChangeCallback(nameof(teams))] public bool _teams = false;
    [UdonSyncedAttribute(UdonSyncMode.None), FieldChangeCallback(nameof(world_owner_only))] public bool _world_owner_only = true;
    [HideInInspector] public bool game_active = false;
    public bool teams
    {
        get => _teams;
        set
        {
            _teams = value;
            RequestSerialization();
            UpdateScores();
        }
    }
    public int max_kills
    {
        get => _max_kills;
        set
        {
            _max_kills = value;
            RequestSerialization();
            if (maxScoreText != null)
            {
                maxScoreText.text = "" + value;
            }
        }
    }
    public bool world_owner_only
    {
        get => _world_owner_only;
        set
        {
            if (_world_owner_only != value)
            {
                _world_owner_only = value;
                RequestSerialization();
            }
            if (world_owner_toggle != null && world_owner_toggle.isOn != value)
            {
                world_owner_toggle.isOn = value;
            }
        }
    }

    public void RequestToggleWorldOwner()
    {
        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            world_owner_only = !world_owner_only;
        }
    }

    public void RequestTeamsOn()
    {
        if (game_active)
        {
            return;
        }
        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            TeamsOn();
        } else if (!world_owner_only)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(TeamsOn));
        }
    }
    public void RequestTeamsOff()
    {
        if (game_active)
        {
            return;
        }
        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            TeamsOff();
        } else if (!world_owner_only)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(TeamsOff));
        } 
    }

    public void TeamsOn()
    {
        teams = true;

    }
    public void TeamsOff()
    {
        teams = false;
    }
    
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


        winningName = "No Winners Yet";
        winningScore = 0;

        if (winningNameText != null)
        {
            winningNameText.text = "";
        }

        if (winningScoreText != null)
        {
            winningScoreText.text = "";
        }

        world_owner_only = world_owner_only;
        max_kills = max_kills;
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

    public void RequestMaxKillsUp()
    {
        if (game_active)
        {
            return;
        }
        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            MaxKillsUp();
        }
        else if (!world_owner_only)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(MaxKillsUp));
        }
    }

    public void RequestMaxKillsDown()
    {
        if (game_active)
        {
            return;
        }
        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            MaxKillsDown();
        }
        else if (!world_owner_only)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(MaxKillsDown));
        }
    }

    public void MaxKillsUp()
    {
        max_kills += 1;
    }

    public void MaxKillsDown()
    {
        max_kills -= 1;
        if (max_kills < 1)
        {
            max_kills = 1;
        }
    }

    public void RequestGameStart()
    {
        if (game_active)
        {
            return;
        }
        if (!world_owner_only || Networking.LocalPlayer.IsOwner(gameObject))
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(OnGameStart));
        }
    }

    public void OnGameStart()
    {
        force_end = false;
        game_active = true;
        player_handler.scores = this;
        player_handler.ResetKills();
        winningName = "Game In Progress";
        winningScore = 0;

        if (winningNameText != null)
        {
            winningNameText.text = winningName;
        }

        if (winningScoreText != null)
        {
            winningScoreText.text = "Settings Locked";
        }

        if (gameStartBehaviour != null)
        {
            gameStartBehaviour.SendCustomEvent(gameStartEvent);
        }

        if (player_handler._localPlayer != null)
        {
            player_handler.Respawn();//Will automatically force them to drop what's in their hands
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
            OnFinishSorting();
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
            OnFinishSorting();
        }
    }

    public void OnFinishSorting()
    {
        int[] teamKillCount = new int[0];
        for (int i = 0; i < entries.Length; i++)
        {
            Player p = player_handler.players[sorted[i]];
            entries[i].DisplayScore(player_handler.players[sorted[i]], teams);
            if (p != null && p.gameObject.activeSelf)
            {
                if (i == 0 && game_active)
                {
                    int top_score = p.kills;
                    if (top_score >= max_kills || (!teams && force_end))
                    {
                        winningScore = top_score;
                        winningName = p.Owner == null || !p.Owner.IsValid() ? "Player Left World" : p.Owner.displayName + " WINS!";
                        OnGameEnd();
                    }
                }

                if (teams)
                {
                    if (p.team >= teamKillCount.Length)
                    {
                        int[] newTeamKillCount = new int[p.team + 1];
                        teamKillCount.CopyTo(newTeamKillCount, 0);
                        teamKillCount = newTeamKillCount;
                    }

                    teamKillCount[p.team] = teamKillCount[p.team] + p.kills;
                }
            }
        }

        if (teams && teamKillCount.Length > 0 && game_active)
        {
            int topTeam = 0;
            int topScore = teamKillCount[0];
            for (int i = 1; i < teamKillCount.Length; i++)
            {
                if (teamKillCount[i] > topScore)
                {
                    topScore = teamKillCount[i];
                    topTeam = i;
                }
            }
            if (topScore >= max_kills || force_end)
            {
                winningScore = topScore;
                winningName = topTeam == 0 ? "DRAW" : "Team " + topTeam + " WINS!";
                OnGameEnd();
            }
        }
    }

    public void OnGameEnd()
    {
        game_active = false;
        if (winningNameText != null)
        {
            winningNameText.text = winningName;
        }

        if (winningScoreText != null)
        {
            winningScoreText.text = "Total Kills: " + winningScore;
        }

        //Here's where you would teleport people back
        if (player_handler._localPlayer != null && !force_end)
        {
            player_handler.Respawn();//Will automatically force them to drop what's in their hands
        }


        if (gameEndBehaviour != null)
        {
            gameEndBehaviour.SendCustomEvent(gameEndEvent);
        }
        force_end = false;
    }

    public void RequestGameEnd()
    {
        if (!world_owner_only || Networking.LocalPlayer.IsOwner(gameObject))
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ForceGameEnd));
        }
    }

    public void ForceGameEnd()
    {
        force_end = true;
        Sort();
    }
}
