using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private int globalPlatinumNumber;
    [SerializeField] private int globalGoldNumber;
    [SerializeField] private int globalSilverNumber;
    [SerializeField] private int globalCopperNumber;

    [SerializeField] private List<GameObject> areas = new List<GameObject>();
    [SerializeField] private List<Transform> transforms = new List<Transform>();

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
            Instantiate(areas[rand], transform.position, transform.rotation);
            used.Add(rand);
        }
    }
}
