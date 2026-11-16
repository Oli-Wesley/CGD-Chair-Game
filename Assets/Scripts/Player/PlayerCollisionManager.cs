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
    
    private ParticleSystem _hitParticles;
    private ParticleSystem _throwParticles;
    private List<ParticleSystem> _currentlyPlaying;

    private float _lastHitTime = -999f;

    private void Awake()
    {
        if (!interactionManager) interactionManager = GetComponent<InteractionManager>();
        if (!dropSpawner) dropSpawner = GetComponentInChildren<IInteractableSpawner>();

        if (!interactionManager) Debug.LogError($"[PlayerCollisionManager ({name})] No InteractionManager found");
        if (!dropSpawner) Debug.LogError($"[PlayerCollisionManager ({name})] No IInteractableSpawner found");
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        //just hit
        _currentlyPlaying.Add(Instantiate(_hitParticles, other.contacts[0].point, Quaternion.identity));
        
        if (Time.time - _lastHitTime < hitCooldown) return;
        
        if (!interactionManager || !dropSpawner) return;
        
        // only drop things if the other player is dashing.
        if (!other.gameObject.GetComponent<PlayerController>().isDashing()) return;

        // hit and throw
        _currentlyPlaying.Add(Instantiate(_throwParticles, other.contacts[0].point, Quaternion.identity));
        _lastHitTime = Time.time;
        
        List<GameObject> dropped = interactionManager.RemoveFromInventory(removalAmount);
        print(dropped.Count);
        
        if (dropped.Count > 0) dropSpawner.EjectObjects(dropped);
    }
}