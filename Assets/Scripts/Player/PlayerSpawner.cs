using CameraControls;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace  Player
{
public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private CameraFollowController camera_controller;
    [SerializeField] public Transform[] spawnPoints;
    [SerializeField] public Color[] Colors;
    
    
    private int _mplayerCount = 0;
    
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        playerInput.transform.SetPositionAndRotation(spawnPoints[_mplayerCount].position, spawnPoints[_mplayerCount].rotation);
        var playerManager = playerInput.transform.GetComponent<PlayerManager>();
        playerManager.playerColor = Colors[_mplayerCount];
        playerManager.m_playerCount = _mplayerCount;
        camera_controller.addTarget(playerInput.transform);
        print ("spawned Player" + _mplayerCount + "at position" + spawnPoints[_mplayerCount].position); 
        _mplayerCount++;
    }
}
}
