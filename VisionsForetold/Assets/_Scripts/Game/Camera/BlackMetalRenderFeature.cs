using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Black Metal fullscreen post-processing effect
/// Creates iconic black metal album cover aesthetic:
/// - High contrast, desaturated
/// - Heavy film grain and noise
/// - Intense vignetting
/// - Edge detection for graphic novel look
/// </summary>
public class BlackMetalRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        [Header("Effect Settings")]
        public Material material;
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        
        [Header("Black Metal Style")]
        [Range(1.0f, 3.0f)] public float contrast = 2.0f;
        [Range(0.0f, 1.0f)] public float saturation = 0.1f;
        [Range(-0.5f, 0.5f)] public float brightness = -0.1f;
        
        [Header("Grain and Noise")]
        [Range(0.0f, 1.0f)] public float grainAmount = 0.3f;
        [Range(1.0f, 5.0f)] public float grainSize = 2.0f;
        [Range(0.0f, 2.0f)] public float noiseSpeed = 0.5f;
        
        [Header("Vignette")]
        [Range(0.0f, 1.0f)] public float vignetteIntensity = 0.6f;
        [Range(0.0f, 1.0f)] public float vignetteSmoothness = 0.5f;
        [Range(0.0f, 1.0f)] public float vignetteRoundness = 0.5f;
        
        [Header("Color Grading")]
        public Color colorTint = new Color(0.9f, 0.95f, 1.0f, 1.0f);
        public Color shadowTint = new Color(0.0f, 0.0f, 0.1f, 1.0f);
        public Color midtoneTint = new Color(0.5f, 0.5f, 0.6f, 1.0f);
        
        [Header("Fog and Atmosphere")]
        [Range(0.0f, 1.0f)] public float fogAmount = 0.2f;
        public Color fogColor = new Color(0.1f, 0.1f, 0.15f, 1.0f);
        
        [Header("Sharpening")]
        [Range(0.0f, 2.0f)] public float sharpness = 0.5f;
        
        [Header("Edge Detection")]
        [Range(0.0f, 1.0f)] public float edgeIntensity = 0.3f;
        [Range(0.0f, 2.0f)] public float edgeThickness = 1.0f;
    }

    public Settings settings = new Settings();
    private BlackMetalRenderPass renderPass;

    public override void Create()
    {
        renderPass = new BlackMetalRenderPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null)
        {
            Debug.LogWarning("[BlackMetalEffect] Material is not assigned!");
            return;
        }

        renderer.EnqueuePass(renderPass);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            renderPass?.Dispose();
        }
    }

    class BlackMetalRenderPass : ScriptableRenderPass
    {
        private Settings settings;
        private RTHandle source;
        private RTHandle destination;
        private RTHandle tempRTHandle;
        
        private static readonly int ContrastID = Shader.PropertyToID("_Contrast");
        private static readonly int SaturationID = Shader.PropertyToID("_Saturation");
        private static readonly int BrightnessID = Shader.PropertyToID("_Brightness");
        
        private static readonly int GrainAmountID = Shader.PropertyToID("_GrainAmount");
        private static readonly int GrainSizeID = Shader.PropertyToID("_GrainSize");
        private static readonly int NoiseSpeedID = Shader.PropertyToID("_NoiseSpeed");
        
        private static readonly int VignetteIntensityID = Shader.PropertyToID("_VignetteIntensity");
        private static readonly int VignetteSmoothnessID = Shader.PropertyToID("_VignetteSmoothness");
        private static readonly int VignetteRoundnessID = Shader.PropertyToID("_VignetteRoundness");
        
        private static readonly int ColorTintID = Shader.PropertyToID("_ColorTint");
        private static readonly int ShadowTintID = Shader.PropertyToID("_ShadowTint");
        private static readonly int MidtoneTintID = Shader.PropertyToID("_MidtoneTint");
        
        private static readonly int FogAmountID = Shader.PropertyToID("_FogAmount");
        private static readonly int FogColorID = Shader.PropertyToID("_FogColor");
        
        private static readonly int SharpnessID = Shader.PropertyToID("_Sharpness");
        
        private static readonly int EdgeIntensityID = Shader.PropertyToID("_EdgeIntensity");
        private static readonly int EdgeThicknessID = Shader.PropertyToID("_EdgeThickness");

        public BlackMetalRenderPass(Settings settings)
        {
            this.settings = settings;
            this.renderPassEvent = settings.renderPassEvent;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            
            RenderingUtils.ReAllocateIfNeeded(ref tempRTHandle, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp);
        }
        
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // Configure is called within render pass scope
        }

        public void Setup(ScriptableRenderer renderer)
        {
            source = renderer.cameraColorTargetHandle;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (settings.material == null)
                return;

            // Get camera color target handle within the render pass scope
            var renderer = renderingData.cameraData.renderer;
            source = renderer.cameraColorTargetHandle;

            CommandBuffer cmd = CommandBufferPool.Get("BlackMetalEffect");

            // Set material properties
            settings.material.SetFloat(ContrastID, settings.contrast);
            settings.material.SetFloat(SaturationID, settings.saturation);
            settings.material.SetFloat(BrightnessID, settings.brightness);
            
            settings.material.SetFloat(GrainAmountID, settings.grainAmount);
            settings.material.SetFloat(GrainSizeID, settings.grainSize);
            settings.material.SetFloat(NoiseSpeedID, settings.noiseSpeed);
            
            settings.material.SetFloat(VignetteIntensityID, settings.vignetteIntensity);
            settings.material.SetFloat(VignetteSmoothnessID, settings.vignetteSmoothness);
            settings.material.SetFloat(VignetteRoundnessID, settings.vignetteRoundness);
            
            settings.material.SetColor(ColorTintID, settings.colorTint);
            settings.material.SetColor(ShadowTintID, settings.shadowTint);
            settings.material.SetColor(MidtoneTintID, settings.midtoneTint);
            
            settings.material.SetFloat(FogAmountID, settings.fogAmount);
            settings.material.SetColor(FogColorID, settings.fogColor);
            
            settings.material.SetFloat(SharpnessID, settings.sharpness);
            
            settings.material.SetFloat(EdgeIntensityID, settings.edgeIntensity);
            settings.material.SetFloat(EdgeThicknessID, settings.edgeThickness);

            // Blit with effect
            Blitter.BlitCameraTexture(cmd, source, tempRTHandle, settings.material, 0);
            Blitter.BlitCameraTexture(cmd, tempRTHandle, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // Cleanup is handled by RTHandle system
        }

        public void Dispose()
        {
            tempRTHandle?.Release();
        }
    }
}
