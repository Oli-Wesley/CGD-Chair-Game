using Interaction;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Player{
public class PlayerManager : MonoBehaviour
{
    public int  m_playerCount;
    public Color playerColor;
    private SceneInfo scene_info;
    private CinemachineTargetGroup  m_targetGroup;
    private InteractionManager m_interactionManager;
    
    public void SpawnPlayer()
    {
        scene_info = GameObject.Find("@ SCENE INFO").GetComponent<SceneInfo>();
        SetPlayerActive(true);
        // spawn at the designated player start spawn points
        var target =  scene_info.getSpawnLocation(m_playerCount);
        Teleport(target.position, target.rotation);
        gameObject.GetComponent<InteractionManager>().resetInventory();
        gameObject.GetComponent<PlayerController>().reset();
        
        print("Spawned player (" + m_playerCount + ")");

    }
    public void RespawnPlayer()
    {
        if (!m_targetGroup)
        {
            m_targetGroup = GameObject.Find("Target Group").GetComponent<CinemachineTargetGroup>();
        }
        var target = scene_info.getNearestSpawnPoint(m_targetGroup.Transform);
        Teleport(target.position, target.rotation);
    }
    
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Respawn")
        {
            RespawnPlayer();
        }
    }
    
    private void Teleport(Vector3 position, Quaternion rotation)
    {
        if (TryGetComponent(out CharacterController cc))
        {
            cc.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            cc.enabled = true;
        }
        else if (TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = position;
            rb.rotation = rotation;
            transform.SetPositionAndRotation(position, rotation);
        }
        else
        {
            transform.SetPositionAndRotation(position, rotation);
        }
    }
    
    public void SetPlayerActive(bool active)
    {
        foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = active;
        foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = active;

        if (TryGetComponent(out Rigidbody rb))
        {
            if (active)
            {
                rb.isKinematic = false;
            }
            else
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }

        if (TryGetComponent(out PlayerInput input))
        {
            if (active) input.ActivateInput();
            else input.DeactivateInput(); 
        }

        if (TryGetComponent(out InteractionManager im)) im.enabled = active;
    }

    public int GetPlayerScore()
    {
        //TODO: this will need changing when we refactor the interaction manager stuff. this is just a nice location to get all the information from the player for UI and the such.
        //keep the function, but just replace how it finds the score.
        if (!m_interactionManager) m_interactionManager = gameObject.GetComponent<InteractionManager>();
        return m_interactionManager.GetTotalRubbishDroppedOff();
    }
}
}