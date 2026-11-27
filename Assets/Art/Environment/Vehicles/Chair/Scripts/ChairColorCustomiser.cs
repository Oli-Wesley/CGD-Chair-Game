using DELTation.ToonRP.Extensions.BuiltIn;
using Player;
using UnityEngine;

public class ChairColor : MonoBehaviour
{
    [Header("Chair Seat Component")] [SerializeField]
    private MeshRenderer chairSeat;

    [Header("Chair Back Component")] [SerializeField]
    private MeshRenderer chairBack;

    [Header("Target Colour Seat")] [SerializeField]
    private Color targetSeatColor = Color.white;

    [Header("Target Colour Back")] [SerializeField]
    private Color targetBackColor = Color.white;

    // shader  IDs for Toon RP - SWITCH FROM URP
    private static readonly int MainColorProperty = Shader.PropertyToID("_MainColor");
    private MaterialPropertyBlock mpb;

    private void Awake()
    {
        PlayerManager playerManager = GetComponentInParent<PlayerManager>();
        if (playerManager != null)
        {
            targetSeatColor = playerManager.playerColor;
            targetBackColor = playerManager.playerColor;
        }

        SetMainColors(targetBackColor, targetSeatColor);
    }

    private void OnValidate()
    {
        // Editor preview only: per-renderer, never touches the .mat asset
        ApplyColor(chairSeat, targetSeatColor);
        ApplyColor(chairBack, targetBackColor);
    }

    public void SetMainColors(Color newBackColor, Color newSeatColor)
    {
        targetBackColor = newBackColor;
        targetSeatColor = newSeatColor;

        ApplyColor(chairBack, targetBackColor);
        ApplyColor(chairSeat, targetSeatColor);
    }

    private void ApplyColor(Renderer rend, Color color)
    {
        if (rend == null) return;
        mpb ??= new MaterialPropertyBlock();
        rend.GetPropertyBlock(mpb);
        mpb.SetColor(MainColorProperty, color);
        rend.SetPropertyBlock(mpb);
    }
}