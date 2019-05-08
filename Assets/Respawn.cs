using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    [SerializeField] private float minHieght = -5f;
    private string spawnPointTag = "Spawn Point";
    private List<Transform> spawnPoints = new List<Transform>();
    private Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        var spawnPointObjs = GameObject.FindGameObjectsWithTag(spawnPointTag);
        foreach (GameObject spawnPoint in spawnPointObjs)
        {
            spawnPoints.Add(spawnPoint.transform);
        }
    }
    void Update()
    {
        if (player.position.y < minHieght)
        {
            var nearestSpawnPoint = spawnPoints[0];
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (Vector3.Distance(player.position, spawnPoint.position) < Vector3.Distance(player.position, nearestSpawnPoint.position))
                {
                    nearestSpawnPoint = spawnPoint;
                }
            }

            player.position = nearestSpawnPoint.position;
        }
    }
}
