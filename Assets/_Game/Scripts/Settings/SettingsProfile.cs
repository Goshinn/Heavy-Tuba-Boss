using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

[CreateAssetMenu(fileName = "Settings Profile", menuName = "New Settings Profile")]
public class SettingsProfile : ScriptableObject
{
    [Header("Default settings")]
    [SerializeField] private float defaultMouseSensitivity;
    [SerializeField] private float defaultControllerHorizontalSensitivity;
    [SerializeField] private float defaultControllerVerticalSensitivity;
    [SerializeField] private float defaultBGMVolume;
    [SerializeField] private float defaultSFXVolume;

    [Header("Saved settings")]
    [SerializeField] private float savedMouseSensitivity;
    [SerializeField] private float savedControllerHorizontalSensitivity;
    [SerializeField] private float savedControllerVerticalSensitivity;
    [SerializeField] private float savedBGMVolume;
    [SerializeField] private float savedSFXVolume;

    [Header("PlayerPrefs Keys")]
    private string mouseSensitivityKey = "MouseSensitivity";
    private string controllerHorizontalSensitivityKey = "ControllerHorizontalSensitivity";
    private string controllerVerticalSensitivityKey = "ControllerVerticalSensitivity";
    private string bgmVolumeKey = "BGMVolume";
    private string sfxVolumeKey = "SFXVolume";

    #region Accessors
    public float MouseSensitivity { get => savedMouseSensitivity; }
    public float ControllerVerticalSensitivity { get => savedControllerVerticalSensitivity; }
    public float ControllerHorizontalSensitivity { get => savedControllerHorizontalSensitivity; }
    public float BGMVolume { get => savedBGMVolume; }
    public float SFXVolume { get => savedSFXVolume; }
    #endregion

    private void OnValidate()
    {
        InitializeSettings();
    }

    private void InitializeSettings()
    {
        savedMouseSensitivity = PlayerPrefs.HasKey(mouseSensitivityKey) ? PlayerPrefs.GetFloat(mouseSensitivityKey) : defaultMouseSensitivity;
        savedControllerVerticalSensitivity = PlayerPrefs.HasKey(controllerVerticalSensitivityKey) ? PlayerPrefs.GetFloat(controllerVerticalSensitivityKey) : defaultControllerVerticalSensitivity;
        savedControllerHorizontalSensitivity = PlayerPrefs.HasKey(controllerHorizontalSensitivityKey) ? PlayerPrefs.GetFloat(controllerHorizontalSensitivityKey) : defaultControllerHorizontalSensitivity;

        savedBGMVolume = PlayerPrefs.HasKey(bgmVolumeKey) ? PlayerPrefs.GetFloat(bgmVolumeKey) : defaultBGMVolume;
        savedSFXVolume = PlayerPrefs.HasKey(sfxVolumeKey) ? PlayerPrefs.GetFloat(sfxVolumeKey) : defaultSFXVolume;    
    }

    #region Public Methods
    public void SetCameraLookSensitivity(float sensitivity)
    {
        savedMouseSensitivity = sensitivity;
        PlayerPrefs.SetFloat(mouseSensitivityKey, sensitivity);
    }

    public void SetControllerVerticalSensitivity(float sensitivity)
    {
        savedControllerVerticalSensitivity = sensitivity;
        PlayerPrefs.SetFloat(controllerVerticalSensitivityKey, sensitivity);
    }

    public void SetControllerHorizontalSensitivity(float sensitivity)
    {
        savedControllerHorizontalSensitivity = sensitivity;
        PlayerPrefs.SetFloat(controllerHorizontalSensitivityKey, sensitivity);
    }

    public void SetBGMVolume(float volume)
    {
        savedBGMVolume = volume;
        PlayerPrefs.SetFloat(bgmVolumeKey, volume);
        RuntimeManager.GetBus("bus:/BGM").setVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        savedSFXVolume = volume;
        PlayerPrefs.SetFloat(sfxVolumeKey, volume);
        RuntimeManager.GetBus("bus:/SFX").setVolume(volume);
    }
    #endregion
}
