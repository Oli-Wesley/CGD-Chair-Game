
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using InteractionTypes;
using Player;

namespace Interaction
{
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

        #region Inventory Settings

        [Header("Inventory Settings")]
        [Tooltip("The max number of rubbish a player can carry (-1 = Infinite)")] 
        [SerializeField] private int maxRubbishCount = -1;

        #endregion
        
        #region Drop Off Settings
    
        [Header("Interaction Settings (Drop Off)")]
    
        [Tooltip("The distance from a bin the player must be to drop off. If 0, it's the same as Interaction Range")]
        [SerializeField] private float dropOffRange = 3f;
        
        #endregion
    
        #region Variables

        private int _playerID = -1;
        
        private Dictionary<Transform, RubbishType> _nearbyTransforms = new Dictionary<Transform, RubbishType>();
        private Dictionary<GameObject, RubbishType> _playerInventory = new Dictionary<GameObject, RubbishType>();
        private List<GameObject> _objectsToDropOff = new List<GameObject>();
    
        private IDropoffPoint _currentDropoffPoint;
        
        private int _rubbishPickedUp = 0;
        private int _rubbishDroppedOff = 0;
        private bool _isFull = false;
        
        #endregion
    
        #region Events

        /// <summary>
        /// Event called every time rubbish is added to the player inventory
        /// Carries: RubbishType and Player ID
        /// </summary>
        public static event Action<RubbishType, int> OnRubbishCollected;
    
        /// <summary>
        /// Event called every time the player detects new rubbish
        /// Carries: RubbishType and Player ID
        /// </summary>
        public static event Action<RubbishType, int> OnRubbishDetected;
    
        /// <summary>
        /// Event called every time the player drops off rubbish
        /// Carries: RubbishType and Player ID
        /// </summary>
        public static event Action<RubbishType, int> OnRubbishDroppedOff;
        
        /// <summary>
        /// Event called when the players inventory fills up
        /// Carries: Player ID
        /// </summary>
        public static event Action<int> OnInventoryFull;
        
        #endregion
    
        #region Setup
        
