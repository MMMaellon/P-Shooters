
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ScoreboardEntry : UdonSharpBehaviour
{
    public TMPro.TextMeshProUGUI scoreText;
    public TMPro.TextMeshProUGUI nameText;
    void Start()
    {
        
    }

    public void DisplayScore(Player playerObject)
    {
        if (playerObject == null || !playerObject.gameObject.activeSelf || playerObject.Owner == null || !playerObject.Owner.IsValid())
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        scoreText.text = playerObject.kills.ToString();
        nameText.text = playerObject.Owner.displayName;
    }
}
