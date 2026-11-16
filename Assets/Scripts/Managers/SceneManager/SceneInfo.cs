using UnityEngine;

public class SceneInfo : MonoBehaviour
{
    
    [Tooltip("The starting spawn locations of the player, player 1 will spawn in the first slot, 2 in the second ect...")]
    [SerializeField] public Transform[] spawnPoints;
    
    // [SerializeField] public SUPPORTEDGAMEMODES supportedModes;
    
    [SerializeField] private GameObject[] respawns;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        respawns = GameObject.FindGameObjectsWithTag("SpawnPoint");
    }

    public Transform getNearestSpawnPoint(Transform targetPosition)
    {
        Transform current_shortest = null;
        var shortestDistance = float.MaxValue;
        foreach (var g in spawnPoints)
        {
            if (g == null) continue; 
            var dist = Vector3.Distance(targetPosition.position, g.transform.position);
            if (!(dist < shortestDistance)) continue;
            current_shortest = g;
            shortestDistance = dist;
        }
        return current_shortest;
    }

    public Transform getSpawnLocation(int player_id)
    {
        return spawnPoints[player_id];
    }
}

