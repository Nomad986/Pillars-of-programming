using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private int globalPlatinumNumber;
    [SerializeField] private int globalGoldNumber;
    [SerializeField] private int globalSilverNumber;
    [SerializeField] private int globalCopperNumber;

    [SerializeField] private List<GameObject> areaPrefabs = new List<GameObject>();
    [SerializeField] private List<Transform> transforms = new List<Transform>();
    private List<GameObject> spawnedAreas = new List<GameObject>();

    private List<int> used = new List<int>();

    private void Awake()
    {
        foreach(Transform transform in transforms)
        {
            int rand = Random.Range(0, transforms.Count);
            while(used.Contains(rand))
            {
                rand = Random.Range(0, transforms.Count);
            }
            GameObject spawned = Instantiate(areaPrefabs[rand], 
                transform.position, transform.rotation);
            spawnedAreas.Add(spawned);
            used.Add(rand);
        }

        used.Clear();

        while(globalPlatinumNumber > 0)
        {
            int rand = Random.Range(0, transforms.Count);
            while (used.Contains(rand))
            {
                rand = Random.Range(0, transforms.Count);
            }
            //call spawn platinum in spawnedAreas
            used.Add(rand);
            globalPlatinumNumber--;
        }
    }
}
