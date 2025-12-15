using UnityEngine;

/// <summary>
/// Runtime controller for Black Metal post-processing effect
/// Allows dynamic adjustment of the black metal aesthetic during gameplay
/// </summary>
[ExecuteAlways]
public class BlackMetalController : MonoBehaviour
{
    [Header("Presets")]
    [SerializeField] private BlackMetalPreset preset = BlackMetalPreset.Classic;
    [SerializeField] private bool applyPresetOnStart = true;
    
    [Header("Runtime Control")]
    [SerializeField] private bool allowRuntimeAdjustment = true;
    [Range(0f, 1f)] [SerializeField] private float effectIntensity = 1f;
    
    [Header("Dynamic Intensity")]
    [SerializeField] private bool useHealthBasedIntensity = false;
    [SerializeField] private AnimationCurve intensityCurve = AnimationCurve.Linear(0, 0.5f, 1, 1);
    
    private BlackMetalRenderFeature renderFeature;
    private BlackMetalRenderFeature.Settings originalSettings;
    private Health playerHealth;

    public enum BlackMetalPreset
    {
        Classic,        // Traditional black metal album aesthetic
        Atmospheric,    // More fog, less grain
        Raw,            // Maximum grain, high contrast
        Depressive,     // Very dark, heavy vignette
        Custom          // User-defined settings
    }

    private void Start()
    {
        if (applyPresetOnStart)
        {
            ApplyPreset(preset);
        }
        
        if (useHealthBasedIntensity)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
            }
        }
    }

    private void Update()
    {
        if (useHealthBasedIntensity && playerHealth != null)
        {
            float healthPercent = playerHealth.HealthPercentage;
            effectIntensity = intensityCurve.Evaluate(healthPercent);
        }
    }

    public void ApplyPreset(BlackMetalPreset presetType)
    {
        // Note: This requires manual setup in URP renderer settings
        // The actual values would be set on the BlackMetalRenderFeature in the renderer
        
        switch (presetType)
        {
            case BlackMetalPreset.Classic:
                ApplyClassicPreset();
                break;
            case BlackMetalPreset.Atmospheric:
                ApplyAtmosphericPreset();
                break;
            case BlackMetalPreset.Raw:
                ApplyRawPreset();
                break;
            case BlackMetalPreset.Depressive:
                ApplyDepressivePreset();
                break;
        }
        
        Debug.Log($"[BlackMetal] Applied preset: {presetType}");
    }

    private void ApplyClassicPreset()
    {
        // Classic black metal album cover aesthetic
        // High contrast, moderate grain, strong vignette
    }

    private void ApplyAtmosphericPreset()
    {
        // Atmospheric black metal
        // More fog, softer grain, ethereal feel
    }

    private void ApplyRawPreset()
    {
        // Raw/lo-fi black metal
        // Maximum grain, very high contrast, sharp edges
    }

    private void ApplyDepressivePreset()
    {
        // Depressive/suicidal black metal
        // Very dark, heavy vignette, desaturated
    }

    #region Public API

    /// <summary>
    /// Set the overall effect intensity
    /// </summary>
    public void SetIntensity(float intensity)
    {
        effectIntensity = Mathf.Clamp01(intensity);
    }

    /// <summary>
    /// Enable or disable the effect
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        effectIntensity = enabled ? 1f : 0f;
    }

    /// <summary>
    /// Switch between presets at runtime
    /// </summary>
    public void SwitchPreset(BlackMetalPreset newPreset)
    {
        preset = newPreset;
        ApplyPreset(newPreset);
    }

    #endregion
}
