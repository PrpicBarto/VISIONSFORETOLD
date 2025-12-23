Shader "Custom/BlackMetalOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (0.8,0.8,0.8,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.005
        _Brightness ("Brightness", Range(0, 2)) = 0.6
        _ContrastThreshold ("Contrast Threshold", Range(0, 1)) = 0.6
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "Queue"="Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100
        
        // First pass - outline
        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZWrite On
            ZTest LEqual
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };
            
            CBUFFER_START(UnityPerMaterial)
                float _OutlineWidth;
                half4 _OutlineColor;
                half4 _Color;
                half _Brightness;
                half _ContrastThreshold;
                float4 _MainTex_ST;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Expand along normals for outline
                float3 normalOS = normalize(input.normalOS);
                float3 expandedPos = input.positionOS.xyz + normalOS * _OutlineWidth;
                
                output.positionHCS = TransformObjectToHClip(expandedPos);
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
        
        // Second pass - EXTREME BLACK METAL CONTRAST
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // Simplified multi_compile - only essential keywords to avoid conflicts
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half _Brightness;
                half _ContrastThreshold;
                float _OutlineWidth;
                half4 _OutlineColor;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionHCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Sample texture and DESATURATE immediately (TRVE BLACK METAL!)
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // Convert to grayscale using luminance (full desaturation)
                float luminance = dot(texColor.rgb, float3(0.299, 0.587, 0.114));
                texColor.rgb = half3(luminance, luminance, luminance);
                
                // Get main light with shadows
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                
                // Calculate lighting
                float3 normalWS = normalize(input.normalWS);
                float3 lightDir = normalize(mainLight.direction);
                float ndotl = saturate(dot(normalWS, lightDir));
                
                // EXTREME THRESHOLD for TRVE BLACK METAL - adjustable via property
                float lightIntensity = step(_ContrastThreshold, ndotl);
                
                // Apply shadow with HARSH threshold (pure black or pure white)
                float shadow = mainLight.shadowAttenuation;
                lightIntensity *= step(0.5, shadow);
                
                // Calculate final color with EXTREME contrast
                half4 color = texColor * _Color;
                
                // Apply lighting - either FULL brightness or NEAR BLACK
                if (lightIntensity > 0.5)
                {
                    // Lit area - bright but desaturated
                    color.rgb *= _Brightness;
                }
                else
                {
                    // Shadow area - PURE BLACK (true black metal)
                    color.rgb = half3(0.0, 0.0, 0.0);
                }
                
                // Additional contrast pass - push to extremes
                color.rgb = (color.rgb - 0.5) * 2.0 + 0.5;
                
                // Clamp to 0-1 (pure black or bright)
                color.rgb = saturate(color.rgb);
                
                // Final threshold to ensure ONLY black or white/gray
                color.rgb = step(0.3, color.rgb) * color.rgb;
                
                return color;
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Unlit"
}
