using System;
using System.Collections.Generic;
using Player;
using UnityEngine;

public class CrownController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    private ParticleSystem _particles; // Particles that play when despawning or spawning the crown. 
    private MeshRenderer _meshRenderer;
    private GameObject target;

    private PlayerSpawner m_playerSpawner;
    
    
    [SerializeField] private float vertical_offset = 2.1f; 
    private State state = State.Idle;
    private enum  State
    {
        Disabled,
        Idle,
        Tracking
    }

    private void Awake()
    {
        _particles = GetComponent<ParticleSystem>();
        _meshRenderer = GetComponent<MeshRenderer>();
        m_playerSpawner  = FindFirstObjectByType<PlayerSpawner>();
        _particles.Stop();
    }

    private void Update()
    {
        if (!m_playerSpawner)
            m_playerSpawner  = FindFirstObjectByType<PlayerSpawner>(); 
        var players = m_playerSpawner.GetAllPlayersInScoreOrder();
        switch (state)
        {
            case State.Idle:
            {
                _meshRenderer.enabled = false; 
                if (players[0].GetPlayerScore() != players[1].GetPlayerScore())
                    SetTargetPlayer(players[0].transform.Find("Player Model").gameObject);
                break;
            }
            case State.Tracking:
                _meshRenderer.enabled = true; 
                transform.position = target.transform.position + target.transform.up * vertical_offset;
                transform.rotation = target.transform.rotation;
                if (players[0].GetPlayerScore() == players[1].GetPlayerScore())
                {
                    state = State.Idle;
                    _particles.Play();
                }
                break;
            case State.Disabled:
                break;
            default:
                return; 
        }
    }
    
    public void SetTargetPlayer(GameObject targetPlayer)
    {
        state = State.Tracking;
        target = targetPlayer;
        // play, move to new position, play again (plays it both when disappearing and re-appearing)
        _particles.Play();
        transform.position = target.transform.position + target.transform.up * vertical_offset;
        _particles.Play();
        
    }

    public void Reset()
    {
        if (!_meshRenderer)
            _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.enabled = false;
        state = State.Disabled;
    }
    
    public void Activate()
    {
        if (!_meshRenderer)
            _meshRenderer = GetComponent<MeshRenderer>();
        state = State.Idle;
    }
}
