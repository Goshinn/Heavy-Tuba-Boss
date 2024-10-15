using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SliderSetting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image image;
 
    [Header("Settings")]
    [SerializeField] private Color normalColor;
    [SerializeField] private Color selectedColor;

    private void OnValidate()
    {
        image = GetComponentInChildren<Image>(true);
        image.color = normalColor;
    }

    public void SelectSliderSetting()
    {
        //Debug.Log($"{name} has been selected");
        image.color = selectedColor;
    }

    public void DeselectSliderSetting()
    {
        //Debug.Log($"{name} has been deselected");
        image.color = normalColor;
    }
}
