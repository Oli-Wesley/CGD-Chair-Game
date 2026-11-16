using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace Player
{
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private CinemachineTargetGroup targetGroup;
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
            
            // configure player color and ID
            var playerManager = playerInput.transform.GetComponent<PlayerManager>();
            playerManager.playerColor = Colors[_mplayerCount];
            playerManager.SpawnPlayer(_mplayerCount);

            // add the player to the CINEMACHINE TARGET GROUP
            if (targetGroup != null)
            {
                targetGroup.AddMember(playerInput.transform, playerWeight, playerRadius);
            }

            // OR HERE SIGNAL SOME UI STUFF?
            print("spawned Player " + _mplayerCount);
            _mplayerCount++;
        }
    }
}