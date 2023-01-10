
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

#if !COMPILER_UDONSHARP && UNITY_EDITOR

using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor;
using UdonSharpEditor;
using System.Collections.Immutable;


[CustomEditor(typeof(Scoreboard))]
public class ScoreboardEditor : Editor
{

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("PLAYER SETUP");
        EditorGUILayout.HelpBox(
@"1) Drag a 'players' prefab into your scene. There should only be one. Make sure it's players as in the plural version

2) Press the setup button below or the setup button on the players object you just dragged in

3) Call me if it broke. lol", MessageType.Info);
        if (GUILayout.Button(new GUIContent("Set up ALL Scoreboards")))
        {
            foreach (Scoreboard score in GameObject.FindObjectsOfType(typeof(Scoreboard)) as Scoreboard[])
            {
                if (score != null)
                {
                    score.Reset();
                }
            }
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

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Scoreboard : UdonSharpBehaviour
{
    public PlayerHandler player_handler;
    public ScoreboardEntry[] entries;
    public bool forceTeams = false;
    public bool forceNoTeams = false;
    public bool group_by_teams = true;
    public bool team_damage = false;
    public string[] teamNames = {};
    public bool preventJoiningMidgame = true;

    public UdonBehaviour gameStartBehaviour;
    public string gameStartEvent;
    public UdonBehaviour playerRespawnBehaviour;
    public string playerRespawnEvent;
    public UdonBehaviour playerGetKillBehaviour;
    public string playerGetKillEvent;
    public UdonBehaviour playerDieBehaviour;
    public string playerDieEvent;
    public UdonBehaviour gameEndBehaviour;
    public string gameEndEvent;
    [System.NonSerialized] public bool unsorted = false;
    private int[] sorted = { };

    [HideInInspector] public int winningScore = 0;
    [HideInInspector] public string winningName = "No Winners Yet";
    

    [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(max_score))] public int _max_score = 25;

    public TMPro.TextMeshProUGUI winningScoreText;
    public TMPro.TextMeshProUGUI winningNameText;
    public TMPro.TextMeshProUGUI maxScoreText;
    public TMPro.TextMeshProUGUI teamScoreText;
    public TMPro.TextMeshProUGUI teamNamesText;
    public GameObject end_spacer;

    private bool force_end = false;
    private float last_end_game = -1001f;

    [HideInInspector, UdonSyncedAttribute(UdonSyncMode.None), FieldChangeCallback(nameof(game_active))] public bool _game_active = false;
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
    public int max_score
    {
        get => _max_score;
        set
        {
            _max_score = value;
            RequestSerialization();
            if (maxScoreText != null)
            {
                maxScoreText.text = value.ToString();
            }
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
        } else if (!player_handler.world_owner_only)
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
        } else if (!player_handler.world_owner_only)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(TeamsOff));
        } 
    }

    public void TeamsOn()
    {
        if (player_handler.scores == this)
        {
            player_handler.teams = true;
        }

    }
    public void TeamsOff()
    {
        if (player_handler.scores == this)
        {
            player_handler.teams = false;
        }
    }
    
    void Start()
    {
        for (int i = 0; i < sorted.Length; i++)
        {
            sorted[i] = i;
        }
    }

    public void OnEnable()
    {
        game_active = false;
        sorted = new int[player_handler.players.Length];
        for (int i = 0; i < sorted.Length; i++)
        {
            sorted[i] = i;
        }
        winningName = "No Winner";
        winningScore = 0;
        max_score = max_score;
        game_active = game_active;
        UpdateScores();

        if (winningNameText != null)
        {
            winningNameText.text = winningName;
        }

        if (winningScoreText != null)
        {
            winningScoreText.text = "Total Score: " + winningScore;
        }
    }

    public void OnDisable()
    {
        game_active = false;
    }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    public void Reset()
    {
        if (player_handler == null)
        {
            SerializedObject obj = new SerializedObject(this);
            obj.FindProperty("player_handler").objectReferenceValue = GameObject.FindObjectOfType<PlayerHandler>();
            obj.ApplyModifiedProperties();
        }
        if (player_handler != null)
        {
            bool missing = true;
            foreach (Scoreboard score in player_handler.scoreboards)
            {
                if (score == this)
                {
                    missing = false;
                    break;
                }
            }

            if (missing)
            {
                SerializedObject serializedHandler = new SerializedObject(player_handler);
                int index = player_handler.scoreboards.Length;
                serializedHandler.FindProperty("scoreboards").InsertArrayElementAtIndex(index);
                serializedHandler.FindProperty("scoreboards").GetArrayElementAtIndex(index).objectReferenceValue = this;
                serializedHandler.ApplyModifiedProperties();
            }
        }
    }
