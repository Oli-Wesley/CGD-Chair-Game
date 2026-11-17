using UnityEngine;

//ABSTRACT CLASS TO FORCE EDIT
//THIS IS A BASE CLASS FOR A WIN CONDITION
public abstract class WinCondition : ScriptableObject
{
    [TextArea]
    public string description;
    public abstract void initialize();

    // RETURNS TRUE WHEN CONDITION IS MET
    public abstract bool isConditionMet();

    // MESSAGE TO DISPLAY - MAYBE FOR FUTURE? CONSIDER?
    //public abstract string GetVictoryMessage();
}
