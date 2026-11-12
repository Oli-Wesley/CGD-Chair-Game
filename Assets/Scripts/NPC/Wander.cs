using System.Collections;
using Interaction;
using UnityEngine;
using UnityEngine.AI;

namespace NPC
{
    public class Wandering : MonoBehaviour
    {
        [Header("WANDER SETTINGS")]
        [SerializeField] private float wanderRadius = 25f;
        [SerializeField] private float minWaitTime = 2f;
        [SerializeField] private float maxWaitTime = 5f;

        [Header("START TO THROW RUBBISH DELAY")]
        [SerializeField] private float minStartDelay = 15f;
        [SerializeField] private float maxStartDelay = 30f;

        private NavMeshAgent _navmeshAgentComponent;
        private IInteractableSpawner _spawnerComponent;
        private float _timer;

        private void Start()
        {
            // get reference to navmesh agent AND spawner
            _navmeshAgentComponent = GetComponent<NavMeshAgent>();
            _spawnerComponent = GetComponent<IInteractableSpawner>();
            // start the delayed spawning if spawner exists (may not be on all?)
            if (_spawnerComponent != null)
            {
                StartCoroutine(DelayedStartSpawning());
            }
            // set initial random destination
            SetNewRandomDestination();
        }

        private IEnumerator DelayedStartSpawning()
        {
            // wait for a random time between min and max delay to start dropping rubbish
            float randomDelay = Random.Range(minStartDelay, maxStartDelay);
            yield return new WaitForSeconds(randomDelay);
            // start the spawner
            _spawnerComponent.StartSpawning();
        }

        private void Update()
        {
            // check if the NPC has reached its destination
            if (!_navmeshAgentComponent.pathPending && _navmeshAgentComponent.remainingDistance <= _navmeshAgentComponent.stoppingDistance)
            {
                _timer += Time.deltaTime;
                // wait a few seconds before picking a new spot
                if (_timer >= Random.Range(minWaitTime, maxWaitTime))
                {
                    SetNewRandomDestination();
                    // zero out the timer
                    _timer = 0f;
                }
            }
        }

        private void SetNewRandomDestination()
        {
            // get a random point inside a sphere around the NPC
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;

            // check if that point is valid on the NavMesh
            NavMeshHit hit;
            // NOTE - WE COULD RESTRICT AREAS - CONSIDER FOR CARS? DUST TRUCKS? NAVMESH.ALLAREAS?
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                // set the desination if valid
                _navmeshAgentComponent.SetDestination(hit.position);
            }
        }
    }
}