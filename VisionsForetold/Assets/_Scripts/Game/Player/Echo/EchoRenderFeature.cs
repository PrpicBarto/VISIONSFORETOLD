using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace VisionsForetold.Game.Player.Echo
{
    /// <summary>
    /// URP Render Feature for echolocation fog of war effect (Unity 6 RenderGraph)
    /// </summary>
    public class EcholocationRenderFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            public Material echolocationMaterial;
        }

        public Settings settings = new Settings();
        private EcholocationRenderPass renderPass;

        public override void Create()
        {
            if (settings.echolocationMaterial == null)
            {
                Debug.LogWarning("Echolocation material is not assigned!");
                return;
            }

            renderPass = new EcholocationRenderPass(settings);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderPass != null && settings.echolocationMaterial != null)
            {
                renderPass.ConfigureInput(ScriptableRenderPassInput.Depth);
                renderer.EnqueuePass(renderPass);
            }
        }

        protected override void Dispose(bool disposing)
        {
            renderPass?.Dispose();
        }

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
            }

            // UNITY 6 RENDER GRAPH API
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (material == null) return;

                var resourceData = frameData.Get<UniversalResourceData>();
                var cameraData = frameData.Get<UniversalCameraData>();

                TextureHandle source = resourceData.activeColorTexture;

                RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
                desc.depthBufferBits = 0;
                desc.msaaSamples = 1;

                TextureHandle temp = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_EchoTemp", false);

                // Apply echolocation shader
                using (var builder = renderGraph.AddRasterRenderPass<PassData>("Echolocation Effect", out var passData))
                {
                    passData.material = material;
                    builder.UseTexture(source, AccessFlags.Read);
                    builder.SetRenderAttachment(temp, 0, AccessFlags.Write);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    {
                        Blitter.BlitTexture(context.cmd, source, new Vector4(1, 1, 0, 0), data.material, 0);
                    });
                }

                // Copy back to camera target
                using (var builder = renderGraph.AddRasterRenderPass<PassData>("Echo Copy", out var passData))
                {
                    builder.UseTexture(temp, AccessFlags.Read);
                    builder.SetRenderAttachment(source, 0, AccessFlags.Write);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    {
                        Blitter.BlitTexture(context.cmd, temp, new Vector4(1, 1, 0, 0), 0, false);
                    });
                }
            }

            public void Dispose()
            {
                // RenderGraph handles cleanup automatically
            }
        }
    }
}