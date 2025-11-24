Shader "Custom/URP/Echolocation"
{
    Properties
    {
        [Header(Fog Settings)]
        _FogColor ("Fog Color", Color) = (0.05, 0.05, 0.08, 1)
        _FogDensity ("Fog Density", Range(0, 1)) = 0.95
        
        [Header(Echolocation Pulse)]
        _PulseCenter ("Pulse Center (World)", Vector) = (0, 0, 0, 0)
        _PulseRadius ("Pulse Radius", Float) = 0
        _PulseWidth ("Pulse Width", Float) = 5.0
        _PulseIntensity ("Pulse Intensity", Range(0, 1)) = 1.0
        
        [Header(Revealed Areas)]
        _RevealRadius ("Permanent Reveal Radius", Float) = 10.0
        _RevealFade ("Reveal Fade Amount", Range(0, 1)) = 0.0
        
        [Header(Edge Glow)]
        _EdgeColor ("Edge Glow Color", Color) = (0.3, 0.6, 1, 1)
        _EdgeIntensity ("Edge Glow Intensity", Float) = 3.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent+100"
            "RenderPipeline" = "UniversalPipeline" 
        }
        
        Pass
        {
            Name "FogOfWar"
            
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
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };
            
            // Properties
            CBUFFER_START(UnityPerMaterial)
                float4 _FogColor;
                float _FogDensity;
                
                float3 _PulseCenter;
                float _PulseRadius;
                float _PulseWidth;
                float _PulseIntensity;
                
                float _RevealRadius;
                float _RevealFade;
                
                float4 _EdgeColor;
                float _EdgeIntensity;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = input.uv;
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // CRITICAL: Calculate distance in XZ plane (top-down/isometric)
                // Project world position to ground plane at player's height
                float3 pixelPosOnGround = input.positionWS;
                pixelPosOnGround.y = _PulseCenter.y;
                
                float3 playerPos = _PulseCenter;
                
                // Calculate horizontal distance only (XZ plane)
                float3 toPixel = pixelPosOnGround - playerPos;
                toPixel.y = 0; // Ignore vertical component
                float distFromPlayer = length(toPixel);
                
                // Start with full fog (everything hidden)
                half4 finalColor = _FogColor;
                half fogAlpha = _FogDensity;
                
                // === PERMANENT REVEAL AREA (around player) ===
                // Small area around player stays visible
                float permanentReveal = 1.0 - saturate(distFromPlayer / _RevealRadius);
                permanentReveal = smoothstep(0.0, 1.0, permanentReveal);
                
                // Apply reveal fade (fog returns over time)
                permanentReveal *= (1.0 - _RevealFade);
                
                // Reduce fog in permanently revealed area
                fogAlpha *= (1.0 - permanentReveal * 0.7);
                
                // === ACTIVE PULSE WAVE ===
                half pulseReveal = 0.0;
                half edgeGlow = 0.0;
                
                if (_PulseRadius > 0.1 && _PulseIntensity > 0.01)
                {
                    // Distance from pulse ring edge
                    float distFromRing = abs(distFromPlayer - _PulseRadius);
                    
                    // Ring mask - bright at edge, fades with width
                    half ringMask = 1.0 - saturate(distFromRing / _PulseWidth);
                    ringMask = smoothstep(0.0, 1.0, ringMask);
                    ringMask = pow(ringMask, 2.0); // Sharper falloff
                    
                    // Areas INSIDE pulse are revealed (visible)
                    if (distFromPlayer < _PulseRadius)
                    {
                        pulseReveal = _PulseIntensity;
                    }
                    
                    // Edge glow effect
                    edgeGlow = ringMask * _EdgeIntensity * _PulseIntensity;
                    
                    // Reduce fog where pulse has revealed
                    fogAlpha *= (1.0 - pulseReveal);
                }
                
                // Clamp fog alpha
                fogAlpha = max(fogAlpha, 0.0);
                
                // Apply edge glow to color
                if (edgeGlow > 0.01)
                {
                    finalColor.rgb = lerp(finalColor.rgb, _EdgeColor.rgb, saturate(edgeGlow));
                }
                
                // Set final alpha
                finalColor.a = saturate(fogAlpha);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}