using InteractionTypes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Interaction
{
    public enum SpawnShape
    {
        CONE,
        CUBE,
        SPHERE
    }
    
    /*
     * Note: This definitely needs to optimisation + could do with a custom editor.
     * These are things I will add once were past the alpha stage
     *
     * - Harry
     */
    
    public class IInteractableSpawner : MonoBehaviour
    {
        #region Settings

        [Header("General Settings")]
        [Tooltip("What to spawn")]
        [SerializeField] private GameObject rubbishPrefab;
        
        [Tooltip("The parent to spawn them too (Can be null)")]
        [SerializeField] private Transform rubbishParent = null;
        
        [Tooltip("What type of rubbish to spawn")]
        [SerializeField] private RubbishType typeToSpawn;

        [Tooltip("Should it start spawning from level play")]
        [SerializeField] private bool startOnAwake = false;
        
        [Header("Spawn Settings - Amount")]
        [Tooltip("The minimum delay (in seconds) between each spawn burst")]
        [Range(0.1f, 128.0f)][SerializeField] private float minSpawnFrequency = 1.0f;
        
        [Tooltip("The maximum delay (in seconds) between each spawn burst")]
        [Range(0.1f, 128.0f)][SerializeField] private float maxSpawnFrequency = 2.0f;
        
        [Tooltip("The minimum number of rubbish to spawn per burst")]
        [Range(1, 100)][SerializeField] private int minSpawnAmount = 1;
        
        [Tooltip("The maximum number of rubbish to spawn per burst")]
        [Range(1, 100)][SerializeField] private int maxSpawnAmount = 3;
        
        [Header("Spawn Settings - Distribution")]
        
        [Tooltip("The desired 'spray' of rubbish. 'Cone' always spawns in 'forward' (+Z) direction of the emitter")]
        [SerializeField] private SpawnShape desiredSpawnShape = SpawnShape.CONE;
        
        [Tooltip("The closest to the spawner the rubbish can go")]
        [Range(0.5f, 20.0f)][SerializeField] private float minSpawnRadius = 3.0f;
        
        [Tooltip("The furthest from the spawner the rubbish can go")]
        [Range(0.0f, 20.0f)][SerializeField] private float maxSpawnRadius = 6.0f;

        [Tooltip("Total quantity this spawner can spawn (-1 = Infinite)")] 
        [Range(-1, 512)][SerializeField] private int maxAllowedToSpawn = -1;
        
        [Header("Spawn Settings - CONE ONLY")]
        [Tooltip("The spread angle of the cone")]
        [Range(1.0f, 90.0f)][SerializeField] private float coneAngle = 45f;
        
        [Tooltip("The force applied on leaving the cone")]
        [Range(10.0f, 100.0f)][SerializeField] private float spawnForce = 15f;

        #endregion

        #region Variables

        private bool _isSpawning;
        private float _nextSpawnTime = 0.0f;
        private int _spawnedCount = 0;

        #endregion
        
        #region Setup

        private void Start()
        {
            if (rubbishPrefab == null)
            {
                Debug.LogError($"[IInteractableSpawner ({gameObject.name})] Rubbish prefab needs to be set in inspector");
                return;
            }

            if (minSpawnAmount > maxSpawnAmount)
            {
                Debug.LogError($"[IInteractableSpawner ({gameObject.name})] Min spawn amount cannot be greater than max spawn amount");
                return;
            }

            if (minSpawnRadius > maxSpawnRadius)
            {
                Debug.LogError($"[IInteractableSpawner ({gameObject.name})] Min spawn radius cannot be greater than max spawn radius");
                return;
            }
            
            if (minSpawnFrequency > maxSpawnFrequency)
            {
                Debug.LogError($"[IInteractableSpawner ({gameObject.name})] Min spawn frequency cannot be greater than max spawn frequency");
                return;
            }

            if (startOnAwake) _isSpawning = true;
            else _isSpawning = false;
            
            ResetValues();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Start the spawner spawning rubbish
        /// </summary>
        public void StartSpawning()
        {
            ResetValues();
            _isSpawning = true;
        }

        /// <summary>
        /// Stop the spawner spawning rubbish
        /// </summary>
        public void StopSpawning()
        {
            _isSpawning = false;
            ResetValues();
        }

        /// <summary>
        /// Get the total number of items this spawner has spawned
        /// </summary>
        /// <returns>int</returns>
        public int GetAmountSpawned() => _spawnedCount;

        /// <summary>
        /// Set the RubbishType spawned by this spawner
        /// </summary>
        /// <param name="rubbishType">New type to spawn</param>
        public void SetRubbishType(RubbishType rubbishType)
        {
            typeToSpawn = rubbishType;
        }
        #endregion

        #region Update Logic

        private void Update()
        {
            if (!_isSpawning) return;
            if (maxAllowedToSpawn != -1 && (_spawnedCount >= maxAllowedToSpawn)) return;

            if (_nextSpawnTime <= 0.0f) SpawnRubbish();
            else _nextSpawnTime -= Time.deltaTime;
        }
        
        private void SpawnRubbish()
        {
            int numberToSpawn = Random.Range(minSpawnAmount, maxSpawnAmount);

            if (maxAllowedToSpawn != -1) 
                numberToSpawn = Mathf.Min(numberToSpawn, maxAllowedToSpawn);
            

            for (int i = 0; i < numberToSpawn; i++)
            {
                float distanceToSpawn = Random.Range(minSpawnRadius, maxSpawnRadius);
                Vector3 spawnPos = transform.position;

                if (desiredSpawnShape == SpawnShape.CUBE)
                {
                    // Spawn at a random point in the cube
                    spawnPos += new Vector3(
                        Random.Range(-1f, 1f),
                        Random.Range(-1f, 1f),
                        Random.Range(-1f, 1f)
                    ).normalized * distanceToSpawn;
                }
                else if (desiredSpawnShape == SpawnShape.SPHERE)
                {
                    // Spawn at a random point in the sphere
                    Vector3 randomDir = Random.onUnitSphere;
                    spawnPos += randomDir * distanceToSpawn;
                }

                if (desiredSpawnShape == SpawnShape.CONE)
                {
                    // Spawn at a random point in the cone
                    float randomAngle = Random.Range(0f, coneAngle);
                    float randomAxisAngle = Random.Range(0f, 360f);

                    Quaternion coneRotation = 
                        Quaternion.AngleAxis(randomAngle, Vector3.up) * Quaternion.AngleAxis(randomAxisAngle, Vector3.forward);
                    
                    Vector3 coneDirection = transform.rotation * coneRotation * Vector3.forward;

                    spawnPos += coneDirection * distanceToSpawn;
                }

                // Actually Spawn the rubbish
                if (rubbishPrefab != null)
                {
                    GameObject obj = Instantiate(rubbishPrefab, spawnPos, Random.rotation, rubbishParent);
                    
                    // Apply force to cones
                    if (desiredSpawnShape == SpawnShape.CONE)
                    {
                        Rigidbody rb = obj.GetComponent<Rigidbody>();
                        if (rb != null)
                            rb.AddForce(Vector3.forward * spawnForce, ForceMode.Impulse);
                    }
                    
                    // Set rubbish type
                    IInteractable interactable = obj.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        interactable.rubbishType = typeToSpawn;
                        interactable.UpdateRubbishColour();
                    }
                    
                    if (maxAllowedToSpawn != -1) _spawnedCount++;
                }
            }
            
            // Set the delay for the next burst
            float nextDelay = Random.Range(minSpawnFrequency, maxSpawnFrequency);
            _nextSpawnTime = nextDelay;
        }
        
        #endregion

        #region Helpers

        private void ResetValues()
        {
            _nextSpawnTime = 0.0f;
            _spawnedCount = maxAllowedToSpawn == -1 ? -1 : 0;
        }

        #endregion
        
        #region Debug Display

        private void OnDrawGizmos()
        {
            // Draw Centre
            Gizmos.color = Color.maroon;
            Gizmos.DrawSphere(transform.position, 0.2f);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 forward = transform.forward;
            Vector3 centre = transform.position;
            
            // Draw Spawn Radius
            if (desiredSpawnShape == SpawnShape.SPHERE)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(centre, maxSpawnRadius);
                Gizmos.DrawWireSphere(centre, 1.0f);
            }
            else if (desiredSpawnShape == SpawnShape.CUBE)
            {
                Gizmos.color = Color.white;
                Vector3 max_size = new Vector3(maxSpawnRadius, maxSpawnRadius, maxSpawnRadius);
                Vector3 min_size = new Vector3(minSpawnRadius, minSpawnRadius, minSpawnRadius);
                Gizmos.DrawWireCube(centre, max_size);
                Gizmos.DrawWireCube(centre, min_size);
            }
            else if (desiredSpawnShape == SpawnShape.CONE)
            {
                DrawCone(centre, forward, minSpawnRadius, maxSpawnRadius, coneAngle, Color.white);
       
                // Draw Spawn Direction Ray
                Gizmos.color = Color.red;
                Gizmos.DrawRay(centre, forward * maxSpawnRadius);
            }
        }

        private void DrawCone(Vector3 centre, Vector3 direction, float minDistance, float maxDistance, float angle,
            Color colour)
        {
            Gizmos.color = colour;
            int segments = 16;
            Quaternion rot = Quaternion.LookRotation(direction);

            float minRadius = minDistance * Mathf.Tan(angle * Mathf.Deg2Rad);
            float maxRadius = maxDistance * Mathf.Tan(angle * Mathf.Deg2Rad);

            Vector3 lastMinPoint = Vector3.zero;
            Vector3 lastMaxPoint = Vector3.zero;

            for (int i = 0; i <= segments; i++)
            {
                float rad = (i / (float)segments) * Mathf.PI * 2f;
                float cos = Mathf.Cos(rad);
                float sin = Mathf.Sin(rad);

                Vector3 localMinDir = new Vector3(cos * minRadius, sin * minRadius, minDistance);
                Vector3 localMaxDir = new Vector3(cos * maxRadius, sin * maxRadius, maxDistance);

                Vector3 worldMinPoint = centre + (rot * localMinDir);
                Vector3 worldMaxPoint = centre + (rot * localMaxDir);

                if (i > 0)
                {
                    Gizmos.DrawLine(lastMinPoint, worldMinPoint);
                    Gizmos.DrawLine(lastMaxPoint, worldMaxPoint);
                }

                Gizmos.DrawLine(worldMinPoint, worldMaxPoint);

                lastMinPoint = worldMinPoint;
                lastMaxPoint = worldMaxPoint;
            }
        }

        #endregion
    }
}

