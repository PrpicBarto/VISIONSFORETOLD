Shader "Custom/URP/Echolocation"
{
    Properties
    {
        [Header(Fog Settings)]
        _FogColor ("Fog Color", Color) = (0.05, 0.05, 0.08, 1)
        _FogDensity ("Fog Density", Range(0, 1)) = 0.95
        _FogDistanceFalloff ("Distance Falloff", Float) = 100.0
        _FogMinDensity ("Minimum Fog Density (Near)", Range(0, 1)) = 0.3
        _FogMaxDensity ("Maximum Fog Density (Far)", Range(0, 1)) = 1.0
        
        [Header(Echolocation Pulse)]
        _PulseCenter ("Pulse Center (World)", Vector) = (0, 0, 0, 0)
        _PulseRadius ("Pulse Radius", Float) = 0
        _PulseWidth ("Pulse Width", Float) = 5.0
        _PulseIntensity ("Pulse Intensity", Range(0, 1)) = 1.0
        
        [Header(Revealed Areas)]
        _RevealRadius ("Permanent Reveal Radius", Float) = 10.0
        _RevealFade ("Reveal Fade Amount", Range(0, 1)) = 0.0
        
        [Header(Object Reveals)]
        _RevealCount ("Number of Revealed Objects", Int) = 0
        
        [Header(Edge Glow)]
        _EdgeColor ("Edge Glow Color", Color) = (0.3, 0.6, 1, 1)
        _EdgeIntensity ("Edge Glow Intensity", Float) = 1.5
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
                float _FogDistanceFalloff;
                float _FogMinDensity;
                float _FogMaxDensity;
                
                float3 _PulseCenter;
                float _PulseRadius;
                float _PulseWidth;
                float _PulseIntensity;
                
                float _RevealRadius;
                float _RevealFade;
                
                float4 _EdgeColor;
                float _EdgeIntensity;
                
                int _RevealCount;
            CBUFFER_END
            
            // Arrays for revealed objects (defined outside CBUFFER)
            float4 _RevealPositions[50];
            float _RevealRadii[50];
            float _RevealStrengths[50];
            
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
                
                // === DISTANCE-BASED FOG DENSITY ===
                // Fog gets denser with distance from player
                float distanceRatio = saturate(distFromPlayer / _FogDistanceFalloff);
                
                // Quadratic falloff for more dramatic effect
                // Close to player: _FogMinDensity, Far from player: _FogMaxDensity
                float distanceDensityMultiplier = _FogMinDensity + (distanceRatio * distanceRatio * (_FogMaxDensity - _FogMinDensity));
                fogAlpha *= distanceDensityMultiplier;
                
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
                
                // === REVEALED OBJECTS ===
                // Clear fog around objects that have been revealed by pulse
                float objectReveal = 0.0;
                
                for (int i = 0; i < _RevealCount && i < 50; i++)
                {
                    float3 objectCenter = _RevealPositions[i].xyz;
                    float objectRadius = _RevealRadii[i];
                    float revealStrength = _RevealStrengths[i];
                    
                    // Calculate distance to revealed object (XZ plane for 2D, or full 3D)
                    float3 toObject = input.positionWS - objectCenter;
                    toObject.y = 0; // Keep 2D for now (can make this configurable)
                    float distToObject = length(toObject);
                    
                    // Reveal area around object with smooth falloff
                    float reveal = 1.0 - saturate(distToObject / objectRadius);
                    reveal = smoothstep(0.0, 1.0, reveal);
                    reveal *= revealStrength; // Apply fade in/out
                    
                    // Accumulate reveals (max of all reveals)
                    objectReveal = max(objectReveal, reveal);
                }
                
                // Apply object reveals to fog
                fogAlpha *= (1.0 - objectReveal);
                
                // Clamp fog alpha
                fogAlpha = max(fogAlpha, 0.0);
                
                // Apply edge glow to color (additive, respecting fog alpha)
                if (edgeGlow > 0.01)
                {
                    // Only add glow to areas with fog
                    half glowStrength = saturate(edgeGlow * 0.5); // Reduced intensity
                    finalColor.rgb = _FogColor.rgb + (_EdgeColor.rgb * glowStrength * fogAlpha);
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