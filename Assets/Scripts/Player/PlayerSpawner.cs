using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace Player
{
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private CinemachineTargetGroup targetGroup;
        [SerializeField] public Transform[] spawnPoints;
        [SerializeField] public Color[] Colors;

        [Header("CINEMACHINE SETTINGS")]
        [SerializeField] private float playerWeight = 1f;
        [SerializeField] private float playerRadius = 1f;

        [Header("PLAYER LIMITS")]
        // num of max players
        [SerializeField] private int maxPlayers = 4;

        private int _mplayerCount = 0;

        public void OnPlayerJoined(PlayerInput playerInput)
        {

            // check if we have already reached the maximum player limit
            if (_mplayerCount >= maxPlayers)
            {
                // NOT NEEDED FOR UI?
                print("Max player limit reached (" + maxPlayers + "). Ignoring extra join request.");

                // destroy the extra player input object
                Destroy(playerInput.gameObject);
                return;
            }

            // NOTE - COULD SIGNAL SOME UI STUFF HERE?

            // position and rotate the newly joined player
            playerInput.transform.SetPositionAndRotation(spawnPoints[_mplayerCount].position, spawnPoints[_mplayerCount].rotation);

            // configure player color and ID
            var playerManager = playerInput.transform.GetComponent<PlayerManager>();
            playerManager.playerColor = Colors[_mplayerCount];
            playerManager.m_playerCount = _mplayerCount;

            // add the player to the CINEMACHINE TARGET GROUP
            if (targetGroup != null)
            {
                targetGroup.AddMember(playerInput.transform, playerWeight, playerRadius);
            }

            // OR HERE SIGNAL SOME UI STUFF?
            print("spawned Player " + _mplayerCount + " at position " + spawnPoints[_mplayerCount].position);
            _mplayerCount++;
        }
    }
}