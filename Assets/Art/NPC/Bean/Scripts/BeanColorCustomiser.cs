using DELTation.ToonRP.Extensions.BuiltIn;
using Player;
using UnityEngine;

public class BeanColor : MonoBehaviour
{
    [Header("Chair Back Component")]
    [SerializeField] private SkinnedMeshRenderer beanBody;

    [Header("Target Colour Seat")]
    [SerializeField] private Color targetColor = Color.white;

    // shader  IDs for Toon RP - SWITCH FROM URP
    private static readonly int MainColorProperty = Shader.PropertyToID("_MainColor");

    private Material beanBodyMaterialInstance;

    private void Awake()
    {
        // cache specific instances for the back and seat
        if (beanBody != null)
        {
            beanBodyMaterialInstance = beanBody.material;
        }

        // PULL COLOR FROM PARENT PLAYER MANAGER IF IT EXISTS
        PlayerManager playerManager = GetComponentInParent<PlayerManager>();
        if (playerManager != null)
        {
            targetColor = playerManager.playerColor;
        }
        else
        {
            // PICK A RANDOM COLOR IF NO PLAYER MANAGER OR COLOR IS UNASSIGNED
            targetColor = Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f);
        }

        // apply initial colours when the game starts
        SetMainColors(targetColor);
    }

    private void OnValidate()
    {
        // CALLED WHEN INSPECTOR CHANGES - EDITOR PREVIEW ONLY
        if (beanBody != null && beanBody.sharedMaterial != null)
        {
            beanBody.sharedMaterial.SetColor(MainColorProperty, targetColor);
        }
    }

    // PUBLIC FUNCTION TO CHANGE SEAT AND BACK MAIN TINTS TOGETHER
    public void SetMainColors(Color newBeanBodyColor)
    {
        targetColor = newBeanBodyColor;

        if (beanBodyMaterialInstance != null)
        {
            beanBodyMaterialInstance.SetColor(MainColorProperty, targetColor);
        }
    }
}