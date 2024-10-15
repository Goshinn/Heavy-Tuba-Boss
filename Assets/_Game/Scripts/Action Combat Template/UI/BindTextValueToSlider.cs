using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BindTextValueToSlider : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Slider slider;
    [SerializeField] private int decimalPlace;

    [Header("References")]
    private Text textBox;

    private void Awake()
    {
        slider.onValueChanged.AddListener(UpdateTextValue);
    }

    // Called OnValueChanged by mouse sensitivity slider
    public void UpdateTextValue(float value)
    {
        if (textBox == null) textBox = GetComponent<Text>();
        textBox.text = slider.value.ToString($"F{decimalPlace}");
    }
}
