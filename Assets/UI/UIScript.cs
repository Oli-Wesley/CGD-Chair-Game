using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEditor.UI;
using UnityEngine;

//https://www.youtube.com/watch?v=Bzhlm5gdl2Y
//need to make and assign renderer
public class UIScript : MonoBehaviour
{
    public static UIScript instance;

    [SerializeField] Transform target;
    [SerializeField] Renderer targetRender;
    [SerializeField] RectTransform uiObj;
    [SerializeField] Vector3 offset;

    public Camera cam;

    private void Awake() {
        instance = this;
        cam = Camera.main;
    }

    private void FixedUpdate()
    {
        if (target != default && targetRender != default)
        {
            uiObj.gameObject.SetActive(true);

            Vector3 screenPos = cam.WorldToScreenPoint(target.position);
            Rect rect = GetScreenRect(targetRender, cam);

            Vector3 pos = screenPos + offset + new Vector3(rect.width / 2f, rect.height / 2f);

            target.position = pos;
        }
        else 
        {
            uiObj.gameObject.SetActive(false);
        }
    }

    public static Rect GetScreenRect(Renderer renderer, Camera rectCam) { 
        Bounds bounds = renderer.bounds;

        Vector3[] corners =
            {
            rectCam.WorldToScreenPoint(bounds.min), 
            rectCam.WorldToScreenPoint(bounds.max),
                                                                                               //recording each axis boundary
            rectCam.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z)), // min x min y max Z
            rectCam.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z)), // min x max Y min z
            rectCam.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z)), // max X min y min z

                                                                                               //recording diagonal axis boundary
            rectCam.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.max.z)), // min x max Y max Z
            rectCam.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.max.z)),  //max X min y max Z
            rectCam.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.min.z)),  //max X max Y min z
            };

        float minX = corners.Min(v => v.x);
        float maxX = corners.Max(v => v.x);
        float minY = corners.Min(v => v.y);
        float maxY = corners.Max(v => v.y);

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }

}
