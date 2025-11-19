using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
public class EcholocationEffect : MonoBehaviour
{
    [Header("Echolocation Effect")]
    [SerializeField] private Material echolocationMaterial;
    [SerializeField] private bool enableEffect = true;

    [Header("URP Integration")]
    [SerializeField] private bool useURPRenderFeature = true;
    [SerializeField] private string renderFeatureInfo = "Render feature must be added to URP Forward Renderer manually";

    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = true;
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool forceTestMode = false;

    [Header("Cinemachine Detection")]
    [SerializeField] private bool autoDetectCinemachine = true;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private CinemachineCamera activeVirtualCamera;

    [Header("Echo Settings")]
    [SerializeField] private float echoRadius = 10f;
    [SerializeField] private Color echoColor = Color.cyan;
    [SerializeField] private bool autoTriggerEchoes = true;

    private Camera targetCamera;
    private EchoBase echoBase;
    private int frameCount = 0;
    private bool isCinemachineSetup = false;
    private bool isURPDetected = false;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
        echoBase = FindFirstObjectByType<EchoBase>();

        // Detect render pipeline
        DetectRenderPipeline();

        // Detect Cinemachine setup
        DetectCinemachineSetup();

        if (showDebugLogs)
        {
            Debug.Log($"=== ECHOLOCATION EFFECT SETUP ===");
            Debug.Log($"Camera: {targetCamera?.name}");
            Debug.Log($"URP Detected: {isURPDetected}");
            Debug.Log($"Cinemachine Setup: {isCinemachineSetup}");
            Debug.Log($"EchoBase: {echoBase?.name}");
            Debug.Log($"Material: {echolocationMaterial?.name}");
            Debug.Log($"Use URP Render Feature: {useURPRenderFeature}");
        }
    }

    private void DetectRenderPipeline()
    {
        var pipeline = GraphicsSettings.currentRenderPipeline;
        isURPDetected = pipeline is UniversalRenderPipelineAsset;

        if (showDebugLogs)
        {
            Debug.Log($"Render Pipeline: {(pipeline ? pipeline.name : "Built-in")}");
            Debug.Log($"Is URP: {isURPDetected}");
        }

        if (!isURPDetected && useURPRenderFeature)
        {
            Debug.LogWarning("URP Render Feature requested but URP not detected! Falling back to legacy mode.");
            useURPRenderFeature = false;
        }
    }

    private void DetectCinemachineSetup()
    {
        cinemachineBrain = GetComponent<CinemachineBrain>();

        if (cinemachineBrain == null && autoDetectCinemachine)
        {
            cinemachineBrain = FindFirstObjectByType<CinemachineBrain>();
        }

        if (cinemachineBrain != null)
        {
            isCinemachineSetup = true;

            if (activeVirtualCamera == null && autoDetectCinemachine)
            {
                activeVirtualCamera = FindFirstObjectByType<CinemachineCamera>();
            }

            if (showDebugLogs)
            {
                Debug.Log($"Cinemachine detected - Brain: {cinemachineBrain.name}, Virtual Camera: {activeVirtualCamera?.name}");
            }
        }
    }

    private void Start()
    {
        if (useURPRenderFeature)
        {
            SetupURPIntegration();
            ForceSetupMaterial(); // Add this line
        }
        else
        {
            SetupLegacyMode();
        }

        // Setup EchoBase integration
        SetupEchoBaseIntegration();
    }

    private void SetupURPIntegration()
    {
        if (!isURPDetected)
        {
            Debug.LogError("Cannot use URP Render Feature - URP not detected!");
            return;
        }

        // With URP Render Feature, this script becomes a data provider
        // The actual rendering is handled by EcholocationRenderFeature
        if (showDebugLogs)
        {
            Debug.Log("✓ URP Integration enabled");
            Debug.Log("Make sure EcholocationRenderFeature is added to your URP Forward Renderer");
            Debug.Log("The render feature will automatically find EchoBase and this material");
        }

        // Disable OnRenderImage since URP render feature handles everything
        enabled = true; // Keep script enabled for data management
    }

    private void SetupLegacyMode()
    {
        if (isURPDetected)
        {
            Debug.LogWarning("Using legacy OnRenderImage mode with URP - this may not work properly!");
            Debug.LogWarning("Consider using the EcholocationRenderFeature instead.");
        }
        else
        {
            Debug.Log("Legacy mode enabled for Built-in Render Pipeline");
        }
    }

    private void SetupEchoBaseIntegration()
    {
        if (echoBase == null)
        {
            Debug.LogWarning("No EchoBase found - creating basic echo functionality");
            return;
        }

        if (showDebugLogs)
        {
            Debug.Log($"✓ EchoBase integration setup: {echoBase.name}");
        }

        // Sync settings with EchoBase
        if (echoBase.EchoRadius != echoRadius)
        {
            echoBase.SetEchoRadius(echoRadius);
        }
    }

    private void Update()
    {
        frameCount++;

        // Debug controls
        HandleDebugInput();

        // Update echo base if needed
        UpdateEchoBaseSettings();

        // Provide helpful reminders
        if (useURPRenderFeature && frameCount == 300) // After 5 seconds
        {
            ProvideSetupReminders();
        }
    }

    private void HandleDebugInput()
    {
        if (!debugMode) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) ToggleEffect();
        if (Input.GetKeyDown(KeyCode.Alpha2)) TriggerManualEcho();
        if (Input.GetKeyDown(KeyCode.Alpha3)) DebugEcholocationState();
        if (Input.GetKeyDown(KeyCode.Alpha4)) DebugMaterial();
        if (Input.GetKeyDown(KeyCode.Alpha5)) DebugCinemachine();
        if (Input.GetKeyDown(KeyCode.E)) TriggerEcho();
        if (Input.GetKeyDown(KeyCode.T)) ToggleTestMode();
        if (Input.GetKeyDown(KeyCode.Alpha6)) ShowURPSetupInstructions();
    }

    private void UpdateEchoBaseSettings()
    {
        if (echoBase == null) return;

        // Sync settings periodically
        if (frameCount % 60 == 0) // Once per second
        {
            if (Mathf.Abs(echoBase.EchoRadius - echoRadius) > 0.1f)
            {
                echoBase.SetEchoRadius(echoRadius);
            }
        }
    }

    private void ProvideSetupReminders()
    {
        if (!useURPRenderFeature) return;

        Debug.Log("=== URP SETUP REMINDER ===");
        Debug.Log("If the echolocation effect is not visible:");
        Debug.Log("1. Go to Project Settings > Graphics");
        Debug.Log("2. Click on your URP asset");
        Debug.Log("3. Click on your Forward Renderer");
        Debug.Log("4. Add 'EcholocationRenderFeature' to Renderer Features");
        Debug.Log("5. Assign your echolocation material to the render feature");
        Debug.Log("6. Enable Force Test Mode and Always Log for testing");
    }

    // Replace your OnRenderImage method with this enhanced version:
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        frameCount++;
        
        if (useURPRenderFeature)
        {
            // URP render feature should handle this
            Graphics.Blit(source, destination);
            
            // But if render feature isn't working, provide feedback
            if (frameCount == 60) // After 1 second
            {
                Debug.LogWarning("URP Render Feature should be handling rendering. If you don't see effects, check URP setup.");
            }
            return;
        }

        // EMERGENCY FALLBACK - Force manual rendering for testing
        if (Input.GetKey(KeyCode.F1)) // Hold F1 to force manual test
        {
            if (echolocationMaterial != null)
            {
                ForceSetupMaterial();
                UpdateMaterialProperties();
                Graphics.Blit(source, destination, echolocationMaterial);
                
                if (frameCount % 60 == 0)
                {
                    Debug.Log("MANUAL FALLBACK: Rendering echolocation effect directly (Hold F1)");
                }
            }
            else
            {
                Graphics.Blit(source, destination);
                Debug.LogError("Cannot render - no material assigned!");
            }
            return;
        }

        // Normal legacy mode
        if (enableEffect && echolocationMaterial != null)
        {
            UpdateMaterialProperties();
            Graphics.Blit(source, destination, echolocationMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    private void UpdateMaterialProperties()
    {
        if (echolocationMaterial == null) return;

        // Update material with current settings
        if (echoBase != null)
        {
            echoBase.UpdateEcholocationMaterial(echolocationMaterial);
        }
        else
        {
            // Fallback values
            SetFallbackMaterialProperties();
        }

        // Set camera properties
        SetCameraProperties();
    }

    private void SetFallbackMaterialProperties()
    {
        if (echolocationMaterial.HasProperty("_TestMode"))
            echolocationMaterial.SetFloat("_TestMode", forceTestMode ? 1f : 0f);

        if (echolocationMaterial.HasProperty("_BlackoutColor"))
            echolocationMaterial.SetColor("_BlackoutColor", Color.black);

        if (echolocationMaterial.HasProperty("_EchoColor"))
            echolocationMaterial.SetColor("_EchoColor", echoColor);

        if (echolocationMaterial.HasProperty("_EchoRadius"))
            echolocationMaterial.SetFloat("_EchoRadius", echoRadius);

        if (echolocationMaterial.HasProperty("_RevealIntensity"))
            echolocationMaterial.SetFloat("_RevealIntensity", 0.3f);
    }

    private void SetCameraProperties()
    {
        if (targetCamera == null || echolocationMaterial == null) return;

        if (echolocationMaterial.HasProperty("_CameraWorldPos"))
            echolocationMaterial.SetVector("_CameraWorldPos", targetCamera.transform.position);

        if (echolocationMaterial.HasProperty("_IsOrthographic"))
            echolocationMaterial.SetFloat("_IsOrthographic", targetCamera.orthographic ? 1f : 0f);

        if (echolocationMaterial.HasProperty("_OrthographicSize"))
            echolocationMaterial.SetFloat("_OrthographicSize", targetCamera.orthographicSize);
    }

    // Public API
    public void ToggleEffect()
    {
        enableEffect = !enableEffect;
        Debug.Log($"Echolocation effect: {(enableEffect ? "Enabled" : "Disabled")}");

        if (useURPRenderFeature)
        {
            Debug.Log("Note: Effect state is controlled by the URP Render Feature settings");
        }
    }

    public void TriggerEcho()
    {
        if (echoBase != null)
        {
            echoBase.TriggerEcho();
            Debug.Log("Manual echo triggered");
        }
        else
        {
            Debug.LogWarning("No EchoBase available to trigger echo");
        }
    }

    public void SetEchoRadius(float newRadius)
    {
        echoRadius = Mathf.Max(0.1f, newRadius);
        if (echoBase != null)
        {
            echoBase.SetEchoRadius(echoRadius);
        }
        Debug.Log($"Echo radius set to: {echoRadius}");
    }

    public void SetEchoColor(Color newColor)
    {
        echoColor = newColor;
        if (echolocationMaterial != null && echolocationMaterial.HasProperty("_EchoColor"))
        {
            echolocationMaterial.SetColor("_EchoColor", echoColor);
        }
        Debug.Log($"Echo color set to: {echoColor}");
    }

    // Debug methods
    private void TriggerManualEcho()
    {
        TriggerEcho();
    }

    private void ToggleTestMode()
    {
        forceTestMode = !forceTestMode;
        Debug.Log($"Test mode: {(forceTestMode ? "Enabled" : "Disabled")}");

        if (useURPRenderFeature)
        {
            Debug.Log("Test mode is controlled by the URP Render Feature Force Test Mode setting");
        }
    }

    private void ShowURPSetupInstructions()
    {
        Debug.Log("=== URP RENDER FEATURE SETUP ===");
        Debug.Log("1. Open Project Settings (Edit > Project Settings)");
        Debug.Log("2. Go to Graphics section");
        Debug.Log("3. Click on your URP asset (Scriptable Render Pipeline Settings)");
        Debug.Log("4. Find 'Renderer List' and click on your Forward Renderer");
        Debug.Log("5. Scroll to 'Renderer Features' and click the + button");
        Debug.Log("6. Select 'EcholocationRenderFeature'");
        Debug.Log("7. Assign your echolocation material to the render feature");
        Debug.Log("8. Set Force Test Mode = true and Always Log = true for testing");
        Debug.Log("9. Play the game - you should see debug messages if it's working");
    }

    private void DebugEcholocationState()
    {
        Debug.Log("=== ECHOLOCATION STATE ===");
        Debug.Log($"Effect enabled: {enableEffect}");
        Debug.Log($"URP mode: {useURPRenderFeature}");
        Debug.Log($"URP detected: {isURPDetected}");
        Debug.Log($"Cinemachine: {isCinemachineSetup}");
        Debug.Log($"Echo radius: {echoRadius}");
        Debug.Log($"Echo color: {echoColor}");
        Debug.Log($"EchoBase active waves: {(echoBase ? echoBase.ActiveWaveCount.ToString() : "N/A")}");

        if (useURPRenderFeature)
        {
            Debug.Log("IMPORTANT: Rendering is handled by URP Render Feature");
            Debug.Log("Check URP Forward Renderer settings if effect not visible");
        }
    }

    private void DebugMaterial()
    {
        Debug.Log("=== MATERIAL DEBUG ===");
        if (echolocationMaterial != null)
        {
            Debug.Log($"Material: {echolocationMaterial.name}");
            Debug.Log($"Shader: {echolocationMaterial.shader?.name}");
            Debug.Log($"Has _TestMode: {echolocationMaterial.HasProperty("_TestMode")}");
            Debug.Log($"Has _EchoRadius: {echolocationMaterial.HasProperty("_EchoRadius")}");
            Debug.Log($"Has _BlackoutColor: {echolocationMaterial.HasProperty("_BlackoutColor")}");

            if (useURPRenderFeature)
            {
                Debug.Log("This material should be assigned to the URP Render Feature");
            }
        }
        else
        {
            Debug.LogError("No echolocation material assigned!");
        }
    }

    private void DebugCinemachine()
    {
        Debug.Log("=== CINEMACHINE DEBUG ===");
        Debug.Log($"Brain found: {cinemachineBrain != null}");
        Debug.Log($"Virtual camera: {activeVirtualCamera?.name}");
        Debug.Log($"Setup complete: {isCinemachineSetup}");
    }

    // Context menu methods for inspector testing
    [ContextMenu("Trigger Echo")]
    public void TriggerEchoFromMenu()
    {
        TriggerEcho();
    }

    [ContextMenu("Toggle Effect")]
    public void ToggleEffectFromMenu()
    {
        ToggleEffect();
    }

    [ContextMenu("Debug State")]
    public void DebugStateFromMenu()
    {
        DebugEcholocationState();
    }

    [ContextMenu("Show URP Setup Instructions")]
    public void ShowURPSetupInstructionsFromMenu()
    {
        ShowURPSetupInstructions();
    }

    [ContextMenu("Force URP Debug Test")]
    public void ForceURPDebugTest()
    {
        Debug.Log("=== FORCE URP DEBUG TEST ===");
        Debug.Log($"URP Detected: {isURPDetected}");
        Debug.Log($"Use URP Render Feature: {useURPRenderFeature}");
        Debug.Log($"Material assigned: {echolocationMaterial != null}");
        Debug.Log($"Material name: {echolocationMaterial?.name}");
        Debug.Log($"Shader name: {echolocationMaterial?.shader?.name}");
        
        // Test EchoBase
        if (echoBase != null)
        {
            Debug.Log($"EchoBase active waves: {echoBase.ActiveWaveCount}");
            echoBase.TriggerEcho();
            Debug.Log("Manual echo triggered via debug test");
        }
        
        Debug.Log("If you don't see render feature execution messages, the URP setup is wrong");
    }

    private void OnValidate()
    {
        // Clamp values
        echoRadius = Mathf.Max(0.1f, echoRadius);

        // Update EchoBase if in play mode
        if (Application.isPlaying && echoBase != null)
        {
            echoBase.SetEchoRadius(echoRadius);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw echo radius
        Gizmos.color = echoColor;
        Gizmos.DrawWireSphere(transform.position, echoRadius);

        // Draw camera info
        if (targetCamera != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 pos = targetCamera.transform.position;
            Gizmos.DrawWireCube(pos, Vector3.one * 0.5f);
        }
    }

    // Add this method to EcholocationEffect.cs
    private void ForceSetupMaterial()
    {
        if (echolocationMaterial == null)
        {
            Debug.LogError("CRITICAL: No echolocation material assigned to EcholocationEffect!");
            return;
        }

        if (echolocationMaterial.shader == null)
        {
            Debug.LogError("CRITICAL: Material has no shader assigned!");
            return;
        }

        if (echolocationMaterial.shader.name != "Custom/EcholocationIsometric")
        {
            Debug.LogWarning($"Shader mismatch: Expected 'Custom/EcholocationIsometric', got '{echolocationMaterial.shader.name}'");
        }

        // Force set essential properties
        if (echolocationMaterial.HasProperty("_TestMode"))
            echolocationMaterial.SetFloat("_TestMode", 1.0f); // Force test mode

        if (echolocationMaterial.HasProperty("_BlackoutColor"))
            echolocationMaterial.SetColor("_BlackoutColor", Color.black);

        if (echolocationMaterial.HasProperty("_EchoRadius"))
            echolocationMaterial.SetFloat("_EchoRadius", 5f);

        if (echolocationMaterial.HasProperty("_RevealIntensity"))
            echolocationMaterial.SetFloat("_RevealIntensity", 0.5f);

        Debug.Log("Material properties force-updated for testing");
    }
}