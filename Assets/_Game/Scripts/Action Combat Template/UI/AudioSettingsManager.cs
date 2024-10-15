using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SettingsProfile settingsProfile;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private void Start()
    {
        // Initialize text value according to saved sensitivity
        bgmVolumeSlider.value = settingsProfile.BGMVolume;
        sfxVolumeSlider.value = settingsProfile.SFXVolume;
    }
}
