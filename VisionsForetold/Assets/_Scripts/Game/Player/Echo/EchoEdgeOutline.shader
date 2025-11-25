Shader "Custom/URP/EchoEdgeOutline"
{
    Properties
    {
        _EdgeColor ("Edge Color", Color) = (0.3, 0.8, 1, 1)
        _EdgeThickness ("Edge Thickness", Range(0.0, 0.1)) = 0.02
        _Alpha ("Alpha", Range(0, 1)) = 1.0
        _BackgroundColor ("Background Color", Color) = (0, 0, 0, 0)
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline" 
        }
        
        Pass
        {
            Name "EdgeDetection"
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _EdgeColor;
                float _EdgeThickness;
                float _Alpha;
                float4 _BackgroundColor;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.uv = input.uv;
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Normalize vectors
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                
                // Calculate Fresnel effect (edge detection based on view angle)
                float fresnel = 1.0 - saturate(dot(normalWS, viewDirWS));
                
                // Sharpen the edge
                fresnel = pow(fresnel, 1.0 / max(_EdgeThickness, 0.001));
                fresnel = saturate(fresnel);
                
                // Create edge mask
                float edgeMask = step(1.0 - _EdgeThickness * 10.0, fresnel);
                
                // Blend between background and edge color
                half4 finalColor = lerp(_BackgroundColor, _EdgeColor, edgeMask);
                finalColor.a *= _Alpha;
                
                // If not an edge and background is transparent, discard
                if (edgeMask < 0.5 && _BackgroundColor.a < 0.01)
                {
                    finalColor.a = edgeMask * _Alpha;
                }
                
                return finalColor;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
