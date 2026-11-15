// LIBRARY FOR ARRAYS
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    [Header("Win Conditions")]
    public List<WinCondition> winConditionsList = new List<WinCondition>();
    public GameStateManager gameStateManager;
    [Header("UI Elements")]
    public TextMeshProUGUI timerText;

    // CREATE A PLAY THROUGH CLONE OF EACH CONDITION TO PREVENT OVERWRITING BASE OBJECT VARIABLES (TIME LEFT)
    private List<WinCondition> _activeConditions = new List<WinCondition>();
    // PRESET GAME OVER TO FALSE
    private bool _isGameOver = false;

    // REFERENCE FOR THE TIMER UI
    private TimedPlay _activeTimedPlay;
    private bool isTimedGame = true;

    private void Start()
    {
        // CLEAR ANY PREVIOUS ACTIVE CONDITIONS - NEW PLAY THROUGH
        _activeConditions.Clear();

        // LOOP THE CONDITIONASSETS ARRAY
        foreach (var winCondition in winConditionsList)
        {
            if (winCondition != null)
            {
                WinCondition instance = Instantiate(winCondition);
                instance.initialize();
                // ADD TO THE ACTIVE CONDITIONS ARRAY
                _activeConditions.Add(instance);
                // CHECK IF THIS CLONED INSTANCE IS A TIMED PLAY CONDITION
                if (instance is TimedPlay timedPlayInstance)
                {
                    _activeTimedPlay = timedPlayInstance;
                }
                else
                {
                    isTimedGame = false;
                }
            }
        }
    }

    private void Update()
    {
        // IF NO ACTIVE CONDITIONS OR GAME IS OVER - DO NOTHING AND RETURN
        if (_isGameOver || _activeConditions.Count == 0) return;

        // UPDATE TIMER UI EVERY FRAME IF A TIMED PLAY CONDITION IS ACTIVE
        if (isTimedGame)
        {
            if (_activeTimedPlay != null && timerText != null)
            {
                float remainingTime = _activeTimedPlay.GetTimeRemaining();
                int displaySeconds = Mathf.CeilToInt(remainingTime);
                timerText.text = $"Time: {displaySeconds}s";
            }
        }

        bool allConditionsMet = true;

        // LOOP THROUGH EACH CONDITION
        foreach (var condition in _activeConditions)
        {
            // UNLESS ALL CONDITIONS MET - CONTINUE PLAY
            if (!condition.isConditionMet())
            {
                allConditionsMet = false;
                break;
            }
        }

        // IF ALL TRUE TRIGGER STATE CHANGE
        if (allConditionsMet)
        {
            TriggerEndGame();
        }
    }

    private void TriggerEndGame()
    {
        _isGameOver = true;
        Debug.Log("All conditions met! Ending game...");


        // TIMER TEXT END
        if (timerText != null)
        {
            timerText.text = "TIME IS OVER";
        }

        // CALLING GAME STATE MANAGER
        if (GameStateManager.instance != null)
        {
            GameStateManager.instance.changeToGameOver();
        }
    }

}