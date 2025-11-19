using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Reflection;

public class EcholocationDiagnostic : MonoBehaviour
{
    [Header("Diagnostic Settings")]
    [SerializeField] private bool runOnStart = true;
    [SerializeField] private bool detailedLogging = true;
    [SerializeField] private bool autoFixIssues = false;

    [Header("References (Auto-detected if null)")]
    [SerializeField] private EcholocationRenderFeature renderFeature;
    [SerializeField] private Material echolocationMaterial;
    [SerializeField] private EchoBase echoBase;

    [Header("Expected Values")]
    [SerializeField] private string expectedShaderName = "Custom/EcholocationIsometric";
    [SerializeField] private bool expectedTestMode = true;
    [SerializeField] private float expectedEchoRadius = 5f;

    private int issueCount = 0;

    private void Start()
    {
        if (runOnStart)
        {
            RunCompleteDiagnostic();
        }
    }

    [ContextMenu("Run Complete Diagnostic")]
    public void RunCompleteDiagnostic()
    {
        issueCount = 0;
        Debug.Log("=== ECHOLOCATION DIAGNOSTIC START ===");

        // Auto-detect missing references
        AutoDetectReferences();

        // Run checks in order of importance
        CheckURPSetup();
        CheckRenderFeature();
        CheckMaterialAndShader();
        CheckEchoBase();
        CheckCameraSetup();
        CheckForConflicts();

        // Summary
        LogSummary();
        Debug.Log("=== ECHOLOCATION DIAGNOSTIC END ===");
    }

    private void AutoDetectReferences()
    {
        if (detailedLogging) Debug.Log("--- AUTO-DETECTING REFERENCES ---");

        if (echoBase == null)
        {
            echoBase = FindFirstObjectByType<EchoBase>();
            if (echoBase != null && detailedLogging)
                Debug.Log($"Auto-detected EchoBase: {echoBase.name}");
        }

        if (renderFeature == null && detailedLogging)
        {
            Debug.Log("Render feature reference not set (this is normal - it's in URP settings)");
        }

        if (echolocationMaterial == null && detailedLogging)
        {
            Debug.LogWarning("Echolocation material not assigned - please assign it manually");
        }
    }

    private void CheckURPSetup()
    {
        Debug.Log("--- URP SETUP CHECK ---");

        var pipeline = GraphicsSettings.currentRenderPipeline;

        if (pipeline == null)
        {
            LogIssue("CRITICAL: No render pipeline set! Using Built-in Render Pipeline.");
            Debug.LogError("Go to Project Settings > Graphics and assign a URP asset to 'Scriptable Render Pipeline Settings'");
            return;
        }

        Debug.Log($"✓ Current render pipeline: {pipeline.name}");

        if (!(pipeline is UniversalRenderPipelineAsset urpAsset))
        {
            LogIssue("CRITICAL: Not using URP! Echolocation requires Universal Render Pipeline.");
            return;
        }

        Debug.Log($"✓ URP Asset: {urpAsset.name}");

        // Check renderer data using reflection (since m_RendererDataList is internal)
        CheckURPRenderers(urpAsset);
    }

