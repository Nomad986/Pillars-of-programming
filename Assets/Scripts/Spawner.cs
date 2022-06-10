using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Range(0, 6)] [SerializeField] private int globalPlatinumNumber;
    [SerializeField] private int globalGoldNumber;
    [SerializeField] private int globalSilverNumber;
    [SerializeField] private int globalCopperNumber;

    [SerializeField] private GameObject platinumPrefab;
    [SerializeField] private GameObject goldPrefab;
    [SerializeField] private GameObject silverPrefab;
    [SerializeField] private GameObject copperPrefab;

    [SerializeField] private List<GameObject> areaPrefabs = new List<GameObject>();
    [SerializeField] private List<Transform> transforms = new List<Transform>();
    private List<GameObject> spawnedAreas = new List<GameObject>();

    private List<int> used = new List<int>();

    [SerializeField] private Terrain terrain;
    private NavMeshBuilder builder;

    private void Awake()
    {
        builder = terrain.GetComponent<NavMeshBuilder>();

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
            while (spawnedAreas[rand].CompareTag("Nonentrable") || used.Contains(rand))
            {
                rand = Random.Range(0, transforms.Count);
            }
            SpawnPlatinum(spawnedAreas[rand]);
            used.Add(rand);
            globalPlatinumNumber--;
        }

        used.Clear();

        while(globalGoldNumber > 0)
        {
            int rand = Random.Range(0, transforms.Count);
            while (spawnedAreas[rand].CompareTag("Nonentrable") || used.Contains(rand))
            {
                rand = Random.Range(0, transforms.Count);
            }

            bool spawned = SpawnOtherMetal(spawnedAreas[rand], goldPrefab);
            if (spawned)
            {
                globalGoldNumber--;
                used.Add(rand);
                if(used.Count > 5)
                {
                    used.Clear();
                }
            }
        }

        while (globalSilverNumber > 0)
        {
            int rand = Random.Range(0, transforms.Count);
            while (spawnedAreas[rand].CompareTag("Nonentrable") || used.Contains(rand))
            {
                rand = Random.Range(0, transforms.Count);
            }

            bool spawned = SpawnOtherMetal(spawnedAreas[rand], silverPrefab);
            if (spawned)
            {
                globalSilverNumber--;
                used.Add(rand);
                if (used.Count > 5)
                {
                    used.Clear();
                }
            }
        }

        while (globalCopperNumber > 0)
        {
            int rand = Random.Range(0, transforms.Count);
            while (spawnedAreas[rand].CompareTag("Nonentrable") || used.Contains(rand))
            {
                rand = Random.Range(0, transforms.Count);
            }

            bool spawned = SpawnOtherMetal(spawnedAreas[rand], copperPrefab);
            if (spawned)
            {
                globalCopperNumber--;
                used.Add(rand);
                if (used.Count > 5)
                {
                    used.Clear();
                }
            }
        }
    }

    private void Start()
    {
        builder.Build();
    }

    private void SpawnPlatinum(GameObject location)
    {
        BuildingPrefabData data = location.GetComponent<BuildingPrefabData>();
        if(data != null)
        {
            Instantiate(platinumPrefab, data.platinumSpot.spot.transform.position,
                data.platinumSpot.spot.transform.rotation);
        }
        else
        {
            //exception
        }
    }

    private bool SpawnOtherMetal(GameObject location, GameObject metalPrefab)
    {
        BuildingPrefabData data = location.GetComponent<BuildingPrefabData>();
        if (data != null)
        {
            if (!data.full)
            {
                int rand = Random.Range(0, data.otherSpots.Count);
                while (data.otherSpots[rand].taken)
                {
                    rand = Random.Range(0, data.otherSpots.Count);
                }
                Instantiate(metalPrefab, data.otherSpots[rand].spot.transform.position,
                    data.otherSpots[rand].spot.transform.rotation);
                data.otherSpots[rand].taken = true;
                data.CheckFullness();
                return true;
            }
            else
            {
                return false;
            }
        }
        {
            //exception
            return false;
        }
    }
}
