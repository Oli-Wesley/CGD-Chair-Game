using System.Collections;
using UnityEngine;

public enum GameState
{
    None,
    MainMenu,
    PlayerSelect,
    Playing,
    Pause,
    GameOver,
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    public GameObject MainMenuUi;
    public GameObject PlayerSelectUi;
    public GameObject PlayUi;
    public GameObject PauseUi;
    public GameObject GameOverUi;

    public GameState currentState { get; private set; }
    // TRANSITION DELAY - CAN BE CHANGED
    public int delay = 1;

    // TRACK TRANSITIONS TO PREVENT OVERLAPPING COROUTINES AND SPAM
    private Coroutine _currentTransition;
    private GameState _targetState;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // FORCE THE INITIAL STATE ON START
        currentState = GameState.None;
        changeState(GameState.MainMenu);
    }

    // TRANSITION BETWEEN SCENES - RUN AS COROUTINE DELAY SET IN VARIABLE
    private IEnumerator _transitionToState(GameState newState)
    {
        if (newState != GameState.MainMenu)
        {
            yield return new WaitForSecondsRealtime(delay);
        }

        currentState = newState;
        _handleStateChange();

        // CLEAR TRANSITION REFERENCE ONCE COMPLETE
        _currentTransition = null;
    }

    private void _handleStateChange()
    {
        // HIDE ALL MENUS BEFORE ACTIVATING THE CURRENT ONE
        _hideAllMenus();

        Debug.Log($"GameState Changed To: {currentState}");
        switch (currentState)
        {
            case GameState.MainMenu:
                Time.timeScale = 0;
                _safeToSetActive(MainMenuUi, true);
                // SWITCH MUSIC HERE?
                break;
            case GameState.PlayerSelect:
                Time.timeScale = 0;
                _safeToSetActive(PlayerSelectUi, true);
                // SWITCH MUSIC HERE?
                break;
            case GameState.Playing:
                Time.timeScale = 1;
                _safeToSetActive(PlayUi, true);
                // SWITCH MUSIC HERE?
                break;
            case GameState.Pause:
                Time.timeScale = 0;
                _safeToSetActive(PauseUi, true);
                // SWITCH MUSIC HERE?
                break;
            case GameState.GameOver:
                Time.timeScale = 0;
                _safeToSetActive(GameOverUi, true);
                // SWITCH MUSIC HERE?
                break;
        }
    }

    private void _hideAllMenus()
    {
        _safeToSetActive(MainMenuUi, false);
        _safeToSetActive(PlayerSelectUi, false);
        _safeToSetActive(PlayUi, false);
        _safeToSetActive(PauseUi, false);
        _safeToSetActive(GameOverUi, false);
    }

    // PREVENT NULL REFERENCE EXCEPTIONS IF A UI PANEL IS UNASSIGNED
    private void _safeToSetActive(GameObject uiElement, bool state)
    {
        if (uiElement != null)
        {
            uiElement.SetActive(state);
        }
    }

    // PUBLIC FUNCTIONS

    public void changeState(GameState newState)
    {
        // PREVENT REDUNDANT CALLS IF ALREADY IN STATE OR TRANSITIONING TO IT
        if (currentState == newState || (_currentTransition != null && _targetState == newState))
        {
            return;
        }

        // CANCEL ONGOING TRANSITION IF SWITCHING TO A DIFFERENT STATE MID-DELAY
        if (_currentTransition != null)
        {
            StopCoroutine(_currentTransition);
        }

        _targetState = newState;
        _currentTransition = StartCoroutine(_transitionToState(newState));
    }

    // MENU SPECIFIC FOR INSPECTOR

    public void changeToMainMenu()
    {
        changeState(GameState.MainMenu);
    }
    public void changeToPlayerSelect()
    {
        changeState(GameState.PlayerSelect);
    }
    public void changeToPlaying()
    {
        changeState(GameState.Playing);
    }
    public void changeToPause()
    {
        changeState(GameState.Pause);
    }
    public void changeToGameOver()
    {
        changeState(GameState.GameOver);
    }
}