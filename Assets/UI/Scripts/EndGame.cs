using TMPro;
using UnityEngine;

public class EndGameScoreDisplay : MonoBehaviour
{
    [Header("REFERENCES TO SCORE MANAGER")]
    [SerializeField] private PlayerScoreManager scoreManager;

    [Header("UI TEXT FOR PLAYERS")]
    [SerializeField] private TextMeshProUGUI[] playerResultTexts = new TextMeshProUGUI[4];

    private void OnEnable()
    {
        //
        if (scoreManager == null)
        {
            // NO SCORE MANAGER
        }

        DisplayAllScores();
    }

    public void DisplayAllScores()
    {
        if (scoreManager == null) return;

        // LOOP AND DISPLAY
        for (int i = 0; i < playerResultTexts.Length; i++)
        {
            if (playerResultTexts[i] != null)
            {
                int score = scoreManager.GetPlayerScore(i);
                playerResultTexts[i].text = $"Player {i + 1}: {score} Points";
            }
        }
    }
}