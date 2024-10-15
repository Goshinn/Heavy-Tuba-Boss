using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerSettingsManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SettingsProfile settingsProfile;
    [SerializeField] private Slider controllerHorizontalSensitivitySlider;
    [SerializeField] private Slider controllerVerticalSensitivitySlider;

    private void Start()
    {
        // Initialize text value according to saved sensitivity
        controllerHorizontalSensitivitySlider.value = settingsProfile.ControllerHorizontalSensitivity;
        controllerVerticalSensitivitySlider.value = settingsProfile.ControllerVerticalSensitivity;
    }
}
