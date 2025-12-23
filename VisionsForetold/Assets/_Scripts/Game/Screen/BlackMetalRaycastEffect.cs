using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Black Metal Raycast Post-Processing Effect
/// Renders everything as white with black outlines - TRVE KVLT style
/// This is a standalone fullscreen effect that works on the entire scene
/// Compatible with Unity 6 (6000.2.7f2) and URP
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class BlackMetalRaycastEffect : MonoBehaviour
{
    [Header("Black Metal Raycast Settings")]
    [Tooltip("Enable/disable the effect")]
    [SerializeField] private bool enableEffect = true;

    [Header("Outline Settings")]
    [Tooltip("Thickness of black outlines (0-5)")]
    [SerializeField] [Range(0f, 5f)] private float outlineThickness = 1.0f;
    
    [Tooltip("Sensitivity for depth-based edges (lower = more edges)")]
    [SerializeField] [Range(0f, 1f)] private float depthThreshold = 0.1f;
    
    [Tooltip("Sensitivity for normal-based edges (lower = more edges)")]
    [SerializeField] [Range(0f, 1f)] private float normalThreshold = 0.4f;

    [Header("Appearance")]
    [Tooltip("Brightness of white surfaces")]
    [SerializeField] [Range(0f, 2f)] private float brightness = 1.0f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    private Material material;
    private Shader shader;
    private Camera cam;
    private UniversalAdditionalCameraData cameraData;

    void Awake()
    {
        cam = GetComponent<Camera>();
        
        // Get URP camera data
        cameraData = cam.GetUniversalAdditionalCameraData();
        
        // Enable depth and normal textures for URP
        if (cameraData != null)
        {
            cameraData.requiresDepthTexture = true;
            cameraData.requiresColorTexture = false;
        }
        
        // Also set legacy depth mode for compatibility
        cam.depthTextureMode = DepthTextureMode.Depth | DepthTextureMode.DepthNormals;
        
        shader = Shader.Find("Hidden/BlackMetalRaycast");
        if (shader == null)
        {
            Debug.LogError("[BlackMetalRaycastEffect] Shader 'Hidden/BlackMetalRaycast' not found! Make sure it exists in Assets/Shaders/");
            enabled = false;
            return;
        }
        
        material = new Material(shader);
        material.hideFlags = HideFlags.HideAndDontSave;
        
        if (showDebugInfo)
        {
            Debug.Log($"[BlackMetalRaycastEffect] Initialized successfully on Unity {Application.unityVersion}");
            Debug.Log($"[BlackMetalRaycastEffect] URP Camera Data: {(cameraData != null ? "Found" : "Not Found")}");
        }
    }

    void OnEnable()
    {
        if (cam == null)
            cam = GetComponent<Camera>();
        
        if (cameraData == null)
            cameraData = cam.GetUniversalAdditionalCameraData();
            
        // Ensure depth and normal textures are enabled
        if (cameraData != null)
        {
            cameraData.requiresDepthTexture = true;
        }
        
        cam.depthTextureMode = DepthTextureMode.Depth | DepthTextureMode.DepthNormals;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material == null || !enableEffect)
        {
            Graphics.Blit(source, destination);
            return;
        }

        // Verify depth texture is available
        if (Shader.GetGlobalTexture("_CameraDepthTexture") == null && showDebugInfo)
        {
            Debug.LogWarning("[BlackMetalRaycastEffect] Depth texture not available! Make sure URP asset has Depth Texture enabled.");
        }

        // Pass parameters to shader
        material.SetFloat("_OutlineThickness", outlineThickness);
        material.SetFloat("_DepthThreshold", depthThreshold);
        material.SetFloat("_NormalThreshold", normalThreshold);
        material.SetFloat("_Brightness", brightness);

        // Apply the effect
        Graphics.Blit(source, destination, material);
    }

    void OnDestroy()
    {
        if (material != null)
        {
            DestroyImmediate(material);
        }
    }

    void OnValidate()
    {
        // Clamp values
        outlineThickness = Mathf.Clamp(outlineThickness, 0f, 5f);
        depthThreshold = Mathf.Clamp01(depthThreshold);
        normalThreshold = Mathf.Clamp01(normalThreshold);
        brightness = Mathf.Clamp(brightness, 0f, 2f);
    }
}
