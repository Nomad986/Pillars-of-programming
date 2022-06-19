using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(10000)]
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject[] spawnPositions;
    private int numberOfEnemies = 3;
    private List<int> used = new List<int>();

    private void Start()
    {
        while(numberOfEnemies > 0)
        {
            int rand = Random.Range(0, numberOfEnemies);

            if (!used.Contains(rand) &&
                NavMesh.SamplePosition(spawnPositions[rand].transform.position, 
                out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                spawnPositions[rand].transform.position = hit.position;
                Instantiate(enemyPrefab, spawnPositions[rand].transform, false);
                used.Add(rand);
                numberOfEnemies--;
            }
        }

        /*for (int i = 0; i < spawnPositions.Length; i++)
        {
            if (NavMesh.SamplePosition(spawnPositions[i].transform.position,
                out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                spawnPositions[i].transform.position = hit.position;
                Instantiate(enemyPrefab, spawnPositions[i].transform, false);
            }
        }*/
    }
}
