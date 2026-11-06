using UnityEngine;

public enum RubbishType
{
    CARBOARD,
    PLASTIC,
    GLASS,
    METAL,
    REFUSE
}

public class IInteractable : MonoBehaviour
{
    public RubbishType rubbishType;
}
