using System.Collections.Generic;
using UnityEngine;

public class BuildingPrefabData : MonoBehaviour
{
    [System.Serializable]
    public class MetalSpot
    {
        public Transform spot;
        public bool taken;
    }

    public bool full = false;

    public MetalSpot platinumSpot;
    public List<MetalSpot> otherSpots = new List<MetalSpot>();

    public void CheckFullness()
    {
        List<MetalSpot> takenSpots = otherSpots.FindAll(FindTakenSpots);
        if (takenSpots.Count == otherSpots.Count)
        {
            full = true;
        }
    }

    private static bool FindTakenSpots(MetalSpot spot)
    {
        if (spot.taken)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
