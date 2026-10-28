using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;

public class UIScript : MonoBehaviour
{
    public GameObject player;
    public Canvas thisCanvas; 
    public float yOffset;
    public float xOffset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void setPosition()
    {
        float y = player.transform.position.y + yOffset;
        float x = player.transform.position.x + xOffset;

        Vector3 offsetPos = new Vector3(x, y, player.transform.position.z);

        //we have player position, now need to take ui and place it with them

        Vector2 canvasPos;
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(offsetPos);

        Rect canvasRect = thisCanvas.pixelRect;
        
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(thisCanvas, screenPoint, null, out canvasPos);

        
    }
}
