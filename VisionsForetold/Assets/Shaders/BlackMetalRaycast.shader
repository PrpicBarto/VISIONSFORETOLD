Shader "Hidden/BlackMetalRaycast"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineThickness ("Outline Thickness", Range(0.0, 5.0)) = 1.0
        _DepthThreshold ("Depth Threshold", Range(0.0, 1.0)) = 0.1
        _NormalThreshold ("Normal Threshold", Range(0.0, 1.0)) = 0.4
        _Brightness ("Brightness", Range(0.0, 2.0)) = 1.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        
        Cull Off 
        ZWrite Off 
        ZTest Always

        Pass
        {
            Name "BlackMetalRaycast"
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _OutlineThickness;
                float _DepthThreshold;
                float _NormalThreshold;
                float _Brightness;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            // Sample depth at offset
            float SampleDepth(float2 uv, float2 offset)
            {
                float2 sampleUV = uv + offset;
                return SampleSceneDepth(sampleUV);
            }

            // Sample normals at offset
            float3 SampleNormal(float2 uv, float2 offset)
            {
                float2 sampleUV = uv + offset;
                return SampleSceneNormals(sampleUV);
            }

            // Detect edges using Sobel operator on depth
            float DetectDepthEdge(float2 uv, float2 texelSize)
            {
                float thickness = _OutlineThickness;
                
                // Sobel kernel for depth
                float depth00 = SampleDepth(uv, float2(-1, -1) * texelSize * thickness);
                float depth01 = SampleDepth(uv, float2(0, -1) * texelSize * thickness);
                float depth02 = SampleDepth(uv, float2(1, -1) * texelSize * thickness);
                
                float depth10 = SampleDepth(uv, float2(-1, 0) * texelSize * thickness);
                float depth12 = SampleDepth(uv, float2(1, 0) * texelSize * thickness);
                
                float depth20 = SampleDepth(uv, float2(-1, 1) * texelSize * thickness);
                float depth21 = SampleDepth(uv, float2(0, 1) * texelSize * thickness);
                float depth22 = SampleDepth(uv, float2(1, 1) * texelSize * thickness);
                
                // Sobel operator
                float sobelX = depth00 + 2.0 * depth10 + depth20 - depth02 - 2.0 * depth12 - depth22;
                float sobelY = depth00 + 2.0 * depth01 + depth02 - depth20 - 2.0 * depth21 - depth22;
                
                float depthEdge = sqrt(sobelX * sobelX + sobelY * sobelY);
                return step(_DepthThreshold, depthEdge);
            }

            // Detect edges using normals
            float DetectNormalEdge(float2 uv, float2 texelSize)
            {
                float thickness = _OutlineThickness;
                
                // Sample center normal
                float3 normalCenter = SampleNormal(uv, float2(0, 0));
                
                // Sample surrounding normals
                float3 normalUp = SampleNormal(uv, float2(0, 1) * texelSize * thickness);
                float3 normalDown = SampleNormal(uv, float2(0, -1) * texelSize * thickness);
                float3 normalLeft = SampleNormal(uv, float2(-1, 0) * texelSize * thickness);
                float3 normalRight = SampleNormal(uv, float2(1, 0) * texelSize * thickness);
                
                // Calculate differences
                float dotUp = dot(normalCenter, normalUp);
                float dotDown = dot(normalCenter, normalDown);
                float dotLeft = dot(normalCenter, normalLeft);
                float dotRight = dot(normalCenter, normalRight);
                
                // Find minimum dot product (maximum difference)
                float minDot = min(min(dotUp, dotDown), min(dotLeft, dotRight));
                
                // Edge detected if normals differ significantly
                float normalEdge = 1.0 - minDot;
                return step(_NormalThreshold, normalEdge);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                
                // Get screen resolution
                float2 texelSize = 1.0 / _ScreenParams.xy;
                
                // Sample depth to check if geometry exists
                float depth = SampleSceneDepth(uv);
                
                // If depth is near 1.0, it's the skybox/background - render black
                if (depth > 0.9999)
                {
                    return half4(0, 0, 0, 1);
                }
                
                // Detect edges using depth
                float depthEdge = DetectDepthEdge(uv, texelSize);
                
                // Detect edges using normals
                float normalEdge = DetectNormalEdge(uv, texelSize);
                
                // Combine edge detection
                float edge = max(depthEdge, normalEdge);
                
                // TRVE BLACK METAL:
                // If edge detected -> BLACK outline
                // If no edge -> WHITE surface
                if (edge > 0.5)
                {
                    return half4(0, 0, 0, 1); // BLACK outline
                }
                else
                {
                    return half4(_Brightness, _Brightness, _Brightness, 1); // WHITE surface
                }
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
