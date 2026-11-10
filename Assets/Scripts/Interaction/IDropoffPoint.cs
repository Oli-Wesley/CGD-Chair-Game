using System;
using UnityEngine;
using InteractionTypes;

[RequireComponent(typeof(Collider))]
public class IDropoffPoint : MonoBehaviour
{
    [Tooltip("The type of rubbish that can be dropped off here")]
    public RubbishType acceptedType;
    
    [Tooltip("The size of the drop off area")]
    public Vector3 zoneSize;


    #region Events

    /// <summary>
    /// Event called whenever the player enters a drop off point
    /// </summary>
    public static event Action<IDropoffPoint> OnDropOffPointEntered;
    
    /// <summary>
    /// Event called whenever a player exits a drop off point
    /// </summary>
    public static event Action<IDropoffPoint> OnDropOffPointExited;

    #endregion

    
    #region Interaction

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            OnDropOffPointEntered?.Invoke(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            OnDropOffPointExited?.Invoke(this);
        }
    }

    #endregion

    #region Setup

    private void Awake() => UpdateZoneSize();
    private void OnValidate() => UpdateZoneSize();

    private void UpdateZoneSize()
    {
        BoxCollider col = transform.GetComponent<BoxCollider>();
        if (col != null)
        {
            col.size = zoneSize;
            col.isTrigger = true;
        }
        
        UpdateDebugCube();
    }
    
    #endregion
    

    // Todo: Find a better way to do this?
    #region Debug Code
    // This is done the way it is below for alpha. Once we have a model, everything but the collider code can be removed
    public GameObject debugCube;

    private void UpdateDebugCube()
    {
        if (debugCube == null) return;
        
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
        if (renderer) renderer.sharedMaterial.SetColor("_Color", colour);
    }
    #endregion
    
}
