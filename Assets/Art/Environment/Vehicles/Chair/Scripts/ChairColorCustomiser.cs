using DELTation.ToonRP.Extensions.BuiltIn;
using Player;
using UnityEngine;

public class ChairColor : MonoBehaviour
{
    [Header("Chair Seat Component")]
    [SerializeField] private MeshRenderer chairSeat;
    [Header("Chair Back Component")]
    [SerializeField] private MeshRenderer chairBack;

    [Header("Target Colour Seat")]
    [SerializeField] private Color targetSeatColor = Color.white;
    [Header("Target Colour Back")]
    [SerializeField] private Color targetBackColor = Color.white;

    // shader  IDs for Toon RP - SWITCH FROM URP
    private static readonly int MainColorProperty = Shader.PropertyToID("_MainColor");

    private Material seatMaterialInstance;
    private Material backMaterialInstance;

    private void Awake()
    {
        // cache specific instances for the back and seat
        if (chairSeat != null)
        {
            seatMaterialInstance = chairSeat.material;
        }

        if (chairBack != null)
        {
            backMaterialInstance = chairBack.material;
        }

        // PULL COLOR FROM PARENT PLAYER MANAGER IF IT EXISTS
        PlayerManager playerManager = GetComponentInParent<PlayerManager>();
        if (playerManager != null)
        {
            targetSeatColor = playerManager.playerColor;
            targetBackColor = playerManager.playerColor;
        }

        // apply initial colours when the game starts
        SetMainColors(targetBackColor, targetSeatColor);
    }

    private void OnValidate()
    {
        // CALLED WHEN INSPECTOR CHANGES - EDITOR PREVIEW ONLY
        if (chairSeat != null && chairSeat.sharedMaterial != null)
        {
            chairSeat.sharedMaterial.SetColor(MainColorProperty, targetSeatColor);
        }
        if (chairBack != null && chairBack.sharedMaterial != null)
        {
            chairBack.sharedMaterial.SetColor(MainColorProperty, targetBackColor);
        }
    }

    // PUBLIC FUNCTION TO CHANGE SEAT AND BACK MAIN TINTS TOGETHER
    public void SetMainColors(Color newBackColor, Color newSeatColor)
    {
        targetBackColor = newBackColor;
        targetSeatColor = newSeatColor;

        if (backMaterialInstance != null)
        {
            backMaterialInstance.SetColor(MainColorProperty, targetBackColor);
        }
        if (seatMaterialInstance != null)
        {
            seatMaterialInstance.SetColor(MainColorProperty, targetSeatColor);
        }
    }
}