#endif

    public void OnGotKill()
    {
        if (playerGetKillBehaviour != null)
        {
            playerGetKillBehaviour.SendCustomEvent(playerGetKillEvent);
        }
    }

    public void IncrementScore()
    {
        if (player_handler != null && player_handler.scores == this && game_active)
        {
            player_handler._localPlayer.score++;
        }
    }
    public void DecrementScore()
    {
        if (player_handler != null && player_handler.scores == this && game_active)
        {
            player_handler._localPlayer.score--;
        }
    }
    public void IncrementScoreCustom(int custom)
    {
        if (player_handler != null && player_handler.scores == this && game_active)
        {
            player_handler._localPlayer.score += custom;
        }
    }
    public void SetScore(int custom)
    {
        if (player_handler != null && player_handler.scores == this && game_active)
        {
            player_handler._localPlayer.score = custom;
        }
    }
    
    public void ResetScore()
    {
        if (player_handler != null && player_handler.scores == this)
        {
            player_handler.ResetScore();
        }
    }

    public void RequestMaxScoreUp()
    {
        if (game_active)
        {
            return;
        }
        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            MaxScoreUp();
        }
        else if (!player_handler.world_owner_only)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(MaxScoreUp));
        }
    }

    public void RequestMaxScoreDown()
    {
        if (game_active)
        {
            return;
        }
        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            MaxScoreDown();
        }
        else if (!player_handler.world_owner_only)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(MaxScoreDown));
        }
    }

    public void MaxScoreUp()
    {
        max_score += 1;
    }

    public void MaxScoreDown()
    {
        max_score -= 1;
        if (max_score < 1)
        {
            max_score = 1;
        }
    }

    public void RequestGameStart()
    {
        if (game_active)
        {
            return;
        }
        if (!player_handler.world_owner_only || Networking.LocalPlayer.IsOwner(gameObject))
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
        player_handler.ResetScore();
        winningName = "Game In Progress";
        winningScore = 0;
        UpdateScores();

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
            player_handler._localPlayer._Reset();//Will automatically force them to drop what's in their hands
            if (gameStartBehaviour != null)
            {
                gameStartBehaviour.SendCustomEventDelayedFrames(gameStartEvent, 1);
            }
        }

    }

    public void RequestDisableGame()
    {
        if ((player_handler.scores != null && player_handler.scores.game_active) || (player_handler.world_owner_only && !Networking.LocalPlayer.IsOwner(gameObject)))
        {
            return;
        }
        if (Networking.LocalPlayer.IsOwner(gameObject))
        {
            DisableGame();
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(DisableGame));
        }
    }

    public void DisableGame()
    {
        if (player_handler.scores == this)
        {
            player_handler.scores = null;
        }
    }

    public void RequestEnableGame()
    {
        if ((player_handler.scores != null && player_handler.scores.game_active) || (player_handler.world_owner_only && !Networking.LocalPlayer.IsOwner(gameObject)))
        {
            return;
        }
        if (Networking.LocalPlayer.IsOwner(gameObject)){
            EnableGame();
        } else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(EnableGame));
        }
    }

    public void EnableGame()
    {
        player_handler.scores = this;
    }

    public void RequestToggleGame()
    {
        if (player_handler.scores == this)
        {
            RequestDisableGame();
        }
        else
        {
            RequestEnableGame();
        }
    }

    public void Join()
    {
        if (player_handler.scores != this || (preventJoiningMidgame && game_active) || sorted == null)
        {
            return;
        }
        if (player_handler._localPlayer != null)
        {
            int team_to_join = 1;
            if (player_handler.teams)
            {
                int teamCount = 2;//assume there will be at least 2 teams
                foreach (int index in sorted)
                {
                    if (index >= 0 && index < player_handler.players.Length)
                    {
                        if (player_handler.players[index] == player_handler._localPlayer)
                        {
                            continue;
                        }
                        if (player_handler.players[index].gameObject.activeSelf)
                        {
                            teamCount = teamCount < player_handler.players[index].team ? player_handler.players[index].team : teamCount;
                        } else
                        {
                            break;
                        }
                    }
                }

                int[] teamMemberCount = new int[teamCount];

                foreach (int index in sorted)
                {
                    if (index >= 0 && index < player_handler.players.Length)
                    {
                        if (player_handler.players[index] == player_handler._localPlayer)
                        {
                            continue;
                        }
                        if (player_handler.players[index].gameObject.activeSelf && player_handler.players[index].team > 0)
                        {
                            teamMemberCount[player_handler.players[index].team - 1]++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                int tiedForLowest = 0;
                int lowestCount = 1001;
                foreach (int count in teamMemberCount)
                {
                    if (lowestCount > count)
                    {
                        lowestCount = count;
                        tiedForLowest = 1;
                    } else if (lowestCount == count)
                    {
                        tiedForLowest++;
                    }
                }

                int[] lowestCounts = new int[tiedForLowest];
                int i = 0;
                for (int teamIndex = 0; teamIndex < teamMemberCount.Length; teamIndex++)
                {
                    if (lowestCount == teamMemberCount[teamIndex])
                    {
                        lowestCounts[i] = teamIndex;
                        i++;
                    }
                }

                team_to_join = lowestCounts[Random.Range(0, tiedForLowest)] + 1;
            }
            player_handler._localPlayer.team = team_to_join;
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
        if (sorted == null)
        {
            return;
        }
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
            bool should_flip = (next.gameObject.activeSelf && !prev.gameObject.activeSelf) || (next.Owner != null && prev.Owner == null) || (next.team > 0 && prev.team == 0);
            if (!should_flip) {
                bool alphabetical_order = System.String.Compare(next.Owner.displayName, prev.Owner.displayName) < 0;
                if (!player_handler.teams || !group_by_teams)
                {
                    if (next.score > prev.score)
                    {
                        should_flip = true;
                    } else if (next.score == prev.score)
                    {
                        should_flip = alphabetical_order;
                    }
                } else if (next.team < prev.team)
                {
                    should_flip = true;
                }
                else if (next.team == prev.team)
                {
                    if (next.score > prev.score)
                    {
                        should_flip = true;
                    }
                    else if (next.score == prev.score)
                    {
                        should_flip = alphabetical_order;
                    }
                }
            }

            if (should_flip)
            {
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
        if (sorted == null || sorted.Length == 0)
        {
            // SendCustomEventDelayedFrames(nameof(OnFinishSorting), 1);
            return;
        }
        int[] teamTotalScore = new int[0];
        bool game_recently_ended = last_end_game + 3f > Time.timeSinceLevelLoad;//to allow for slow network connections to get their final scores in
        for (int i = 0; i < entries.Length; i++)
        {
            Player p = player_handler.players[sorted[i]];
            entries[i].DisplayScore(this, player_handler.players[sorted[i]], player_handler.teams);
            if (p != null && p.gameObject.activeSelf)
            {
                if (player_handler.teams)
                {
                    if (p.team >= teamTotalScore.Length)
                    {
                        int[] newTeamKillCount = new int[p.team + 1];
                        teamTotalScore.CopyTo(newTeamKillCount, 0);
                        teamTotalScore = newTeamKillCount;
                    }

                    teamTotalScore[p.team] = teamTotalScore[p.team] + p.score;
                }
                
                if (i == 0 && (game_recently_ended || game_active))
                {
                    if (!player_handler.teams)
                    {
                        int top_score = p.score;
                        winningScore = top_score;
                        winningName = !p.gameObject.activeSelf || p.team == 0 || p.Owner == null || !p.Owner.IsValid() ? "No Winner" : p.Owner.displayName + " WINS!";
                        if (top_score >= max_score || force_end)
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

        if (player_handler.teams && teamTotalScore.Length > 0)
        {
            teamNamesText.transform.parent.gameObject.SetActive(true);
            teamNamesText.text = "";
            teamScoreText.text = "";
            for (int i = 1; i < teamTotalScore.Length; i++)
            {
                string teamName = i <= teamNames.Length ? teamNames[i - 1] : "Team " + i;
                if (i == 1)
                {
                    teamNamesText.text = teamName;
                    teamScoreText.text = teamTotalScore[i].ToString();
                } else
                {
                    teamNamesText.text = teamNamesText.text + "\n" + teamName;
                    teamScoreText.text = teamScoreText.text + "\n" + teamTotalScore[i].ToString();
                }
            }


            if (game_recently_ended || game_active)
                {
                    int topTeam = 0;
                    int topScore = teamTotalScore[0];
                    int secondTopScore = -1001;
                    for (int i = 1; i < teamTotalScore.Length; i++)
                    {
                        if (teamTotalScore[i] > topScore)
                        {
                            secondTopScore = topScore;
                            topScore = teamTotalScore[i];
                            topTeam = i;
                        }
                        else if (teamTotalScore[i] > secondTopScore)
                        {
                            secondTopScore = teamTotalScore[i];
                        }
                    }
                    winningScore = topScore;
                    string teamName = topTeam > 0 && topTeam <= teamNames.Length ? teamNames[topTeam - 1] : "Team " + topTeam;
                    winningName = topTeam == 0 || topScore == secondTopScore ? "No Winner" : teamName + " WINS!";
                    if (topScore >= max_score || force_end)
                    {
                        force_end = false;
                        if (Networking.LocalPlayer.IsOwner(gameObject))
                        {
                            EndGame();
                        }
                        OnGameEnd();
                    }
                }
        } else
        {
            teamNamesText.transform.parent.gameObject.SetActive(false);
        }

        //forces text to update properly
        SendCustomEventDelayedFrames(nameof(ToggleSpacer), 5);//picked an arbitrary number. 1 doesnt work
    }

    public void ToggleSpacer()
    {
        end_spacer.SetActive(!end_spacer.activeSelf);
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
            winningScoreText.text = "Total Score: " + winningScore;
        }

        if (player_handler.scores != this)
        {
            return;
        }
        //Here's where you would teleport people back
        if (player_handler._localPlayer != null && player_handler._localPlayer.team > 0)
        {
            player_handler._localPlayer._Reset();//Will automatically force them to drop what's in their hands
            if (gameEndBehaviour != null)
            {
                gameEndBehaviour.SendCustomEventDelayedFrames(gameEndEvent, 1);
            }
        }
        
    }

    public void RequestGameEnd()
    {
        if (!player_handler.world_owner_only || Networking.LocalPlayer.IsOwner(gameObject))
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
