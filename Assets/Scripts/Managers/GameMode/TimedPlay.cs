using UnityEngine;

// OK THIS Is A TEMPLATE FOR A TIMED WIN CONDITION MODE - SO IF YOU CREATE A SCRIPTABLE
// OBJECT FROM THIS YOU CAN SPECIFY THE PLAYTIME FOR THE LEVEL BEFORE THE CONDITION = TRUE
// IT INHERITS FROM THE BASE CLASS OF WIN CONDITION WHICH IS JUST A BASIC CLASS
// TO STRUCTURE THE BASE FUNCTIONS OF A "WIN CONDITION"
// THIS ONE SPECIFICALLY IS FOR TIMED
// USE THIS AS A TEMPLATE FOR "TOTAL RUBBISH" OR SOMEWHAT

[CreateAssetMenu(fileName = "TimeCondition", menuName = "GameModes/Win Conditions/Timed Play")]
public class TimedPlay : WinCondition
{
    [Header("Time Condition")]
    public float timeLimitSeconds = 60f;
    private float _timeRemaining;

    public override void initialize()
    {
        _timeRemaining = timeLimitSeconds;
    }

    public override bool isConditionMet()
    {
        _timeRemaining -= Time.deltaTime;
        // RETURNS TRUE WHEN TIME RUNS OUT
        return _timeRemaining <= 0f;
    }

    // PUBLIC GETTER SO THE MANAGER CAN READ THE TIME FOR THE UI
    public float GetTimeRemaining()
    {
        return Mathf.Max(0f, _timeRemaining);
    }
}