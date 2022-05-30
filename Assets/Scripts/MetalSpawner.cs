using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalSpawner : MonoBehaviour
{
    [SerializeField] private GameObject platinumPrefab;
    [SerializeField] private GameObject goldPrefab;
    [SerializeField] private GameObject silverPrefab;
    [SerializeField] private GameObject copperPrefab;

    [SerializeField] private Transform platinumSpot;
    [SerializeField] private List<Transform> otherSpots = new List<Transform>();

    private bool platinumSpawned;

    private void Awake()
    {
        platinumSpawned = false;
    }

    public bool SpawnPlatinum()
    {
        if(platinumSpawned)
        {
            return false;
        }
        else
        {
            Instantiate(platinumPrefab, platinumSpot);
            platinumSpawned = true;
            return true;
        }
    }

    public bool SpawnOtherMetal(string name)
    {
        return false;
    }
}
