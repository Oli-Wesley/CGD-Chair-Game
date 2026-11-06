using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using InteractionTypes;

public class InteractionManager : MonoBehaviour
{
    /*
     * Todo:
     * - Make items spawn into drop off one at a time
     * - Fix the bug where the item is not removed from the _objectsToRemove
     * - Clean up code
     */
    
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
    #endregion
    
    #region Variables

    private Dictionary<Transform, RubbishType> _nearbyTransforms = new Dictionary<Transform, RubbishType>();
    private Dictionary<GameObject, RubbishType> _playerInventory = new Dictionary<GameObject, RubbishType>();
    public List<GameObject> _objectsToDropOff = new List<GameObject>(); // Todo: Set private once bugs fixed
    
    private IDropoffPoint _currentDropoffPoint;
    
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
    
    /// <summary>
    /// Event called every time the player drops off rubbish
    /// </summary>
    public static event Action<RubbishType> OnRubbishDroppedOff;
    
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

    private void OnEnable()
    {
        IDropoffPoint.OnDropOffPointEntered += HandleDropOffEnter;
        IDropoffPoint.OnDropOffPointExited += HandleDropOffExit;
    }
    
    private void OnDisable()
    {
        IDropoffPoint.OnDropOffPointEntered -= HandleDropOffEnter;
        IDropoffPoint.OnDropOffPointExited -= HandleDropOffExit;
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
        // Pick Up
        GetInteractables();
        if (_nearbyTransforms.Count > 0) MoveInteractables();
        
        // Drop Off
        if (_currentDropoffPoint != null)
        {
            GetItemsToDropOff();
            
            if (_objectsToDropOff.Count > 0) DropOffInventory();
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
                MoveItemTowards(obj, playerTransform, flySpeed, true);
                
                // Check if its close enough
                Vector3 objXZ = new Vector3(obj.position.x, playerTransform.position.y, obj.position.z);
                float sqrDistance = (objXZ - playerTransform.position).sqrMagnitude;
                if (sqrDistance < (minCollectDistance * minCollectDistance))
                {
                    _playerInventory.Add(obj.gameObject, pair.Value);
                    OnRubbishCollected?.Invoke(pair.Value);
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

    private void HandleDropOffEnter(IDropoffPoint dropOffPoint) => _currentDropoffPoint = dropOffPoint;
    private void HandleDropOffExit(IDropoffPoint dropOffPoint) => _currentDropoffPoint = null;

    private void GetItemsToDropOff()
    {
        if (_currentDropoffPoint == null) return;
        if (_playerInventory.Count <= 0) return;
        
        _objectsToDropOff.Clear();
        
        RubbishType thisType = _currentDropoffPoint.acceptedType;

        foreach (KeyValuePair<GameObject, RubbishType> pair in _playerInventory)
        {
            if (pair.Value == thisType)
            {
                _objectsToDropOff.Add(pair.Key);
            }
        }
    }
    
    private void DropOffInventory()
    {
        if (_objectsToDropOff.Count <= 0) return;

        foreach (var obj in _objectsToDropOff)
        {
            // Show Item Again
            if (obj && !obj.activeInHierarchy)
            {
                Vector3 spawnPos = playerTransform.position + playerTransform.forward * 1.5f;
                obj.transform.position = spawnPos;
                obj.SetActive(true);
                _objectsToDropOff.Remove(obj);
                
                Collider collider = obj.GetComponent<Collider>();
                if (collider != null) collider.enabled = false;
            }

            MoveItemTowards(obj.transform, _currentDropoffPoint.transform, flySpeed, true);
            
            // Check if its close enough
            Vector3 objXZ = new Vector3(
                obj.transform.position.x, 
                _currentDropoffPoint.transform.position.y, 
                obj.transform.position.z
            );
            
            float sqrDistance = (objXZ - playerTransform.position).sqrMagnitude;
            if (sqrDistance < (minCollectDistance * minCollectDistance))
            {
                _playerInventory.Remove(obj);
                _objectsToDropOff.Remove(obj);
                OnRubbishDroppedOff?.Invoke(_currentDropoffPoint.acceptedType);
                Destroy(obj);
                // Todo: Add some cool particle effects
            }
        }
    }
    
    
    #endregion

    private void MoveItemTowards(Transform itemToMove, Transform target, float speed, bool useArc = false)
    {
        Vector3 nextPos = Vector3.MoveTowards(
            itemToMove.position,
            target.position,
            speed * Time.deltaTime
        );

        // Fly with an arc
        float heightOffset = 0;
        if (useArc)
        {
            float currentDistance = Vector3.Distance(itemToMove.position, target.position);
            float progress = 1f - Mathf.Clamp01(currentDistance / interactionRange);
            heightOffset = Mathf.Sin(progress * Mathf.PI) * arcHeight;
        }
        
        itemToMove.position = new Vector3(nextPos.x, nextPos.y + heightOffset, nextPos.z);
    }
    
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
