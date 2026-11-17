using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using Unity.AppUI.UI;
using UnityEngine.Serialization;

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
        
        [Header("UI")]
        [SerializeField] private UnityEngine.UI.Button playButton;
        [SerializeField] private PlayerStatusUI playerStatusUI;

        
        private readonly List<PlayerManager> _players  = new List<PlayerManager>();
        private int _mplayerCount = 0;

        //TODO: handle on player leave, currently if a controller disconnects everything breaks.
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
            playerManager.m_playerCount = _mplayerCount;
            _players.Add(playerManager);
            
            // add the player to the CINEMACHINE TARGET GROUP
            if (targetGroup != null)
            {
                targetGroup.AddMember(playerInput.transform, playerWeight, playerRadius);
            }

            //SCOREMANAGER
            PlayerScoreManager scoreManager = FindAnyObjectByType<PlayerScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.RegisterPlayer(_mplayerCount);
            }

            if (playerStatusUI != null)
            {
                playerStatusUI.UpdatePlayerStatusUI(_mplayerCount);
            }  
            
            _mplayerCount++;
            playerManager.setPlayerActive(false);
            print("Player joined" + _mplayerCount);
            
            //TODO: manage this on the button itself (or some UI manager), once again alpha jank. 
            if (_mplayerCount == maxPlayers)
            {
                playButton.interactable = true;
            }
            return;
        }

        public void spawnAllPlayers()
        {
            print("Spawning all players (" + _players.Count + ")");
            foreach (var player in _players)
            {
                player.SpawnPlayer();
            }

            //TODO: Move this to the rubbish pool, jank fix for alpha
            var rubbish = GameObject.Find("@ RubbishParent").transform;
            int count = 0;
            while (rubbish.childCount != 0)
            {
                Destroy(rubbish.GetChild(count++).gameObject);
            }
        }

        public void deSpawnAllPlayers()
        {
            foreach (var player in _players)
                player.setPlayerActive(false); 
        }
    }
}