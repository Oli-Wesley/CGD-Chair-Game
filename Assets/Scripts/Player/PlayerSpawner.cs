using UnityEngine;
using UnityEngine.InputSystem;

namespace  Player
{
public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] public Transform[] spawnPoints;
    [SerializeField] public Color[] Colors;
    
    private int _mplayerCount = 0;


    public void OnPlayerJoined(PlayerInput playerInput)
    {
        playerInput.transform.SetPositionAndRotation(spawnPoints[_mplayerCount].position, spawnPoints[_mplayerCount].rotation);
        var playerManager = playerInput.transform.GetComponent<PlayerManager>();
        playerManager.playerColor = Colors[_mplayerCount];
        playerManager.m_playerCount = _mplayerCount;
        
        print ("spawned Player" + _mplayerCount + "at position" + spawnPoints[_mplayerCount].position); 
        _mplayerCount++;
    }
}
}
