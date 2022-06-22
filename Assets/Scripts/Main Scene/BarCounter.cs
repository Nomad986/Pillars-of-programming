using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BarCounter : MonoBehaviour
{
    [SerializeField] GameObject hole;
    [SerializeField] private TextMeshProUGUI text;

    private void Update()
    {
        text.text = "Bars to collect: " + hole.GetComponent<QuestHole>().GetRemainingBars().ToString();
    }
}
