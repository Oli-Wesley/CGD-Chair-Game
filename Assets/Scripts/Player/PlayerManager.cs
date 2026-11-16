using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player{
public class PlayerManager : MonoBehaviour
{
    public int  m_playerCount;
    public Color playerColor;
    private SceneInfo scene_info;
    private CinemachineTargetGroup  m_targetGroup;

    public void SpawnPlayer(int _playerCount)
    {
        m_playerCount = _playerCount;
        // spawn at the designated player start spawn points
        if (!scene_info){scene_info = GameObject.Find("@ SCENE INFO").GetComponent<SceneInfo>(); }
        var target =  scene_info.getSpawnLocation(m_playerCount);
        this.transform.SetPositionAndRotation(target.position, target.rotation);
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
    

    private void Awake()
    {
        scene_info = GameObject.Find("@ SCENE INFO").GetComponent<SceneInfo>();
        print (scene_info.spawnPoints.Length);
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
}
}