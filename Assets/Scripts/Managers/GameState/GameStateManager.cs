using System;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public enum GameState
{
    None,
    MainMenu,
    PlayerSelect,
    Playing,
    Pause,
    GameOver,
    Quit,
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager instance;

    [Header("UI References")]
    [SerializeField] private GameObject MainMenuUi;
    [SerializeField] private  GameObject PlayerSelectUi;
    [SerializeField] private  GameObject PlayUi;
    [SerializeField] private  GameObject PauseUi;
    [SerializeField] private  GameObject GameOverUi;

   
    public GameState currentState { get; private set; }
    // TRANSITION DELAY - CAN BE CHANGED
    public int delay = 1;

    //TODO: unjank this.
    [Header("Jank Button Fix")]
    [SerializeField] private  Button GameOverMenuButton;
    [SerializeField] private  Button GameOverQuitButton;

    [Tooltip("Time in Seconds")]
    [SerializeField] private float EnableDelay = 5.0f;


    // TRACK TRANSITIONS TO PREVENT OVERLAPPING COROUTINES AND SPAM
    private Coroutine _currentTransition;
    private GameState _targetState;

    [Header("REFERENCES")]
    [SerializeField] private GameModeManager gameModeManager;
    [SerializeField] private PlayerScoreManager scoreManager;
    [SerializeField] private SceneLoader sceneLoader;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
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

        // NEED TO WORK THIS OUT - ADATIVE SCENES....
        // DELAYED SCENE TRANSITION MOVED FROM BELOW
        //switch (newState)
        //{
        //    case GameState.Playing:
        //        SceneManager.LoadScene("SampleScene");
        //        break;
        //    case GameState.MainMenu:
        //        SceneManager.LoadScene("SampleScene2");
        //        break;
        //}

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
                //SceneManager.LoadScene("MainMenuScene");
                break;
            case GameState.PlayerSelect:
                Time.timeScale = 0;
                _safeToSetActive(PlayerSelectUi, true);
                // SWITCH MUSIC HERE?
                break;
            case GameState.Playing:
                Time.timeScale = 1;
                RestartGame();
                _safeToSetActive(PlayUi, true);
                // SWITCH MUSIC HERE?
                break;
            case GameState.Pause:
                Time.timeScale = 0;
                _safeToSetActive(PauseUi, true);
                // SWITCH MUSIC HERE?
                break;
            case GameState.GameOver:
                handleButtons();
                Time.timeScale = 0;
                _safeToSetActive(GameOverUi, true);
                // SWITCH MUSIC HERE?
                break;
            case GameState.Quit:
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                // SWITCH MUSIC HERE?
                break;
            case GameState.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void handleButtons()
    {
        if (GameOverMenuButton && GameOverQuitButton)
        {
            GameOverMenuButton.interactable = false;
            GameOverQuitButton.interactable = false;

            StartCoroutine(EnableButtonsAfterDelay());
        }
    }

    private IEnumerator EnableButtonsAfterDelay()
    {
        yield return new WaitForSecondsRealtime(EnableDelay);

        if (GameOverMenuButton && GameOverQuitButton)
        {
            GameOverMenuButton.interactable = true;
            GameOverQuitButton.interactable = true;
        } else
        {
            yield break;
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
        if (uiElement)
        {
            uiElement.SetActive(state);
        }
    }
    public void RestartGame()
    {
        // RESET WIN
        if (!gameModeManager)
        {
            gameModeManager = FindAnyObjectByType<GameModeManager>();
        }
        if (gameModeManager)
        {
            gameModeManager.ResetGameMode();
        }

        // WIPE SCORES
        if (!scoreManager)
        {
            scoreManager = FindAnyObjectByType<PlayerScoreManager>();
        }
        if (scoreManager)
        {
            scoreManager.ResetAllScores();
        }

        // RESET MAP
        if (!sceneLoader)
        {
            sceneLoader = FindAnyObjectByType<SceneLoader>();
        }
        if (sceneLoader)
        {
            sceneLoader.UnloadAndReloadScene();
        }
    }

    // PUBLIC FUNCTIONS

    private void changeState(GameState newState)
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
    public void changeToQuit()
    {
        changeState(GameState.Quit);
    }
}