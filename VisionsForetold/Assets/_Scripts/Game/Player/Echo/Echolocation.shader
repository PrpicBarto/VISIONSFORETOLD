Shader "Custom/URP/Echolocation"
{
    Properties
    {
        [Header(Fog Settings)]
        _FogColor ("Fog Color", Color) = (0.0, 0.0, 0.0, 1.0)
        _FogDensity ("Fog Density", Range(0, 1)) = 1.0
        _FogDistanceFalloff ("Distance Falloff", Float) = 100.0
        _FogMinDensity ("Minimum Fog Density (Near)", Range(0, 1)) = 0.95
        _FogMaxDensity ("Maximum Fog Density (Far)", Range(0, 1)) = 1.0
        
        [Header(Vertical Distance Settings)]
        _VerticalInfluence ("Vertical Distance Influence", Range(0, 1)) = 0.3
        
        [Header(Echolocation Pulse)]
        _PulseCenter ("Pulse Center (World)", Vector) = (0, 0, 0, 0)
        _PulseRadius ("Pulse Radius", Float) = 0
        _PulseWidth ("Pulse Width", Float) = 5.0
        _PulseIntensity ("Pulse Intensity", Range(0, 1)) = 1.0
        _PulseAge ("Pulse Age (Time)", Float) = 0
        _PulseFadeDelay ("Fade Delay After Pulse", Float) = 0.5
        _PulseFadeSpeed ("Fade Back Speed", Float) = 2.0
        
        [Header(Revealed Areas)]
        _RevealRadius ("Permanent Reveal Radius", Float) = 10.0
        _RevealFade ("Reveal Fade Amount", Range(0, 1)) = 0.0
        
        [Header(Object Reveals)]
        _RevealCount ("Number of Revealed Objects", Int) = 0
        
        [Header(Pulse Memory)]
        _MemoryCount ("Number of Memory Zones", Int) = 0
        
        [Header(Edge Glow)]
        _EdgeColor ("Edge Glow Color", Color) = (0.3, 0.6, 1, 1)
        _EdgeIntensity ("Edge Glow Intensity", Float) = 1.5
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent+200"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }
        
        Pass
        {
            Name "FogOfWar"
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
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
                
                float _VerticalInfluence;
                
                float3 _PulseCenter;
                float _PulseRadius;
                float _PulseWidth;
                float _PulseIntensity;
                float _PulseAge;
                float _PulseFadeDelay;
                float _PulseFadeSpeed;
                
                float _RevealRadius;
                float _RevealFade;
                
                float4 _EdgeColor;
                float _EdgeIntensity;
                
                int _RevealCount;
                int _MemoryCount;
            CBUFFER_END
            
            // Arrays for revealed objects (defined outside CBUFFER)
            float4 _RevealPositions[50];
            float _RevealRadii[50];
            float _RevealStrengths[50];
            
            // Arrays for pulse memory zones
            float _MemoryRadii[10];
            float _MemoryStrengths[10];
            
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
                // Calculate distance (supports both 2D and 3D modes)
                float3 toPixel = input.positionWS - _PulseCenter;
                
                // Calculate horizontal distance (XZ plane)
                float horizontalDist = length(toPixel.xz);
                
                // Calculate vertical offset (Y axis)
                float verticalOffset = abs(toPixel.y);
                
                // Combine with configurable vertical influence
                float distFromPlayer = sqrt(
                    horizontalDist * horizontalDist + 
                    (verticalOffset * verticalOffset * _VerticalInfluence)
                );
                
                // Distance-based fog density
                float distanceRatio = saturate(distFromPlayer / _FogDistanceFalloff);
                half fogAlpha = _FogDensity * (_FogMinDensity + distanceRatio * distanceRatio * (_FogMaxDensity - _FogMinDensity));
                
                // === PERMANENT REVEAL AROUND PLAYER ===
                // Always keep player visible with smooth falloff
                if (_RevealRadius > 0.1)
                {
                    float revealDist = saturate(distFromPlayer / _RevealRadius);
                    revealDist = smoothstep(0.0, 1.0, revealDist); // Smooth edges
                    fogAlpha *= revealDist; // Reduce fog near player
                }
                
                // Pulse reveal and edge glow
                half edgeGlow = 0.0;
                half pulseReveal = 0.0;
                
                if (_PulseRadius > 0.1)
                {
                    // Calculate if pixel is within or behind the pulse
                    float distanceToPulse = distFromPlayer - _PulseRadius;
                    
                    // Pulse has passed this pixel
                    if (distanceToPulse < 0)
                    {
                        // Calculate fade based on time since pulse passed
                        // The further behind the pulse ring, the longer it's been revealed
                        float timeSincePulsePassed = abs(distanceToPulse) / (_PulseRadius + 0.01); // Normalized 0-1
                        timeSincePulsePassed = saturate(timeSincePulsePassed * _PulseAge);
                        
                        // Apply fade delay and speed
                        float fadeAmount = saturate((timeSincePulsePassed - _PulseFadeDelay) * _PulseFadeSpeed);
                        
                        // Reveal starts at full intensity, then fades back to fog
                        pulseReveal = (1.0 - fadeAmount) * _PulseIntensity;
                    }
                    // Pulse ring is at this pixel
                    else if (distanceToPulse <= _PulseWidth)
                    {
                        // Full reveal at the pulse ring edge
                        pulseReveal = _PulseIntensity;
                        
                        // Edge glow at pulse ring
                        float ringPosition = distanceToPulse / _PulseWidth; // 0 at ring, 1 at edge
                        half ringMask = 1.0 - ringPosition;
                        ringMask = ringMask * ringMask; // Squared for sharper falloff
                        edgeGlow = ringMask * _EdgeIntensity * _PulseIntensity;
                    }
                    
                    fogAlpha *= (1.0 - pulseReveal);
                }
                
                // Apply edge glow to color
                half4 finalColor;
                if (edgeGlow > 0.01)
                {
                    half glowStrength = edgeGlow * 0.5;
                    finalColor.rgb = _FogColor.rgb + (_EdgeColor.rgb * glowStrength * fogAlpha);
                }
                else
                {
                    finalColor.rgb = _FogColor.rgb;
                }
                
                finalColor.a = saturate(fogAlpha);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}