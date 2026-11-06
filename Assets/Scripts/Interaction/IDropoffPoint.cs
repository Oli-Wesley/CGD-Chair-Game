using System;
using UnityEngine;
using InteractionTypes;

public class IDropoffPoint : MonoBehaviour
{
    [Tooltip("The type of rubbish that can be dropped off here")]
    public RubbishType acceptedType;
    
    [Tooltip("The size of the drop off area")]
    public Vector3 zoneSize;
    
    
   

    #region Debug Code

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.darkSeaGreen;
        Gizmos.DrawWireCube(transform.position, zoneSize);
    }
    
    // This is done the way it is below for alpha. Once we have a model, the code below can essentially be removed
    public GameObject debugCube;
    private void Awake() => UpdateZoneSize();
    private void OnValidate() => UpdateZoneSize();
    
    private void UpdateZoneSize()
    {
        float yHeight = 0.1f;
        debugCube.transform.localScale = new Vector3(zoneSize.x, yHeight, zoneSize.z);
        
        float yOffset = (zoneSize.y / 2) - (yHeight / 2);
        
        debugCube.transform.position = new Vector3(
            transform.position.x, 
            transform.position.y - yOffset, 
            transform.position.z
        );
        
        Renderer renderer = debugCube.GetComponent<Renderer>();
        Color colour = RubbishHelper.GetColourForRubbishType(acceptedType);
        renderer.sharedMaterial.SetColor("_Color", colour);
    }

    #endregion
    
}
