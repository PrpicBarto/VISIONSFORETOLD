Shader "Custom/URP/Echolocation"
{
    Properties
    {
        _PulseOrigin ("Pulse Origin", Vector) = (0, 0, 0, 0)
        _PulseDistance ("Pulse Distance", Float) = 0
        _PulseWidth ("Pulse Width", Float) = 5.0
        _MaxDistance ("Max Distance", Float) = 40.0
        
        _RevealColor ("Reveal Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _DarkColor ("Dark Color", Color) = (0.3, 0.3, 0.35, 1.0)
        _EdgeGlow ("Edge Glow", Float) = 2.0
    }
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        
        Pass
        {
            Name "Echolocation"
            
            ZWrite Off
            ZTest Always
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            // GLOBAL shader properties (set by EcholocationController via Shader.SetGlobalXXX)
            float3 _PulseOrigin;
            float _PulseDistance;
            float _PulseWidth;
            float _MaxDistance;
            float4 _RevealColor;
            float4 _DarkColor;
            float _EdgeGlow;
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewVector : TEXCOORD1;
            };
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                
                float3 viewVector = mul(unity_CameraInvProjection, float4(input.uv * 2.0 - 1.0, 0.0, -1.0)).xyz;
                output.viewVector = mul(unity_CameraToWorld, float4(viewVector, 0.0)).xyz;
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // Reconstruct world position from depth
                float depth = SampleSceneDepth(input.uv);
                float linearDepth = LinearEyeDepth(depth, _ZBufferParams);
                float3 worldPos = _WorldSpaceCameraPos + normalize(input.viewVector) * linearDepth;
                
                // Distance from pulse origin (XZ plane only for isometric view)
                float3 diff = worldPos - _PulseOrigin;
                diff.y = 0; // Ignore vertical distance for isometric
                float dist = length(diff);
                
                // No pulse active - show fog
                if (_PulseDistance < 0.01)
                {
                    return color * _DarkColor;
                }
                
                // Calculate reveal: areas within pulse radius are visible
                half revealed = dist < _PulseDistance ? 1.0 : 0.0;
                
                // Pulse ring glow at the expanding edge
                half ringDist = abs(dist - _PulseDistance);
                half ring = 1.0 - saturate(ringDist / _PulseWidth);
                ring = smoothstep(0.0, 1.0, ring) * smoothstep(0.0, 1.0, ring); // Squared for sharper falloff
                
                // Mix colors
                half4 dark = color * _DarkColor;        // Fog color
                half4 bright = color * _RevealColor;    // Revealed color
                half4 glow = _RevealColor * ring * _EdgeGlow; // Pulse ring glow
                
                // Final composition
                half4 final = lerp(dark, bright, revealed);
                final.rgb += glow.rgb;
                
                return final;
            }
            ENDHLSL
        }
    }
}