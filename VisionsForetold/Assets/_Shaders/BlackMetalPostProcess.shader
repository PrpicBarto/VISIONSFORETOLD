Shader "Custom/PostProcess/BlackMetal"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        
        [Header(Black Metal Style)]
        _Contrast ("Contrast", Range(1.0, 3.0)) = 2.0
        _Saturation ("Saturation", Range(0.0, 1.0)) = 0.1
        _Brightness ("Brightness", Range(-0.5, 0.5)) = -0.1
        
        [Header(Grain and Noise)]
        _GrainAmount ("Grain Amount", Range(0.0, 1.0)) = 0.3
        _GrainSize ("Grain Size", Range(1.0, 5.0)) = 2.0
        _NoiseSpeed ("Noise Speed", Range(0.0, 2.0)) = 0.5
        
        [Header(Vignette)]
        _VignetteIntensity ("Vignette Intensity", Range(0.0, 1.0)) = 0.6
        _VignetteSmoothness ("Vignette Smoothness", Range(0.0, 1.0)) = 0.5
        _VignetteRoundness ("Vignette Roundness", Range(0.0, 1.0)) = 0.5
        
        [Header(Color Grading)]
        _ColorTint ("Color Tint", Color) = (0.9, 0.95, 1.0, 1.0)
        _ShadowTint ("Shadow Tint", Color) = (0.0, 0.0, 0.1, 1.0)
        _MidtoneTint ("Midtone Tint", Color) = (0.5, 0.5, 0.6, 1.0)
        
        [Header(Fog and Atmosphere)]
        _FogAmount ("Fog Amount", Range(0.0, 1.0)) = 0.2
        _FogColor ("Fog Color", Color) = (0.1, 0.1, 0.15, 1.0)
        
        [Header(Sharpening)]
        _Sharpness ("Sharpness", Range(0.0, 2.0)) = 0.5
        
        [Header(Edge Detection)]
        _EdgeIntensity ("Edge Intensity", Range(0.0, 1.0)) = 0.3
        _EdgeThickness ("Edge Thickness", Range(0.0, 2.0)) = 1.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            Name "BlackMetalEffect"
            
            ZWrite Off
            ZTest Always
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            
            // Black Metal Parameters
            float _Contrast;
            float _Saturation;
            float _Brightness;
            
            // Grain
            float _GrainAmount;
            float _GrainSize;
            float _NoiseSpeed;
            
            // Vignette
            float _VignetteIntensity;
            float _VignetteSmoothness;
            float _VignetteRoundness;
            
            // Color Grading
            half4 _ColorTint;
            half4 _ShadowTint;
            half4 _MidtoneTint;
            
            // Fog
            float _FogAmount;
            half4 _FogColor;
            
            // Sharpening
            float _Sharpness;
            
            // Edge Detection
            float _EdgeIntensity;
            float _EdgeThickness;
            
            // Noise function
            float hash(float2 p)
            {
                p = frac(p * float2(443.897, 441.423));
                p += dot(p, p.yx + 19.19);
                return frac(p.x * p.y);
            }
            
            float noise(float2 uv, float time)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f);
                
                float n = hash(i + time * _NoiseSpeed);
                return n;
            }
            
            // Film grain
            float filmGrain(float2 uv, float time)
            {
                float2 grainUV = uv * _GrainSize;
                float grain = noise(grainUV, time);
                return lerp(0.5, grain, _GrainAmount);
            }
            
            // Vignette
            float vignette(float2 uv)
            {
                float2 d = abs(uv - 0.5) * 2.0;
                d = pow(d, lerp(float2(1.0, 1.0), float2(2.0, 2.0), _VignetteRoundness));
                float dist = length(d);
                return smoothstep(1.0, 1.0 - _VignetteSmoothness, dist) * (1.0 - _VignetteIntensity) + _VignetteIntensity;
            }
            
            // Desaturation
            float3 desaturate(float3 color, float amount)
            {
                float luminance = dot(color, float3(0.299, 0.587, 0.114));
                return lerp(float3(luminance, luminance, luminance), color, amount);
            }
            
            // Contrast adjustment
            float3 adjustContrast(float3 color, float contrast)
            {
                return saturate((color - 0.5) * contrast + 0.5);
            }
            
            // Color grading
            float3 colorGrade(float3 color)
            {
                float luminance = dot(color, float3(0.299, 0.587, 0.114));
                
                // Shadow, midtone, highlight blending
                float shadow = smoothstep(0.0, 0.3, luminance);
                float highlight = smoothstep(0.7, 1.0, luminance);
                float midtone = 1.0 - shadow - highlight;
                
                float3 graded = color;
                graded = lerp(graded, graded * _ShadowTint.rgb, (1.0 - shadow) * _ShadowTint.a);
                graded = lerp(graded, graded * _MidtoneTint.rgb, midtone * _MidtoneTint.a);
                graded = lerp(graded, graded * _ColorTint.rgb, _ColorTint.a);
                
                return graded;
            }
            
            // Edge detection (Sobel)
            float detectEdges(float2 uv)
            {
                float2 texelSize = _MainTex_TexelSize.xy * _EdgeThickness;
                
                // Sample surrounding pixels
                float3 c00 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-texelSize.x, -texelSize.y)).rgb;
                float3 c10 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, -texelSize.y)).rgb;
                float3 c20 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(texelSize.x, -texelSize.y)).rgb;
                
                float3 c01 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-texelSize.x, 0)).rgb;
                float3 c21 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(texelSize.x, 0)).rgb;
                
                float3 c02 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-texelSize.x, texelSize.y)).rgb;
                float3 c12 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, texelSize.y)).rgb;
                float3 c22 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(texelSize.x, texelSize.y)).rgb;
                
                // Sobel operator
                float3 sobelX = c00 + 2.0 * c01 + c02 - c20 - 2.0 * c21 - c22;
                float3 sobelY = c00 + 2.0 * c10 + c20 - c02 - 2.0 * c12 - c22;
                
                float edge = length(float2(length(sobelX), length(sobelY)));
                return edge;
            }
            
            // Sharpening
            float3 sharpen(float2 uv, float3 center)
            {
                float2 texelSize = _MainTex_TexelSize.xy;
                
                float3 up = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, texelSize.y)).rgb;
                float3 down = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - float2(0, texelSize.y)).rgb;
                float3 left = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - float2(texelSize.x, 0)).rgb;
                float3 right = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(texelSize.x, 0)).rgb;
                
                float3 laplacian = 4.0 * center - up - down - left - right;
                return center + laplacian * _Sharpness;
            }
            
            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                
                // Sample base color
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                
                // Apply sharpening
                color.rgb = sharpen(uv, color.rgb);
                
                // Desaturate (black metal aesthetic)
                color.rgb = desaturate(color.rgb, _Saturation);
                
                // High contrast
                color.rgb = adjustContrast(color.rgb, _Contrast);
                
                // Color grading
                color.rgb = colorGrade(color.rgb);
                
                // Brightness adjustment
                color.rgb += _Brightness;
                
                // Film grain
                float time = _Time.y;
                float grain = filmGrain(uv, time);
                color.rgb *= grain;
                
                // Edge detection (add dark edges for graphic novel look)
                float edges = detectEdges(uv);
                color.rgb = lerp(color.rgb, float3(0, 0, 0), edges * _EdgeIntensity);
                
                // Atmospheric fog
                color.rgb = lerp(color.rgb, _FogColor.rgb, _FogAmount);
                
                // Vignette
                float vig = vignette(uv);
                color.rgb *= vig;
                
                // Clamp to prevent overbrightness
                color.rgb = saturate(color.rgb);
                
                return color;
            }
            ENDHLSL
        }
    }
    
    FallBack Off
}
