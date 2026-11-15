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
        private Animator _animatorComponent;
        private float _timer;

        private void Start()
        {
            // REFERENCES
            _navmeshAgentComponent = GetComponent<NavMeshAgent>();
            _spawnerComponent = GetComponent<IInteractableSpawner>();
            _animatorComponent = GetComponentInChildren<Animator>();

            // START DELAYED IF EXISTS
            if (_spawnerComponent != null)
            {
                StartCoroutine(DelayedStartSpawning());
            }
            // SET INITIAL DIRECTION
            SetNewRandomDestination();
        }

        private IEnumerator DelayedStartSpawning()
        {
            // RANDOM TIME BEFORE RUBBISH DROP
            float randomDelay = Random.Range(minStartDelay, maxStartDelay);
            yield return new WaitForSeconds(randomDelay);
            // START THE SPAWNER
            _spawnerComponent.StartSpawning();
        }

        private void Update()
        {
            // CHECK IF NPC HAS STOPPED MOVING AT ITS DESTINATION
            bool hasReachedDestination = !_navmeshAgentComponent.pathPending &&
                                         _navmeshAgentComponent.remainingDistance <= _navmeshAgentComponent.stoppingDistance &&
                                         _navmeshAgentComponent.velocity.sqrMagnitude <= 0.01f;

            if (hasReachedDestination)
            {
                // SET ISMOOCHING TO TRUE WHEN WAITING
                if (_animatorComponent != null)
                {
                    _animatorComponent.SetBool("isMooching", true);
                }

                _timer += Time.deltaTime;
                // WAIT THEN PICK SPOT
                if (_timer >= Random.Range(minWaitTime, maxWaitTime))
                {
                    SetNewRandomDestination();
                    // ZERO TIMER
                    _timer = 0f;
                }
            }
            else
            {
                // SET ISMOOCHING TO FALSE WHEN WALKING
                if (_animatorComponent != null)
                {
                    _animatorComponent.SetBool("isMooching", false);
                }
            }
        }

        private void SetNewRandomDestination()
        {
            // GET A RANDOM POINT AROUND NPC
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;

            // CHECK IF VALID ON NAVMESH
            NavMeshHit hit;
            // NOTE - WE COULD RESTRICT AREAS - CONSIDER FOR CARS? DUST TRUCKS? NAVMESH.ALLAREAS?
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                // SET DESINATION IF VALID
                _navmeshAgentComponent.SetDestination(hit.position);
            }
        }
    }
}