    private void CheckURPRenderers(UniversalRenderPipelineAsset urpAsset)
    {
        try
        {
            // Use reflection to access internal renderer list
            var rendererDataListField = typeof(UniversalRenderPipelineAsset)
                .GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance);

            if (rendererDataListField != null)
            {
                var rendererDataList = rendererDataListField.GetValue(urpAsset) as ScriptableRendererData[];

                if (rendererDataList != null && rendererDataList.Length > 0)
                {
                    Debug.Log($"✓ Found {rendererDataList.Length} renderer(s)");

                    for (int i = 0; i < rendererDataList.Length; i++)
                    {
                        if (rendererDataList[i] != null)
                        {
                            Debug.Log($"  Renderer {i}: {rendererDataList[i].name}");
                            CheckRendererFeatures(rendererDataList[i]);
                        }
                    }
                }
                else
                {
                    LogIssue("No renderers found in URP asset!");
                }
            }
        }
        catch (System.Exception e)
        {
            if (detailedLogging)
                Debug.Log($"Could not check renderers via reflection: {e.Message}");
        }
    }

    private void CheckRendererFeatures(ScriptableRendererData rendererData)
    {
        try
        {
            var rendererFeaturesField = typeof(ScriptableRendererData)
                .GetField("m_RendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);

            if (rendererFeaturesField != null)
            {
                var features = rendererFeaturesField.GetValue(rendererData) as System.Collections.Generic.List<ScriptableRendererFeature>;

                if (features != null)
                {
                    bool echolocationFound = false;
                    foreach (var feature in features)
                    {
                        if (feature is EcholocationRenderFeature echoFeature)
                        {
                            echolocationFound = true;
                            Debug.Log($"  ✓ EcholocationRenderFeature found: {feature.name}");
                            renderFeature = echoFeature; // Cache reference
                            break;
                        }
                    }

                    if (!echolocationFound)
                    {
                        LogIssue($"EcholocationRenderFeature NOT found in {rendererData.name}!");
                        Debug.LogError("Add EcholocationRenderFeature to your Forward Renderer in URP settings");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            if (detailedLogging)
                Debug.Log($"Could not check renderer features: {e.Message}");
        }
    }

    private void CheckRenderFeature()
    {
        Debug.Log("--- RENDER FEATURE CHECK ---");

        if (renderFeature != null)
        {
            Debug.Log($"✓ Render feature reference: {renderFeature.name}");

            // Check if it has the DebugRenderFeature method
            try
            {
                renderFeature.DebugRenderFeature();
            }
            catch (System.Exception e)
            {
                LogIssue($"Error calling DebugRenderFeature: {e.Message}");
            }
        }
        else
        {
            LogIssue("Render feature not found or not assigned");
            Debug.LogError("Make sure EcholocationRenderFeature is added to your URP Forward Renderer");
        }
    }

    private void CheckMaterialAndShader()
    {
        Debug.Log("--- MATERIAL AND SHADER CHECK ---");

        if (echolocationMaterial == null)
        {
            LogIssue("CRITICAL: Echolocation material is NULL!");
            Debug.LogError("Create a material with the EcholocationIsometric shader and assign it");
            return;
        }

        Debug.Log($"✓ Material: {echolocationMaterial.name}");

        var shader = echolocationMaterial.shader;
        if (shader == null)
        {
            LogIssue("CRITICAL: Material shader is NULL!");
            return;
        }

        Debug.Log($"Material shader: {shader.name}");

        if (shader.name != expectedShaderName)
        {
            LogIssue($"Shader mismatch! Expected: {expectedShaderName}, Got: {shader.name}");
        }
        else
        {
            Debug.Log("✓ Correct shader assigned");
        }

        // Check essential shader properties
        CheckShaderProperties();

        // Check current material values
        CheckMaterialValues();
    }

    private void CheckShaderProperties()
    {
        string[] requiredProperties = {
            "_TestMode", "_BlackoutColor", "_EchoRadius", "_RevealIntensity",
            "_EchoColor", "_RevealedColor", "_MainTex"
        };

        foreach (string property in requiredProperties)
        {
            if (echolocationMaterial.HasProperty(property))
            {
                if (detailedLogging) Debug.Log($"  ✓ Has {property}");
            }
            else
            {
                LogIssue($"Missing shader property: {property}");
            }
        }
    }

    private void CheckMaterialValues()
    {
        if (!echolocationMaterial.HasProperty("_TestMode")) return;

        float testMode = echolocationMaterial.GetFloat("_TestMode");
        float echoRadius = echolocationMaterial.HasProperty("_EchoRadius") ?
            echolocationMaterial.GetFloat("_EchoRadius") : 0f;

        Debug.Log($"Current _TestMode: {testMode} (Expected: {(expectedTestMode ? 1 : 0)})");
        Debug.Log($"Current _EchoRadius: {echoRadius} (Expected: {expectedEchoRadius})");

        if (autoFixIssues)
        {
            bool needsUpdate = false;

            if (testMode != (expectedTestMode ? 1f : 0f))
            {
                echolocationMaterial.SetFloat("_TestMode", expectedTestMode ? 1f : 0f);
                needsUpdate = true;
            }

            if (Mathf.Abs(echoRadius - expectedEchoRadius) > 0.1f)
            {
                echolocationMaterial.SetFloat("_EchoRadius", expectedEchoRadius);
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                Debug.Log("✓ Auto-fixed material values");
            }
        }
    }

    private void CheckEchoBase()
    {
        Debug.Log("--- ECHOBASE CHECK ---");

        if (echoBase == null)
        {
            Debug.LogWarning("EchoBase not found - system can work in test mode only");
            return;
        }

        Debug.Log($"✓ EchoBase: {echoBase.name} at {echoBase.transform.position}");
        Debug.Log($"  Echo radius: {echoBase.EchoRadius}");
        Debug.Log($"  Active waves: {echoBase.ActiveWaveCount}");

        // Test echo triggering
        int wavesBefore = echoBase.ActiveWaveCount;
        echoBase.TriggerEcho();

        // Check after a frame
        StartCoroutine(CheckEchoAfterFrame(wavesBefore));
    }

    private System.Collections.IEnumerator CheckEchoAfterFrame(int wavesBefore)
    {
        yield return null; // Wait one frame

        if (echoBase != null)
        {
            int wavesAfter = echoBase.ActiveWaveCount;
            if (wavesAfter > wavesBefore)
            {
                Debug.Log("✓ Echo triggering works correctly");
            }
            else
            {
                LogIssue("Echo triggering might not be working");
            }
        }
    }

    private void CheckCameraSetup()
    {
        Debug.Log("--- CAMERA SETUP CHECK ---");

        var mainCamera = Camera.main;
        if (mainCamera == null)
        {
            LogIssue("CRITICAL: No main camera found!");
            return;
        }

        Debug.Log($"✓ Main camera: {mainCamera.name}");
        Debug.Log($"  Enabled: {mainCamera.enabled}");
        Debug.Log($"  Orthographic: {mainCamera.orthographic}");

        if (mainCamera.orthographic)
        {
            Debug.Log($"  Orthographic size: {mainCamera.orthographicSize}");
        }

        // Check for Cinemachine
        var cinemachineBrain = mainCamera.GetComponent<Unity.Cinemachine.CinemachineBrain>();
        if (cinemachineBrain != null)
        {
            Debug.Log("✓ Cinemachine Brain detected");
        }

        // Check for old EcholocationEffect
        var oldEffect = mainCamera.GetComponent<EcholocationEffect>();
        if (oldEffect != null)
        {
            LogIssue("Old EcholocationEffect found on camera - should be disabled for URP");

            if (autoFixIssues)
            {
                oldEffect.enabled = false;
                Debug.Log("✓ Auto-disabled old EcholocationEffect");
            }
        }
    }

    private void CheckForConflicts()
    {
        Debug.Log("--- CONFLICT CHECK ---");

        // Check for multiple EchoBases
        var allEchoBases = FindObjectsByType<EchoBase>(FindObjectsSortMode.None);
        if (allEchoBases.Length > 1)
        {
            LogIssue($"Multiple EchoBases found ({allEchoBases.Length}) - only one should exist");
        }

        // Check for multiple diagnostic scripts
        var allDiagnostics = FindObjectsByType<EcholocationDiagnostic>(FindObjectsSortMode.None);
        if (allDiagnostics.Length > 1)
        {
            Debug.LogWarning($"Multiple diagnostic scripts found ({allDiagnostics.Length}) - this is usually unnecessary");
        }
    }

    private void LogSummary()
    {
        Debug.Log("--- DIAGNOSTIC SUMMARY ---");

        if (issueCount == 0)
        {
            Debug.Log("✓ <color=green>NO ISSUES FOUND! Echolocation should work correctly.</color>");
        }
        else
        {
            Debug.Log($"<color=red>Found {issueCount} issue(s) that need attention.</color>");

            if (!autoFixIssues)
            {
                Debug.Log("Enable 'Auto Fix Issues' to automatically resolve some problems.");
            }
        }

        // Quick test recommendation
        Debug.Log("\n<color=yellow>QUICK TEST:</color>");
        Debug.Log("1. Set Force Test Mode = true in render feature");
        Debug.Log("2. Play the game");
        Debug.Log("3. You should see a black screen with circular reveal in center");
    }

    private void LogIssue(string issue)
    {
        issueCount++;
        Debug.LogError($"ISSUE #{issueCount}: {issue}");
    }

    [ContextMenu("Quick Fix Common Issues")]
    public void QuickFixCommonIssues()
    {
        Debug.Log("--- RUNNING QUICK FIXES ---");

        bool originalAutoFix = autoFixIssues;
        autoFixIssues = true;

        RunCompleteDiagnostic();

        autoFixIssues = originalAutoFix;

        Debug.Log("Quick fixes completed!");
    }

    [ContextMenu("Test Echo Triggering")]
    public void TestEchoTriggering()
    {
        if (echoBase != null)
        {
            Debug.Log("Testing echo triggering...");
            echoBase.TriggerEcho();
            Debug.Log($"Echo triggered! Active waves: {echoBase.ActiveWaveCount}");
        }
        else
        {
            Debug.LogWarning("No EchoBase found to test");
        }
    }

    // Helper method to find URP asset in project
    [ContextMenu("Find URP Assets")]
    public void FindURPAssets()
    {
        Debug.Log("--- SEARCHING FOR URP ASSETS ---");

        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");

        if (guids.Length == 0)
        {
            Debug.LogWarning("No URP assets found in project!");
        }
        else
        {
            Debug.Log($"Found {guids.Length} URP asset(s):");
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
                Debug.Log($"  {asset.name} at {path}");
            }
        }
    }
}