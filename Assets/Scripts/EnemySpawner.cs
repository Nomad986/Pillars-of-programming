using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(10000)]
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject[] spawnPositions;

    private void Start()
    {
        for (int i = 0; i < spawnPositions.Length; i++)
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
