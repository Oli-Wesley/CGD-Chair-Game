// LIBRARY FOR ARRAYS
using System.Collections.Generic;
using System.Linq;
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
        InitializeGameMode();
    }

    // RESET OBJECTIVES AND RESTART
    public void ResetGameMode()
    {
        // DESTROY OLD RUNTIME CLONES
        foreach (var condition in _activeConditions.Where(condition => condition))
        {
            Destroy(condition);
        }
        _activeConditions.Clear();

        // RESET STATE
        _isGameOver = false;
        _activeTimedPlay = null;
        isTimedGame = true;

        // FRESH CONDITIONS
        InitializeGameMode();
    }

    private void InitializeGameMode()
    {
        // LOOP THE CONDITIONASSETS ARRAY
        foreach (var instance in from winCondition in winConditionsList where winCondition select Instantiate(winCondition))
        {
            instance.Initialize();

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

    private void Update()
    {
        // IF NO ACTIVE CONDITIONS OR GAME IS OVER - DO NOTHING AND RETURN
        if (_isGameOver || _activeConditions.Count == 0) return;
        
        // UPDATE TIMER UI EVERY FRAME IF A TIMED PLAY CONDITION IS ACTIVE 
        //TODO: move this to a getTimeRemaining function or smth to give that behaviour to the timer itself.
        if (isTimedGame)
        {
            if (_activeTimedPlay != null && timerText != null)
            {
                var remainingTime = _activeTimedPlay.GetTimeRemaining();
                var displaySeconds = Mathf.CeilToInt(remainingTime);
                timerText.text = $"Time: {displaySeconds}s";
            }
        }

        var allConditionsMet = _activeConditions.All(condition => condition.IsConditionMet());

        // LOOP THROUGH EACH CONDITION

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
        if (timerText)
        {
            timerText.text = "TIME IS OVER";
        }

        // CALLING GAME STATE MANAGER
        if (GameStateManager.instance)
        {
            GameStateManager.instance.changeToGameOver();
        }
    }
}