
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ScoreboardEntry : UdonSharpBehaviour
{
    public TMPro.TextMeshProUGUI scoreText;
    public TMPro.TextMeshProUGUI teamText;
    public TMPro.TextMeshProUGUI nameText;
    void Start()
    {
        
    }

    public void DisplayScore(Player playerObject, bool show_teams)
    {
        if (playerObject == null || !playerObject.gameObject.activeSelf || playerObject.Owner == null || !playerObject.Owner.IsValid() || playerObject.team == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        if (scoreText != null)
        {
            scoreText.text = playerObject.kills.ToString();
        }
        if (teamText != null)
        {
            teamText.gameObject.SetActive(show_teams);
            teamText.text = "Team " + playerObject.team;
        }
        if (nameText != null)
        {
            nameText.text = playerObject.Owner.displayName;
        }
    }
}
