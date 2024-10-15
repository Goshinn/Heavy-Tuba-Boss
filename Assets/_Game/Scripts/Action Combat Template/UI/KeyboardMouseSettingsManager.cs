using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardMouseSettingsManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SettingsProfile settingsProfile;
    [SerializeField] private Slider mouseSensitivitySlider;

    private void Awake()
    {
        // Initialize text value according to saved cameraSensitivity
        mouseSensitivitySlider.value = settingsProfile.MouseSensitivity;
    }
}
