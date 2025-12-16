using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Options Menu Manager - Handles game settings
/// Graphics, Audio, and Controls settings
/// Will integrate with AudioManager after it's created
/// </summary>
public class OptionsMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MenuManager menuManager;

    [Header("Graphics Settings")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vsyncToggle;

    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;

    [Header("Controls Settings")]
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private TextMeshProUGUI mouseSensitivityText;
    [SerializeField] private Toggle invertYToggle;

    [Header("PlayerPrefs Keys")]
    [SerializeField] private string masterVolumeKey = "MasterVolume";
    [SerializeField] private string musicVolumeKey = "MusicVolume";
    [SerializeField] private string sfxVolumeKey = "SFXVolume";
    [SerializeField] private string qualityKey = "QualityLevel";
    [SerializeField] private string fullscreenKey = "Fullscreen";
    [SerializeField] private string vsyncKey = "VSync";
    [SerializeField] private string mouseSensKey = "MouseSensitivity";
    [SerializeField] private string invertYKey = "InvertY";

    private Resolution[] resolutions;
    private int currentResolutionIndex;

    private void Awake()
    {
        if (menuManager == null)
        {
            menuManager = FindFirstObjectByType<MenuManager>();
        }
    }

    private void Start()
    {
        InitializeSettings();
        LoadSettings();
    }

    #region Initialization

    private void InitializeSettings()
    {
        // Initialize Graphics
        InitializeQualitySettings();
        InitializeResolutionSettings();

        // Add listeners
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(SetQuality);

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(SetResolution);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        if (vsyncToggle != null)
            vsyncToggle.onValueChanged.AddListener(SetVSync);

        // Audio sliders
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        // Controls
        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);

        if (invertYToggle != null)
            invertYToggle.onValueChanged.AddListener(SetInvertY);
    }

    private void InitializeQualitySettings()
    {
        if (qualityDropdown == null) return;

        qualityDropdown.ClearOptions();

        List<string> qualityNames = new List<string>();
        string[] qualityLevels = QualitySettings.names;
        
        foreach (string quality in qualityLevels)
        {
            qualityNames.Add(quality);
        }

        qualityDropdown.AddOptions(qualityNames);
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();
    }

    private void InitializeResolutionSettings()
    {
        if (resolutionDropdown == null) return;

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> resolutionOptions = new List<string>();
        currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRateRatio.value.ToString("F0") + "Hz";
            resolutionOptions.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    #endregion

    #region Graphics Settings

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt(qualityKey, qualityIndex);
        PlayerPrefs.Save();
        Debug.Log($"[OptionsMenu] Quality set to: {QualitySettings.names[qualityIndex]}");
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        currentResolutionIndex = resolutionIndex;
        Debug.Log($"[OptionsMenu] Resolution set to: {resolution.width}x{resolution.height}");
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(fullscreenKey, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"[OptionsMenu] Fullscreen: {isFullscreen}");
    }

    public void SetVSync(bool enableVSync)
    {
        QualitySettings.vSyncCount = enableVSync ? 1 : 0;
        PlayerPrefs.SetInt(vsyncKey, enableVSync ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"[OptionsMenu] VSync: {enableVSync}");
    }

    #endregion

    #region Audio Settings

    public void SetMasterVolume(float volume)
    {
        // This will be integrated with AudioManager later
        AudioListener.volume = volume;
        
        PlayerPrefs.SetFloat(masterVolumeKey, volume);
        PlayerPrefs.Save();

        if (masterVolumeText != null)
        {
            masterVolumeText.text = Mathf.RoundToInt(volume * 100) + "%";
        }

        Debug.Log($"[OptionsMenu] Master Volume: {volume}");
    }

    public void SetMusicVolume(float volume)
    {
        // This will be integrated with AudioManager later
        PlayerPrefs.SetFloat(musicVolumeKey, volume);
        PlayerPrefs.Save();

        if (musicVolumeText != null)
        {
            musicVolumeText.text = Mathf.RoundToInt(volume * 100) + "%";
        }

        Debug.Log($"[OptionsMenu] Music Volume: {volume}");
    }

    public void SetSFXVolume(float volume)
    {
        // This will be integrated with AudioManager later
        PlayerPrefs.SetFloat(sfxVolumeKey, volume);
        PlayerPrefs.Save();

        if (sfxVolumeText != null)
        {
            sfxVolumeText.text = Mathf.RoundToInt(volume * 100) + "%";
        }

        Debug.Log($"[OptionsMenu] SFX Volume: {volume}");
    }

    #endregion

    #region Controls Settings

    public void SetMouseSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat(mouseSensKey, sensitivity);
        PlayerPrefs.Save();

        if (mouseSensitivityText != null)
        {
            mouseSensitivityText.text = sensitivity.ToString("F2");
        }

        Debug.Log($"[OptionsMenu] Mouse Sensitivity: {sensitivity}");
    }

    public void SetInvertY(bool invert)
    {
        PlayerPrefs.SetInt(invertYKey, invert ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"[OptionsMenu] Invert Y: {invert}");
    }

    #endregion

    #region Save/Load

    private void LoadSettings()
    {
        // Graphics
        if (qualityDropdown != null)
        {
            int quality = PlayerPrefs.GetInt(qualityKey, QualitySettings.GetQualityLevel());
            qualityDropdown.value = quality;
            QualitySettings.SetQualityLevel(quality);
        }

        if (fullscreenToggle != null)
        {
            bool fullscreen = PlayerPrefs.GetInt(fullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
            fullscreenToggle.isOn = fullscreen;
            Screen.fullScreen = fullscreen;
        }

        if (vsyncToggle != null)
        {
            bool vsync = PlayerPrefs.GetInt(vsyncKey, QualitySettings.vSyncCount) == 1;
            vsyncToggle.isOn = vsync;
            QualitySettings.vSyncCount = vsync ? 1 : 0;
        }

        // Audio
        if (masterVolumeSlider != null)
        {
            float masterVolume = PlayerPrefs.GetFloat(masterVolumeKey, 1f);
            masterVolumeSlider.value = masterVolume;
            AudioListener.volume = masterVolume;
            if (masterVolumeText != null)
                masterVolumeText.text = Mathf.RoundToInt(masterVolume * 100) + "%";
        }

        if (musicVolumeSlider != null)
        {
            float musicVolume = PlayerPrefs.GetFloat(musicVolumeKey, 0.7f);
            musicVolumeSlider.value = musicVolume;
            if (musicVolumeText != null)
                musicVolumeText.text = Mathf.RoundToInt(musicVolume * 100) + "%";
        }

        if (sfxVolumeSlider != null)
        {
            float sfxVolume = PlayerPrefs.GetFloat(sfxVolumeKey, 1f);
            sfxVolumeSlider.value = sfxVolume;
            if (sfxVolumeText != null)
                sfxVolumeText.text = Mathf.RoundToInt(sfxVolume * 100) + "%";
        }

        // Controls
        if (mouseSensitivitySlider != null)
        {
            float mouseSens = PlayerPrefs.GetFloat(mouseSensKey, 1f);
            mouseSensitivitySlider.value = mouseSens;
            if (mouseSensitivityText != null)
                mouseSensitivityText.text = mouseSens.ToString("F2");
        }

        if (invertYToggle != null)
        {
            bool invertY = PlayerPrefs.GetInt(invertYKey, 0) == 1;
            invertYToggle.isOn = invertY;
        }

        Debug.Log("[OptionsMenu] Settings loaded");
    }

    public void ResetToDefaults()
    {
        // Graphics
        if (qualityDropdown != null)
        {
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            SetQuality(qualityDropdown.value);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = true;
            SetFullscreen(true);
        }

        if (vsyncToggle != null)
        {
            vsyncToggle.isOn = true;
            SetVSync(true);
        }

        // Audio
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = 1f;
            SetMasterVolume(1f);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = 0.7f;
            SetMusicVolume(0.7f);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = 1f;
            SetSFXVolume(1f);
        }

        // Controls
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.value = 1f;
            SetMouseSensitivity(1f);
        }

        if (invertYToggle != null)
        {
            invertYToggle.isOn = false;
            SetInvertY(false);
        }

        Debug.Log("[OptionsMenu] Settings reset to defaults");
    }

    #endregion

    #region Public API

    public float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat(masterVolumeKey, 1f);
    }

    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat(musicVolumeKey, 0.7f);
    }

    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat(sfxVolumeKey, 1f);
    }

    public float GetMouseSensitivity()
    {
        return PlayerPrefs.GetFloat(mouseSensKey, 1f);
    }

    public bool GetInvertY()
    {
        return PlayerPrefs.GetInt(invertYKey, 0) == 1;
    }

    #endregion
}
