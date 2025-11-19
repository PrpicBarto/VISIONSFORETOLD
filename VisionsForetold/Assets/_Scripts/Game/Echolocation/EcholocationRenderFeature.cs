using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EcholocationRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class EcholocationSettings
    {
        [Header("Echolocation Settings")]
        public Material echolocationMaterial = null;
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        [Header("Debug")]
        public bool enableDebugLogs = false;
        public bool forceTestMode = true;
        public bool alwaysLog = true; // NEW: Always log regardless of enableDebugLogs
    }

    public EcholocationSettings settings = new EcholocationSettings();
    private EcholocationRenderPass echolocationPass;

    public override void Create()
    {
        if (settings.alwaysLog)
            Debug.Log("*** EcholocationRenderFeature: Create() called ***");

        echolocationPass = new EcholocationRenderPass(settings);
        echolocationPass.renderPassEvent = settings.renderPassEvent;

        if (settings.alwaysLog)
            Debug.Log($"*** EcholocationRenderFeature: Created with material: {settings.echolocationMaterial?.name} ***");
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.alwaysLog)
            Debug.Log("*** EcholocationRenderFeature: AddRenderPasses() called ***");

        if (settings.echolocationMaterial == null)
        {
            Debug.LogError("EcholocationRenderFeature: Material is NULL!");
            return;
        }

        if (settings.alwaysLog)
            Debug.Log("*** EcholocationRenderFeature: Enqueueing render pass ***");

        // Setup and enqueue the render pass
        echolocationPass.ConfigureInput(ScriptableRenderPassInput.Color);
        renderer.EnqueuePass(echolocationPass);
    }

    protected override void Dispose(bool disposing)
    {
        if (settings.alwaysLog)
            Debug.Log("*** EcholocationRenderFeature: Dispose() called ***");
        echolocationPass?.Dispose();
    }

    // NEW: Add this method for manual debugging
    [ContextMenu("Debug Render Feature")]
    public void DebugRenderFeature()
    {
        Debug.Log("=== RENDER FEATURE DEBUG ===");
        Debug.Log($"Settings exists: {settings != null}");
        Debug.Log($"Material assigned: {settings?.echolocationMaterial != null}");
        Debug.Log($"Material name: {settings?.echolocationMaterial?.name}");
        Debug.Log($"Shader name: {settings?.echolocationMaterial?.shader?.name}");
        Debug.Log($"Debug logs enabled: {settings?.enableDebugLogs}");
        Debug.Log($"Force test mode: {settings?.forceTestMode}");
        Debug.Log($"Always log: {settings?.alwaysLog}");
        Debug.Log($"Render pass exists: {echolocationPass != null}");

        // Check URP setup
        var pipeline = GraphicsSettings.currentRenderPipeline;
        Debug.Log($"Current render pipeline: {pipeline?.name}");
        Debug.Log($"Is URP: {pipeline is UniversalRenderPipelineAsset}");
    }

    class EcholocationRenderPass : ScriptableRenderPass
    {
        private EcholocationSettings settings;
        private RTHandle tempColorTarget;
        private EchoBase echoBase;
        private int frameCount = 0;
        private bool isDisposed = false;

        public EcholocationRenderPass(EcholocationSettings settings)
        {
            this.settings = settings;
            profilingSampler = new ProfilingSampler("Echolocation Effect");

            if (settings.alwaysLog)
                Debug.Log("*** EcholocationRenderPass: Constructor called ***");
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (isDisposed) return;

            if (settings.alwaysLog && frameCount < 3)
                Debug.Log($"*** EcholocationRenderPass: OnCameraSetup called - Frame {frameCount} ***");

            // Create temporary render texture with proper descriptor
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;

            RenderingUtils.ReAllocateIfNeeded(ref tempColorTarget, descriptor,
                FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_EcholocationTemp");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (isDisposed || settings.echolocationMaterial == null || tempColorTarget == null)
            {
                if (settings.alwaysLog && frameCount < 5)
                {
                    Debug.LogError($"Execute early return - Disposed: {isDisposed}, Material: {settings.echolocationMaterial != null}, Target: {tempColorTarget != null}");
                }
                return;
            }

            // Find EchoBase if we don't have it
            if (echoBase == null)
            {
                echoBase = Object.FindFirstObjectByType<EchoBase>();
                if (settings.alwaysLog && frameCount < 3)
                    Debug.Log($"EchoBase found: {echoBase != null}");
            }

            var cmd = CommandBufferPool.Get("Echolocation Effect");

            using (new ProfilingScope(cmd, profilingSampler))
            {
                frameCount++;

                // ALWAYS log first few frames
                if (frameCount <= 10 || settings.enableDebugLogs)
                {
                    Debug.Log($"*** EcholocationRenderPass Execute - Frame {frameCount} ***");
                }

                // Update material properties before rendering
                UpdateMaterialProperties(renderingData.cameraData.camera);

                // Get source and destination
                var cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

                // Apply the echolocation effect
                Blit(cmd, cameraColorTarget, tempColorTarget, settings.echolocationMaterial);
                Blit(cmd, tempColorTarget, cameraColorTarget);

                if (frameCount == 5 || settings.enableDebugLogs)
                {
                    Debug.Log("*** EcholocationRenderPass is working! ***");
                }
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void UpdateMaterialProperties(Camera camera)
        {
            if (settings.echolocationMaterial == null || camera == null) return;

            try
            {
                // Use EchoBase data if available and not in force test mode
                if (echoBase != null && !settings.forceTestMode)
                {
                    echoBase.UpdateEcholocationMaterial(settings.echolocationMaterial);

                    if ((settings.enableDebugLogs || settings.alwaysLog) && frameCount % 120 == 0)
                    {
                        Debug.Log($"Using EchoBase data - Active waves: {echoBase.ActiveWaveCount}, Player pos: {echoBase.PlayerPosition}");
                    }
                }
                else
                {
                    // Fallback test values - guaranteed to work
                    SetFallbackTestValues();

                    if ((settings.enableDebugLogs || settings.alwaysLog) && frameCount % 120 == 0)
                    {
                        Debug.Log("Using fallback test values");
                    }
                }

                // Set camera properties
                SetCameraProperties(camera);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error updating material properties: {e.Message}");

                // Fallback to basic test values
                SetFallbackTestValues();
            }
        }

        private void SetFallbackTestValues()
        {
            var material = settings.echolocationMaterial;

            // Basic properties that should always work
            if (material.HasProperty("_TestMode"))
                material.SetFloat("_TestMode", 1.0f);

            if (material.HasProperty("_BlackoutColor"))
                material.SetColor("_BlackoutColor", Color.black);

            if (material.HasProperty("_RevealedColor"))
                material.SetColor("_RevealedColor", new Color(0.1f, 0.1f, 0.2f, 1f));

            if (material.HasProperty("_EchoColor"))
                material.SetColor("_EchoColor", Color.cyan);

            if (material.HasProperty("_RevealIntensity"))
                material.SetFloat("_RevealIntensity", 0.4f);

            if (material.HasProperty("_EchoRadius"))
                material.SetFloat("_EchoRadius", 5f);

            // Clear wave data for test mode
            if (material.HasProperty("_WaveCount"))
                material.SetInt("_WaveCount", 0);

            if (settings.alwaysLog && frameCount % 240 == 0)
            {
                Debug.Log("Fallback test values set successfully");
            }
        }

        private void SetCameraProperties(Camera camera)
        {
            var material = settings.echolocationMaterial;

            if (material.HasProperty("_CameraWorldPos"))
                material.SetVector("_CameraWorldPos", camera.transform.position);

            if (material.HasProperty("_IsOrthographic"))
                material.SetFloat("_IsOrthographic", camera.orthographic ? 1f : 0f);

            if (material.HasProperty("_OrthographicSize"))
                material.SetFloat("_OrthographicSize", camera.orthographicSize);

            if (material.HasProperty("_GroundHeight"))
                material.SetFloat("_GroundHeight", 0f);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // Cleanup is handled in Dispose
        }

        public void Dispose()
        {
            if (isDisposed) return;

            if (settings.alwaysLog)
                Debug.Log("*** EcholocationRenderPass: Dispose() called ***");

            tempColorTarget?.Release();
            tempColorTarget = null;
            isDisposed = true;
        }
    }
}