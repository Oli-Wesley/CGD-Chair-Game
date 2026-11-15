using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class GetUICamera : MonoBehaviour
{
    private void Awake()
    {
        Canvas canvas = GetComponent<Canvas>();

        if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
        {
            // FIND UI CAMERA BY NAME
            GameObject uiCameraObject = GameObject.Find("UICamera");

            if (uiCameraObject != null)
            {
                Camera uiCamera = uiCameraObject.GetComponent<Camera>();
                if (uiCamera != null)
                {
                    canvas.worldCamera = uiCamera;
                }
            }
            else
            {
                Debug.LogWarning($"Could not find UICamera for {gameObject.name}!");
            }
        }
    }
}