using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private float weight;

    private void Awake()
    {
        weight = 0;
    }

    public float getInventoryWeight()
    { 
        return weight; 
    }
}
