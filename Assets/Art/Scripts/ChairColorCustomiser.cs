using UnityEngine;

public class ChairColor : MonoBehaviour
{
    [Header("ChairSeat Component")]
    [SerializeField] private MeshRenderer chairSeat;
    [Header("ChairBack Component")]
    [SerializeField] private MeshRenderer chairBack;
    [Header("Color for Back")]
    [SerializeField] private Color targetBackColor = Color.white;
    [Header("Color for Seat")]
    [SerializeField] private Color targetSeatColor = Color.white;

    // variable for the equivalent of Unitys UID (GODOT) this uses a UID integer for the _MainColor property (updated for ToonRP)
    private static readonly int MainColorProperty = Shader.PropertyToID("_MainColor");

    private Material seatMaterialInstance;
    private Material backMaterialInstance;

    private void Awake()
    {
        // material instances for seat and back to prevent changing original
        if (chairSeat != null)
        {
            seatMaterialInstance = chairSeat.material;
        }

        if (chairBack != null)
        {
            backMaterialInstance = chairBack.material;
        }

        // apply the colour when the game starts (for inspector changes)
        ApplyColor(targetBackColor, targetSeatColor);
    }

    private void OnValidate()
    {
        // CALLED WHEN INSPECTOR CHANGES - THIS IS FOR EDIT ONLY *CAN BE REMOVED ON BUILD*
        if (chairSeat != null && chairSeat.sharedMaterial != null)
        {
            chairSeat.sharedMaterial.SetColor(MainColorProperty, targetSeatColor);
        }
        if (chairBack != null && chairBack.sharedMaterial != null)
        {
            chairBack.sharedMaterial.SetColor(MainColorProperty, targetBackColor);
        }
    }

    // PUBLIC METHOD TO SET COLOUR
    public void SetColor(Color newBackColor, Color newSeatColor)
    {
        if (seatMaterialInstance != null)
        {
            seatMaterialInstance.SetColor(MainColorProperty, newSeatColor);
        }
        if (backMaterialInstance != null)
        {
            backMaterialInstance.SetColor(MainColorProperty, newBackColor);
        }
    }

    // INIT METHOD FOR AWAKE
    private void ApplyColor(Color newBackColor, Color newSeatColor)
    {
        if (seatMaterialInstance != null)
        {
            seatMaterialInstance.SetColor(MainColorProperty, newSeatColor);
        }
        if (backMaterialInstance != null)
        {
            backMaterialInstance.SetColor(MainColorProperty, newBackColor);
        }
    }
}