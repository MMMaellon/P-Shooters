
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Scoreboard : UdonSharpBehaviour
{
    public PlayerHandler player_handler;
    public ScoreboardEntry[] entries;
    public bool preventJoiningMidgame = true;

    public UdonBehaviour gameStartBehaviour;
    public string gameStartEvent;
    public UdonBehaviour playerRespawnBehaviour;
    public string playerRespawnEvent;
    public UdonBehaviour playerDieBehaviour;
    public string playerDieEvent;
    public UdonBehaviour gameEndBehaviour;
    public string gameEndEvent;
    [System.NonSerialized] public bool unsorted = false;
    private int[] sorted;

    [HideInInspector] public int winningScore = 0;
    [HideInInspector] public string winningName = "No Winners Yet";
    

    [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(max_kills))] public int _max_kills = 25;

    public TMPro.TextMeshProUGUI winningScoreText;
    public TMPro.TextMeshProUGUI winningNameText;
    public TMPro.TextMeshProUGUI maxScoreText;

    public UnityEngine.UI.Toggle world_owner_toggle;

    private bool force_end = false;
    private float last_end_game = -1001f;

    [UdonSyncedAttribute(UdonSyncMode.None), FieldChangeCallback(nameof(teams))] public bool _teams = false;
    [UdonSyncedAttribute(UdonSyncMode.None), FieldChangeCallback(nameof(world_owner_only))] public bool _world_owner_only = true;
    [HideInInspector, UdonSyncedAttribute(UdonSyncMode.None), FieldChangeCallback(nameof(game_active))] public bool _game_active = false;
    [UdonSyncedAttribute(UdonSyncMode.None), FieldChangeCallback(nameof(pause))] public bool _pause = false;
    public bool game_active
    {
        get => _game_active;
        set
        {
            if (_game_active != value)
            {
                if (value)
                {
                    force_end = false;
                    OnGameStart();
                }
                else
                {
                    if (Networking.LocalPlayer.IsOwner(gameObject))
                    {
                        OnGameEnd();
                    } else
                    {
                        ForceGameEnd();
                    }
                }
            }
            _game_active = value;
            RequestSerialization();
        }
    }
    public bool pause
    {
        get => _pause;
        set
        {
            _pause = value;
            RequestSerialization();

            if (value)
            {
                DisableGame();
            }
            else
            {
                EnableGame();
            }
        }
    }
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
            _world_owner_only = value;
            RequestSerialization();
            if (world_owner_toggle != null)
            {
                world_owner_toggle.isOn = _world_owner_only;
            }
        }
    }

    public void RequestToggleWorldOwner()
    {
        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            world_owner_only = !world_owner_only;
        } else
        {
            world_owner_only = world_owner_only;
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
        sorted = new int[player_handler.players.Length];
        for (int i = 0; i < sorted.Length; i++)
        {
            sorted[i] = i;
        }
        winningName = "No Winner";
        winningScore = 0;
        
        teams = teams;
        pause = pause;
        world_owner_only = world_owner_only;
        max_kills = max_kills;
        game_active = game_active;
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
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(StartGame));
        }
    }

    public void StartGame()
    {
        if(!game_active){
            game_active = true;
        }
    }

    public void OnGameStart()
    {
        if (player_handler.scores != this)
        {
            return;
        }
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

        if (player_handler._localPlayer != null && player_handler._localPlayer.team > 0)
        {
            player_handler._localPlayer.Reset();//Will automatically force them to drop what's in their hands
            if (gameStartBehaviour != null)
            {
                gameStartBehaviour.SendCustomEventDelayedFrames(gameStartEvent, 1);
            }
        }

    }

    public void RequestDisableGame()
    {
        if (!world_owner_only || Networking.LocalPlayer.IsOwner(gameObject))
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(TogglePause));
        }
    }

    public void TogglePause()
    {
        pause = !pause;
    }

    public void DisableGame()
    {
        player_handler.scores = null;

        if (winningNameText != null)
        {
            winningNameText.text = "Just RP Mode";
        }

        if (winningScoreText != null)
        {
            winningScoreText.text = "";
        }
    }

    public void EnableGame()
    {
        player_handler.scores = this;
        UpdateScores();
        if (winningNameText != null)
        {
            winningNameText.text = winningName;
        }

        if (winningScoreText != null)
        {
            winningScoreText.text = "Total Kills: " + winningScore;
        }
    }

    public void Join()
    {
        if (player_handler.scores != this || (preventJoiningMidgame && game_active))
        {
            return;
        }
        if (player_handler._localPlayer != null && player_handler._localPlayer.team == 0)
        {
            player_handler._localPlayer.team = 1;
        }
    }
    public void Leave()
    {
        if (player_handler.scores != this)
        {
            return;
        }
        if (player_handler._localPlayer != null)
        {
            player_handler._localPlayer.team = 0;
        }
    }

    public void DelayedUpdateScores()
    {
        SendCustomEventDelayedSeconds(nameof(DelayedUpateScoresCallback), 3);
    }
    
    public void DelayedUpateScoresCallback(){
        UpdateScores();
    }

    public void UpdateScores()
    {
        if (!unsorted)
        {
            unsorted = true;
            Sort();
        }
    }

    public void Sort()
    {
        Debug.LogWarning("Sort " + Time.timeSinceLevelLoad);
        if (!unsorted)
        {
            OnFinishSorting();
            return;
        }
        bool new_unsorted = false;
        for (int i = 1; i < sorted.Length; i++)//start at 1 so we can compare backwards
        {
            Player prev = player_handler.players[sorted[i - 1]];
            Player next = player_handler.players[sorted[i]];
            if (!next.gameObject.activeSelf || next.Owner == null || next.team == 0)
            {
                continue;
            }
            if ((next.gameObject.activeSelf && !prev.gameObject.activeSelf) || (next.Owner != null && prev.Owner == null) || (next.team > 0 && prev.team == 0) || (next.kills > prev.kills) || (next.kills == prev.kills && System.String.Compare(next.Owner.displayName, prev.Owner.displayName) < 0)){
                // Debug.LogWarning("prev: " + prev.Owner.displayName);
                // Debug.LogWarning("next: " + next.Owner.displayName);
                // Debug.LogWarning("(next.gameObject.activeSelf && !prev.gameObject.activeSelf) " + (next.gameObject.activeSelf && !prev.gameObject.activeSelf));
                // Debug.LogWarning("(next.Owner != null && prev.Owner == null) " + (next.Owner != null && prev.Owner == null));
                // Debug.LogWarning("(next.team > 0 && prev.team == 0) " + (next.team > 0 && prev.team == 0));
                // Debug.LogWarning("(next.kills > prev.kills) " + (next.kills > prev.kills));
                // Debug.LogWarning("System.String.Compare(next.Owner.displayName, prev.Owner.displayName) " + System.String.Compare(next.Owner.displayName, prev.Owner.displayName));
                new_unsorted = true;
                int temp = sorted[i];
                sorted[i] = sorted[i - 1];
                sorted[i - 1] = temp;
            }
        }
        unsorted = new_unsorted;
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
        Debug.LogWarning("OnFinishSorting");
        int[] teamKillCount = new int[0];
        bool game_recently_ended = last_end_game + 3f > Time.timeSinceLevelLoad;//to allow for slow network connections to get their final scores in
        for (int i = 0; i < entries.Length; i++)
        {
            Player p = player_handler.players[sorted[i]];
            entries[i].DisplayScore(player_handler.players[sorted[i]], teams);
            if (p != null && p.gameObject.activeSelf && (game_recently_ended || game_active))
            {
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
                
                if (i == 0)
                {
                    if (!teams)
                    {
                        int top_score = p.kills;
                        winningScore = top_score;
                        winningName = !p.gameObject.activeSelf || p.team == 0 || p.Owner == null || !p.Owner.IsValid() ? "No Winner" : p.Owner.displayName + " WINS!";
                        if (top_score >= max_kills || force_end)
                        {
                            force_end = false;
                            if(Networking.LocalPlayer.IsOwner(gameObject)){
                                EndGame();//sync to everyone else just in case
                            }
                            OnGameEnd();
                        }
                    }
                }
            }
        }

        if (teams && teamKillCount.Length > 0 && (game_recently_ended || game_active))
        {
            int topTeam = 0;
            int topScore = teamKillCount[0];
            int secondTopScore = -1001;
            for (int i = 1; i < teamKillCount.Length; i++)
            {
                if (teamKillCount[i] > topScore)
                {
                    secondTopScore = topScore;
                    topScore = teamKillCount[i];
                    topTeam = i;
                } else if (teamKillCount[i] > secondTopScore)
                {
                    secondTopScore = teamKillCount[i];
                }
            }
            winningScore = topScore;
            winningName = topTeam == 0 || topScore == secondTopScore ? "No Winner" : "Team " + topTeam + " WINS!";
            if (topScore >= max_kills || force_end)
            {
                force_end = false;
                if(Networking.LocalPlayer.IsOwner(gameObject)){
                    EndGame();
                }
                OnGameEnd();
            }
        }
    }

    // public void BroadcastGameEnd()
    // {
    //     SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(EndGame));
    // }

    public void EndGame()
    {
        if (game_active)
        {
            game_active = false;
        }
    }

    public void OnGameEnd()
    {
        last_end_game = Time.timeSinceLevelLoad;
        if (winningNameText != null)
        {
            winningNameText.text = winningName;
        }

        if (winningScoreText != null)
        {
            winningScoreText.text = "Total Kills: " + winningScore;
        }

        if (player_handler.scores != this)
        {
            return;
        }
        //Here's where you would teleport people back
        if (player_handler._localPlayer != null && player_handler._localPlayer.team > 0)
        {
            player_handler._localPlayer.Reset();//Will automatically force them to drop what's in their hands
            if (gameEndBehaviour != null)
            {
                gameEndBehaviour.SendCustomEventDelayedFrames(gameEndEvent, 1);
            }
        }
        
    }

    public void RequestGameEnd()
    {
        if (!world_owner_only || Networking.LocalPlayer.IsOwner(gameObject))
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(ForceGameEnd));
        }
    }

    public void ForceGameEnd()
    {
        if (player_handler.scores != this)
        {
            return;
        }
        force_end = true;
        UpdateScores();
    }

    public void OnPlayerDie()
    {
        if (playerDieBehaviour != null)
        {
            playerDieBehaviour.SendCustomEventDelayedFrames(playerDieEvent, 1);
        }
    }
    public void OnPlayerRespawned()
    {
        if (playerRespawnBehaviour != null)
        {
            playerRespawnBehaviour.SendCustomEventDelayedFrames(playerRespawnEvent, 1);
        }
    }
}