        private void Start()
        {
            
            // 'Global'
            if (playerTransform == null)
            {
                Debug.LogError("[InteractionManager] No player transform assigned in inspector!");
                return;
            }

            if (_playerID == -1)
            {
                var playerManager = playerTransform.gameObject.GetComponentInParent<PlayerManager>();
                if (!playerManager)
                {
                    Debug.LogError("[InteractionManager] No PlayerManager found on player prefab!");
                    return;
                }

                _playerID = playerManager.m_playerCount;
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
        /// Returns the count of all rubish currently in the players inventory
        /// </summary>
        /// <returns>int</returns>
        public int GetRubbishCount()
        {
            return _playerInventory.Count;
        }

        /// <summary>
        /// Returns the amount of rubbish currently in the players inventory for the selected type
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


        // Note: If this is needed per RubbishType let me know and I can add it - Harry
        /// <summary>
        /// Returns the total amount of rubbish that this player picked up, in this game
        /// This is different to GetRubbishCount 
        /// </summary>
        /// <returns>int</returns>
        public int GetTotalRubbishPickedUp()
        {
            return _rubbishPickedUp;
        }
        
        /// <summary>
        /// Returns the total number of rubbish that this player dropped off, in this game
        /// </summary>
        /// <returns>int</returns>
        public int GetTotalRubbishDroppedOff()
        {
            return _rubbishDroppedOff;
        }
        
        /// <summary>
        /// Returns the inventory size for the player. -1 = Inifite
        /// </summary>
        public int GetMaxRubbishCount() => maxRubbishCount;
        #endregion

        #region Update Logic
        
        private void FixedUpdate()
        {
            // Handle Inventory Fullness
            if (maxRubbishCount != -1) _isFull = _playerInventory.Count >= maxRubbishCount;
            else _isFull = false;
            
            if (_currentDropoffPoint != null)
            {
                // Drop Off (Only in drop off)
                if (_objectsToDropOff.Count > 0)
                {
                    DropOffInventory();
                }
            }
            else
            {
                // Pick Up (Only when not in drop off)
                if (!_isFull)
                    GetInteractables();
            
                if (_nearbyTransforms.Count > 0) MoveInteractables();
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
                    // Ignore items that are being dealt with
                    if (interactable.interactableState != InteractableState.IDLE) continue;
                    
                    Transform objTransform = collider.transform;
                    interactable.interactableState = InteractableState.PICKING_UP;
                
                    if (!_nearbyTransforms.ContainsKey(objTransform))
                    {
                        _nearbyTransforms.Add(objTransform, interactable.rubbishType);
                        OnRubbishDetected?.Invoke(interactable.rubbishType, _playerID);
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
                    if (_isFull)
                    {
                        if (obj.TryGetComponent<IInteractable>(out IInteractable interactable))
                            interactable.interactableState = InteractableState.IDLE;
                        keysToRemove.Add(obj);
                        continue;
                    }
                    
                    MoveItemTowards(obj, playerTransform, flySpeed, true);
                
                    // Check if its close enough
                    Vector3 objXZ = new Vector3(obj.position.x, playerTransform.position.y, obj.position.z);
                    float sqrDistance = (objXZ - playerTransform.position).sqrMagnitude;
                    
                    if (sqrDistance < (minCollectDistance * minCollectDistance))
                    {
                        if (maxRubbishCount != -1 && (_playerInventory.Count >= maxRubbishCount))
                        {
                            _isFull = true;
                            OnInventoryFull?.Invoke(_playerID);
                            _nearbyTransforms.Clear();
                            break;
                        }
                        
                        _playerInventory.Add(obj.gameObject, pair.Value);

                        if (obj.TryGetComponent<IInteractable>(out IInteractable interactable)) 
                            interactable.interactableState = InteractableState.IN_INVENTORY;
                        
                        OnRubbishCollected?.Invoke(pair.Value, _playerID);
                        _rubbishPickedUp++;
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

        private void HandleDropOffEnter(IDropoffPoint dropOffPoint, int playerID)
        {
            if (playerID != _playerID) return;  // Skip players that aren't THIS player
            
            _currentDropoffPoint = dropOffPoint;
            GetItemsToDropOff();
        }

        private void HandleDropOffExit(IDropoffPoint dropOffPoint, int playerID)
        {
            if (playerID != _playerID) return;  // Skip players that aren't THIS player
            
            if (_currentDropoffPoint == dropOffPoint)
            {
                foreach (var obj in _objectsToDropOff)
                {
                    if (obj != null && obj.activeInHierarchy)
                    {
                        obj.SetActive(false);
                    }
                }
            
                _objectsToDropOff.Clear();
            }
        
            _currentDropoffPoint = null;
        }

        private void GetItemsToDropOff()
        {
            _objectsToDropOff.Clear();
            if (_currentDropoffPoint == null || _playerInventory.Count <= 0) return;
        
            RubbishType thisType = _currentDropoffPoint.acceptedType;

            foreach (KeyValuePair<GameObject, RubbishType> pair in _playerInventory)
            {
                if (pair.Value == thisType)
                {
                    if (pair.Key.TryGetComponent<IInteractable>(out IInteractable interactable)) 
                        interactable.interactableState = InteractableState.DROPPING_OFF;
                    
                    _objectsToDropOff.Add(pair.Key);
                }
            }
        }
    
        private void DropOffInventory()
        {
            if (_objectsToDropOff.Count <= 0) return;

            GameObject obj = _objectsToDropOff[0];

            if (obj != null)
            {
                // Make object visible if it isn't
                if (!obj.activeInHierarchy)
                {
                    Vector3 spawnPos = playerTransform.position + playerTransform.up * 1.5f;
                    obj.transform.position = spawnPos;
                    obj.SetActive(true);
                }

                // Move the objects
                Vector3 targetPos = _currentDropoffPoint.transform.position;
                MoveItemTowards(obj.transform, _currentDropoffPoint.transform, flySpeed, true);
            
                // Remove once close enough
                float sqrDistance = (obj.transform.position - targetPos).sqrMagnitude;
                if (sqrDistance < (minCollectDistance * minCollectDistance))
                {
                    if (_playerInventory.Count != 0)
                    {
                        _playerInventory.Remove(obj.gameObject);
                    }

                    _objectsToDropOff.RemoveAt(0);
                    OnRubbishDroppedOff?.Invoke(_currentDropoffPoint.acceptedType, _playerID);
                    Destroy(obj.gameObject);
                    _rubbishDroppedOff++;
                }
            }
            else
            {
                _objectsToDropOff.RemoveAt(0);
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Move items from Point A to Point B at the set speed, optionally along an arc
        /// </summary>
        /// <param name="itemToMove">Transform of the item that should be moved</param>
        /// <param name="target">Transform of the target position to move towards</param>
        /// <param name="speed">Float that is the max speed the item can move at</param>
        /// <param name="useArc">Boolean for whether to use an arc</param>
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
            
            /*
             * I want to add some code to make items shrink when they fly towards the player,
             * and grow when they leave the player.
             *
             * I'm going to leave that until after alpha tho, as it is definitely a nice to
             * have not a need to have
             */
            
        }
        
        #endregion
    
        #region Debug

        /// <summary>
        /// Sets the InteractionManager debug mode flag
        /// </summary>
        /// <param name="state">Should it be in debug mode</param>
        public void SetDebugMode(bool state) => debugMode = state;
    
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
}

