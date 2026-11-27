using System.Collections.Generic;
using Interaction;
using Player;
using UnityEngine;

public class PlayerCollisionManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("This player's InteractionManager (auto-found on this object if left empty)")]
    [SerializeField] private InteractionManager interactionManager;

    [Tooltip("Spawner used to throw rubbish out of this player (auto-found in children if left empty)")]
    [SerializeField] private IInteractableSpawner dropSpawner;
    
    [Header("Settings")]
    [Tooltip("How much rubbish this player drops when hit")]
    [SerializeField] private int removalAmount = 10;
    
    [Tooltip("Seconds before this player can drop rubbish again (stops one bump firing several times)")]
    [SerializeField] private float hitCooldown = 1f;
    
    [Header("Particles")]
    [SerializeField] private ParticleSystem slowCollisionParticlesPrefab;
    [SerializeField] private ParticleSystem dashCollisionParticlesPrefab;
    [Tooltip("Maximum amount of collision particles a player can create")]
    [SerializeField] private int poolSize = 5;
    
    private ParticlePool _slowColPool;
    private ParticlePool _dashColPool;
    
    private float _lastHitTime = -999f;

    private void Awake()
    {
        if (!interactionManager) interactionManager = GetComponent<InteractionManager>();
        if (!dropSpawner) dropSpawner = GetComponentInChildren<IInteractableSpawner>();

        if (!interactionManager) Debug.LogError($"[PlayerCollisionManager ({name})] No InteractionManager found");
        if (!dropSpawner) Debug.LogError($"[PlayerCollisionManager ({name})] No IInteractableSpawner found");
        
    }

    private void Start()
    {
        _slowColPool = new ParticlePool(slowCollisionParticlesPrefab, poolSize);
        _dashColPool = new ParticlePool(dashCollisionParticlesPrefab, poolSize);
    }
    
    private void OnDestroy()
    {
        _slowColPool?.Destroy();
        _dashColPool?.Destroy();
    }

    private void OnCollisionEnter(Collision other)
    {
        // check for player tag
        if (!other.gameObject.CompareTag("Player")) return;
        
        // normal collision
        Vector3 hitPos = other.GetContact(0).point;
        _slowColPool.Play(hitPos);
        
        if (Time.time - _lastHitTime < hitCooldown) return;
        
        if (!interactionManager || !dropSpawner) return;
        
        // only drop things if the other player is dashing.
        if (!other.gameObject.GetComponent<PlayerController>().isDashing()) return;

        // dash collision
        _dashColPool.Play(hitPos);
        
        _lastHitTime = Time.time;
        
        List<GameObject> dropped = interactionManager.RemoveFromInventory(removalAmount);
        
        if (dropped.Count > 0) dropSpawner.EjectObjects(dropped);
    }
}