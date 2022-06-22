using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Slider slider;
    [SerializeField] private DataScript data;

    private void Update()
    {
        text.text = slider.value.ToString();
        if (slider.name == "Enemies Slider")
        {
            data.SetEnemies((int)slider.value);
        }
        else if (slider.name == "Bars Slider")
        {
            data.SetBars((int)slider.value);
        }
    }
}
