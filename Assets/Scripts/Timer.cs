using System;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [Header("Timer Settings")] 
    [Tooltip("Time is in Minutes")]
    [SerializeField] private float timerLength = 5;
    [SerializeField] private bool startOnAwake = false;
    
    [Header("Debug")]
    [SerializeField] private bool outputToConsole = true;
    private int _lastSecond = -1;

    private float _timerLengthSeconds = 0.0f;
    private bool _timerRunning = false;
    private float _timeElapsed = 0.0f;

    public static event Action OnTimerEnd;
    
    #region Setup & Update

    private void Start()
    {
        _timerLengthSeconds = timerLength * 60.0f;
        if (startOnAwake) StartTimer();
    }

    private void Update()
    {
        if (_timerRunning) _timeElapsed += Time.deltaTime;

        if (_timeElapsed >= _timerLengthSeconds)
        {
            if (outputToConsole) Debug.Log("[Timer] Timer Ended!");
            StopTimer();
            OnTimerEnd?.Invoke();
        }

        if (outputToConsole)
        {
            int currentSecond = Mathf.FloorToInt(_timeElapsed);
            if (currentSecond > _lastSecond && currentSecond <= Mathf.CeilToInt(_timerLengthSeconds))
            {
                _lastSecond = currentSecond;
                Debug.Log($"[Timer] {currentSecond} second(s) elapsed");
            }
        }
    }
    
    #endregion



    #region Public API

    /// <summary>
    /// Starts the timer. If a value is passed in that is the time used
    /// </summary>
    /// <param name="newLength">Optional: New length for timer</param>
    public void StartTimer(float newLength = -1.0f)
    {
        if (newLength != -1.0f) _timerLengthSeconds = newLength * 60.0f;
        else _timerLengthSeconds = timerLength * 60.0f;
        _timerRunning = true;
    }
    
    /// <summary>
    /// Stops and resets the timer
    /// </summary>
    public void StopTimer()
    {
        _timerRunning = false;
        _timeElapsed = 0.0f;
    }

    /// <summary>
    /// Pauses timer
    /// </summary>
    public void PauseTimer()
    {
        _timerRunning = false;
    }

    /// <summary>
    /// Resumes timer when paused
    /// </summary>
    public void ResumeTimer()
    {
        if (_timeElapsed <= 0.0f) StartTimer();
        else _timerRunning = true;
    }
    
    #endregion
}
