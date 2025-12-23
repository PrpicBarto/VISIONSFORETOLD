using UnityEngine;

/// <summary>
/// Black Metal post-processing effect
/// Applies high contrast, grain, and desaturation for a "Transylvanian Hunger" aesthetic
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class BlackMetalPostProcess : MonoBehaviour
{
    [Header("Black Metal Style Settings")]
    [Tooltip("Overall intensity of the effect (0 = normal, 1 = full black metal)")]
    [Range(0f, 1f)]
    public float effectIntensity = 0.8f;

    [Header("Contrast & Brightness")]
    [Tooltip("Contrast level (higher = more harsh black/white)")]
    [Range(0f, 3f)]
    public float contrast = 1.8f;
    
    [Tooltip("Brightness adjustment")]
    [Range(-1f, 1f)]
    public float brightness = -0.2f;

    [Header("Desaturation")]
    [Tooltip("How much to desaturate (0 = color, 1 = black/white)")]
    [Range(0f, 1f)]
    public float desaturation = 0.85f;

    [Header("Film Grain")]
    [Tooltip("Enable film grain/noise")]
    public bool enableGrain = true;
    
    [Tooltip("Grain intensity")]
    [Range(0f, 1f)]
    public float grainIntensity = 0.15f;
    
    [Tooltip("Grain size")]
    [Range(0.5f, 4f)]
    public float grainSize = 1.5f;

    [Header("Vignette")]
    [Tooltip("Enable dark edges vignette")]
    public bool enableVignette = true;
    
    [Tooltip("Vignette intensity")]
    [Range(0f, 1f)]
    public float vignetteIntensity = 0.4f;

    [Header("Scan Lines (optional)")]
    [Tooltip("Enable scan lines for extra grimness")]
    public bool enableScanLines = false;
    
    [Tooltip("Scan line density")]
    [Range(100f, 1000f)]
    public float scanLineDensity = 400f;

    private Material material;
    private Shader shader;

    void Awake()
    {
        shader = Shader.Find("Hidden/BlackMetalPostProcess");
        if (shader == null)
        {
            Debug.LogError("BlackMetalPostProcess shader not found! Make sure the shader is in Resources or has the correct name.");
            enabled = false;
            return;
        }
        
        material = new Material(shader);
        material.hideFlags = HideFlags.HideAndDontSave;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material == null || effectIntensity <= 0f)
        {
            Graphics.Blit(source, destination);
            return;
        }

        // Pass parameters to shader
        material.SetFloat("_EffectIntensity", effectIntensity);
        material.SetFloat("_Contrast", contrast);
        material.SetFloat("_Brightness", brightness);
        material.SetFloat("_Desaturation", desaturation);
        material.SetFloat("_GrainIntensity", enableGrain ? grainIntensity : 0f);
        material.SetFloat("_GrainSize", grainSize);
        material.SetFloat("_VignetteIntensity", enableVignette ? vignetteIntensity : 0f);
        material.SetFloat("_ScanLines", enableScanLines ? scanLineDensity : 0f);
        material.SetFloat("_CustomTime", Time.time);

        Graphics.Blit(source, destination, material);
    }

    void OnDestroy()
    {
        if (material != null)
        {
            DestroyImmediate(material);
        }
    }
}
