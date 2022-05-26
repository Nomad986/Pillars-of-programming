using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalSpawner : MonoBehaviour
{
    [SerializeField] private GameObject goldPrefab;
    [SerializeField] private GameObject silverPrefab;
    [SerializeField] private GameObject copperPrefab;
    [SerializeField] private GameObject platinumPrefab;

    [SerializeField] private Transform platinumSpot;
    [SerializeField] private List<Transform> otherSpots = new List<Transform>();

    private List<int> used = new List<int>();

    public void SpawnMetals(bool spawnPlatinum, int goldNumber, int silverNumber, int copperNumber)
    {
        if (spawnPlatinum)
        {
            Instantiate(platinumPrefab, platinumSpot.position, platinumSpot.rotation);
        }
        Iterate(goldNumber, goldPrefab);
        Iterate(silverNumber, silverPrefab);
        Iterate(copperNumber, copperPrefab);
    }

    private void Iterate(int number, GameObject metal)
    {
        for (int i = 0; i < number; i++)
        {
            int rand = Random.Range(0, otherSpots.Count);
            while(used.Contains(rand))
            {
                rand = Random.Range(0, otherSpots.Count);
            }
            Instantiate(metal, otherSpots[i].position, otherSpots[i].rotation);
            used.Add(rand);
        }
    }
}
