using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace VisionsForetold.Game.Player.Echo
{
    /// <summary>
    /// URP Render Feature for Echolocation (Unity 6 RenderGraph)
    /// 
    /// NOTE: This is the post-processing approach which had issues with black screens.
    /// The current system uses a fog plane overlay instead (see EcholocationController.cs).
    /// 
    /// This file is kept for reference/future use if post-processing approach is needed.
    /// To use this:
    /// 1. Add this Render Feature to your URP Renderer asset
    /// 2. Assign the echolocation material
    /// 3. Disable the fog plane approach in EcholocationController
    /// </summary>
    public class EcholocationRenderFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            [Header("Configuration")]
            [Tooltip("When to apply the effect in the rendering pipeline")]
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            
            [Tooltip("Material using the Custom/URP/Echolocation shader")]
            public Material echolocationMaterial;
            
            [Header("Debug")]
            public bool showDebugLogs = false;
        }

        public Settings settings = new Settings();
        private EcholocationRenderPass renderPass;

        #region Render Feature Lifecycle

        public override void Create()
        {
            if (settings.echolocationMaterial == null)
            {
                Debug.LogWarning("[Echolocation Render Feature] Material is not assigned!");
                return;
            }

            renderPass = new EcholocationRenderPass(settings);
            
            if (settings.showDebugLogs)
            {
                Debug.Log($"[Echolocation Render Feature] Created successfully");
                Debug.Log($"  Render Pass Event: {settings.renderPassEvent}");
                Debug.Log($"  Material: {settings.echolocationMaterial.name}");
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // Only apply to game camera, not scene view
            if (renderingData.cameraData.cameraType != CameraType.Game)
                return;
            
            if (renderPass == null || settings.echolocationMaterial == null)
            {
                if (settings.showDebugLogs)
                    Debug.LogWarning("[Echolocation Render Feature] Cannot add pass - null material or pass");
                return;
            }
            
            // Request color and depth inputs
            renderPass.ConfigureInput(ScriptableRenderPassInput.Color);
            renderPass.ConfigureInput(ScriptableRenderPassInput.Depth);
            
            renderer.EnqueuePass(renderPass);
        }

        protected override void Dispose(bool disposing)
        {
            renderPass?.Dispose();
        }

        #endregion

        #region Render Pass Implementation

        private class EcholocationRenderPass : ScriptableRenderPass
        {
            private Settings settings;
            private Material material;

            private class PassData
            {
                internal Material material;
            }

            public EcholocationRenderPass(Settings settings)
            {
                this.settings = settings;
                this.material = settings.echolocationMaterial;
                renderPassEvent = settings.renderPassEvent;
                
                // Set up profiling sampler for Unity Profiler
                profilingSampler = new ProfilingSampler("Echolocation Effect");
            }

            /// <summary>
            /// Unity 6 RenderGraph API implementation
            /// Records commands for applying the echolocation effect
            /// </summary>
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (material == null)
                {
                    Debug.LogError("[Echolocation Pass] Material is null!");
                    return;
                }

                var resourceData = frameData.Get<UniversalResourceData>();
                var cameraData = frameData.Get<UniversalCameraData>();

                TextureHandle cameraColor = resourceData.activeColorTexture;
                
                if (!cameraColor.IsValid())
                {
                    Debug.LogError("[Echolocation Pass] Camera color texture is invalid!");
                    return;
                }

                // Create descriptor for temporary texture
                RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
                desc.depthBufferBits = 0;
                desc.msaaSamples = 1;

                // Create temporary texture for blit operation
                TextureHandle tempTexture = UniversalRenderer.CreateRenderGraphTexture(
                    renderGraph, desc, "_EchoTemp", false);

                // First pass: Apply echolocation shader
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(
                    "Echolocation Effect", out var passData, profilingSampler))
                {
                    passData.material = material;
                    
                    builder.UseTexture(cameraColor, AccessFlags.Read);
                    builder.SetRenderAttachment(tempTexture, 0, AccessFlags.Write);
                    
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    {
                        // Blit camera color through echolocation shader to temp texture
                        Blitter.BlitTexture(context.cmd, cameraColor, 
                            new Vector4(1, 1, 0, 0), data.material, 0);
                    });
                }

                // Second pass: Copy result back to camera target
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(
                    "Echolocation Copy Back", out var passData, profilingSampler))
                {
                    builder.UseTexture(tempTexture, AccessFlags.Read);
                    builder.SetRenderAttachment(cameraColor, 0, AccessFlags.Write);
                    
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    {
                        // Simple blit without shader (copy operation)
                        Blitter.BlitTexture(context.cmd, tempTexture, 
                            new Vector4(1, 1, 0, 0), 0, false);
                    });
                }
            }

            public void Dispose()
            {
                // RenderGraph handles cleanup automatically in Unity 6
            }
        }

        #endregion
    }
}