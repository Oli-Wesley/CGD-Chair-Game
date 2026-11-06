using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using InteractionTypes;

public class InteractionManager : MonoBehaviour
{
    // 'Global' Settings
    [Header("Debug Mode")] 
    [SerializeField] private bool debugMode = true;

    [Header("References")] 
    [SerializeField] private Transform playerTransform;

    #region Pickup Settings
    [Header("Interaction Settings (Pickup)")]
    
    [Tooltip("The distance around the player interactables are acted upon")]
    [SerializeField] private float interactionRange = 5f;
    
    [Tooltip("The chosen layer for ALL interactables (Drop off points go in another layer)")]
    [SerializeField] private LayerMask interactionLayer;
    
    [Tooltip("The speed at which the interactables fly towards the player")]
    [SerializeField] private float flySpeed = 10f;

    [Tooltip("The height of the arc the rubbish follows")] 
    [SerializeField] private float arcHeight = 3f;

    [Tooltip("The distance from the player that the interactables are added to the inventory")]
    [SerializeField] private float minCollectDistance = 1f;
    #endregion
    
    #region Drop Off Settings
    [Header("Interaction Settings (Drop Off)")]
    
    [Tooltip("The distance from a bin the player must be to drop off. If 0, it's the same as Interaction Range")]
    [SerializeField] private float dropOffRange = 3f;
    
    [Tooltip("The chosen layer for ALL drop off points (Interactables go in another layer)")]
    [SerializeField] private LayerMask dropOffLayer;
    #endregion
    
    #region Variables

    private Dictionary<Transform, RubbishType> _nearbyTransforms = new Dictionary<Transform, RubbishType>();
    private Dictionary<GameObject, RubbishType> _playerInventory = new Dictionary<GameObject, RubbishType>();
    
    #endregion
    
    #region Events

    /// <summary>
    /// Event called every time rubbish is added to the player inventory
    /// </summary>
    public static event Action<RubbishType> OnRubbishCollected;
    
    /// <summary>
    /// Event called every time the player detects new rubbish
    /// </summary>
    public static event Action<RubbishType> OnRubbishDetected;
    
    #endregion
    
    #region Setup

    private void Start()
    {
        // 'Global'
        if (playerTransform == null)
        {
            Debug.LogError("[InteractionManager] No player transform assigned in inspector!");
        }
        
        // Pickup
        if (interactionRange <= 0) interactionRange = 5f;
        
        // Drop off
        if (dropOffRange <= 0) dropOffRange = interactionRange;
    }

    #endregion
    
    #region Public Getters

    /// <summary>
    /// Returns the count of all rubish in the players inventory
    /// </summary>
    /// <returns>int</returns>
    public int GetRubbishCount()
    {
        return _playerInventory.Count;
    }

    /// <summary>
    /// Returns the amount of rubbish in the players inventory for the selected type
    /// </summary>
    /// <param name="rubbishType">The type to return the count of</param>
    /// <returns>int</returns>
    public int GetRubbishCountForType(RubbishType rubbishType)
    {
        int count = 0;
        foreach (KeyValuePair<GameObject, RubbishType> pair in _playerInventory)
        {
            if (pair.Value == rubbishType)
            {
                count++;
            }
        }
        
        return count;
    }
    
    #endregion

    #region Update Logic
    
    private void FixedUpdate()
    {
        GetInteractables();

        if (_nearbyTransforms.Count > 0)
        {
            MoveInteractables();
        }
    }

    #endregion
    
    #region Item Collection Logic
    
    // Checks the interaction range for interactables and gathers them
    private void GetInteractables()
    {
        Collider[] colliders = Physics.OverlapSphere(playerTransform.position, interactionRange, interactionLayer);

        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<IInteractable>(out IInteractable interactable))
            {
                Transform objTransform = collider.transform;
                
                if (!_nearbyTransforms.ContainsKey(objTransform))
                {
                    _nearbyTransforms.Add(objTransform, interactable.rubbishType);
                    OnRubbishDetected?.Invoke(interactable.rubbishType);
                }
            }
        }
    }

    // Moves the interactables towards the player and adds them to the inventory
    private void MoveInteractables()
    {
        if (playerTransform == null) return;
        
        List<Transform> keysToRemove = new List<Transform>();
        
        foreach (KeyValuePair<Transform, RubbishType> pair in _nearbyTransforms)
        {
            Transform obj = pair.Key;
            if (obj != null)
            {
                Vector3 targetPosition = playerTransform.position;
                
                // Move the item towards the player
                Vector3 nextPos = Vector3.MoveTowards(
                    obj.position, 
                    targetPosition,
                    flySpeed * Time.deltaTime
                );
                
                // Curved Arc
                float currentDistance = Vector3.Distance(obj.position, targetPosition);
                float progress = 1f - Mathf.Clamp01(currentDistance / interactionRange);
                float heightOffset = Mathf.Sin(progress * Mathf.PI) * arcHeight;
                
                obj.position = new Vector3(nextPos.x, nextPos.y + heightOffset, nextPos.z);
                
                // Check if its close enough
                Vector3 objXZ = new Vector3(obj.position.x, targetPosition.y, obj.position.z);
                float sqrDistance = (objXZ - playerTransform.position).sqrMagnitude;
                if (sqrDistance < (minCollectDistance * minCollectDistance))
                {
                    _playerInventory.Add(obj.gameObject, pair.Value);
                    OnRubbishCollected?.Invoke(pair.Value);
                    //Destroy(obj.gameObject);
                    obj.gameObject.SetActive(false);
                    keysToRemove.Add(obj);
                    
                    // Todo: Add some cool particle effects
                }
            }
            else
            {
                keysToRemove.Add(obj);
            }
        }

        // Remove all transforms from the nearby list
        foreach (var transform in keysToRemove)
        {
            _nearbyTransforms.Remove(transform);
        }
    }
    
    #endregion
    
    #region Item Drop Off Logic
    
    
    #endregion

    #region Debug

    /// <summary>
    /// Sets the InteractionManager debug mode flag
    /// </summary>
    /// <param name="debugMode">Should it be in debug mode</param>
    public void SetDebugMode(bool debugMode) => this.debugMode = debugMode;
    
    /// <summary>
    /// Returns the current debug mode flag
    /// </summary>
    /// <returns>boolean - Is in debug mode</returns>
    public bool IsDebugMode() => debugMode;
    
    private void OnDrawGizmos()
    {
        if (!debugMode) return;
        
        Vector3 centre = playerTransform.position;
        
        // Pickup Distance
        Gizmos.color = Color.aquamarine;
        Gizmos.DrawWireSphere(centre, interactionRange);
        
        // Drop off Distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centre, dropOffRange);
    }
    
    #endregion
    
}
