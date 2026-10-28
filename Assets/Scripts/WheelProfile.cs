using UnityEngine;

[CreateAssetMenu(fileName = "NewWheelProfile", menuName = "Chair/Wheel Profile")]
public class WheelProfile : ScriptableObject
{
    public string wheelName = "Standard";
    // Higher = slows down faster (rubber or carpet wheels)
    public float rollingFriction = 0.8f;
    // Multiplier for sliding/sideways grip
    public float lateralGrip = 1.0f;
}