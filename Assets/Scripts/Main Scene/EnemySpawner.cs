using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(10000)]
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject[] spawnPositions;
    private int numberOfEnemies;
    private DataScript data;

    private void Start()
    {
        data = FindObjectOfType<DataScript>();

        if(data == null)
        {
            numberOfEnemies = 7;
        }
        else
        {
            numberOfEnemies = data.GeEnemies();
        }

        for (int i = 0; i < numberOfEnemies; i++)
        {
            if (NavMesh.SamplePosition(spawnPositions[i].transform.position,
                out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                spawnPositions[i].transform.position = hit.position;
                Instantiate(enemyPrefab, spawnPositions[i].transform, false);
            }
        }
    }
}
