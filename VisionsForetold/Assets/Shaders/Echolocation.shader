Shader "Custom/EcholocationIsometric"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PlayerPosition ("Player Position", Vector) = (0,0,0,0)
        _EchoRadius ("Echo Radius", Float) = 10.0
        _BlackoutColor ("Blackout Color", Color) = (0,0,0,1)
        _EchoColor ("Echo Color", Color) = (0,1,1,1)
        _RevealedColor ("Revealed Area Color", Color) = (0.1,0.1,0.15,1)
        _GroundHeight ("Ground Height", Float) = 0.0
        _IsOrthographic ("Is Orthographic", Float) = 1.0
        _OrthographicSize ("Orthographic Size", Float) = 5.0
        _TestMode ("Test Mode", Float) = 0.0
        _RevealIntensity ("Reveal Intensity", Range(0,1)) = 0.3
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Overlay" 
            "Queue" = "Overlay"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100
        
        Pass
        {
            Name "EcholocationPass"
            ZWrite Off
            ZTest Always
            Cull Off
            Blend Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Main properties
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            
            float4 _PlayerPosition;
            float _EchoRadius;
            half4 _BlackoutColor;
            half4 _EchoColor;
            half4 _RevealedColor;
            float _GroundHeight;
            float _IsOrthographic;
            float _OrthographicSize;
            float _TestMode;
            float _RevealIntensity;
            
            // Camera properties
            float4x4 _ViewProjectionInverse;
            float4 _CameraWorldPos;
            
            // Echo wave data
            int _WaveCount;
            float4 _WavePositions[8];
            float4 _WaveData[8];

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Sample the original texture
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // Start with complete blackout
                half4 finalColor = _BlackoutColor;
                
                if (_TestMode > 0.5)
                {
                    // TEST MODE: Simple circular reveal in center
                    float2 center = float2(0.5, 0.5);
                    float dist = distance(input.uv, center);
                    float revealRadius = _EchoRadius / 20.0;
                    
                    if (dist < revealRadius)
                    {
                        // Inside reveal area - show dimmed original
                        half4 revealedArea = col * _RevealIntensity;
                        finalColor = lerp(_BlackoutColor, revealedArea, 0.8);
                    }
                    else
                    {
                        // Outside reveal area - blackout
                        finalColor = _BlackoutColor;
                    }
                }
                else
                {
                    // NORMAL MODE: Use wave data
                    float totalReveal = 0.0;
                    float maxWaveVisual = 0.0;
                    
                    // Check echo waves for revealed areas
                    for (int w = 0; w < min(_WaveCount, 8); w++)
                    {
                        if (_WaveData[w].x <= 0.001) continue; // Skip inactive waves
                        
                        float3 waveOrigin = _WavePositions[w].xyz;
                        float waveRadius = _WavePositions[w].w;
                        float waveIntensity = _WaveData[w].x;
                        
                        // Simple screen-space distance for testing
                        float2 screenPos = input.uv;
                        float2 waveScreenPos = float2(0.5, 0.5); // Center for now
                        float distanceToWave = distance(screenPos, waveScreenPos) * 20.0; // Scale to world units
                        
                        if (distanceToWave <= waveRadius)
                        {
                            // Inside wave - reveal this area
                            float distanceFactor = 1.0 - saturate(distanceToWave / waveRadius);
                            float reveal = distanceFactor * waveIntensity;
                            totalReveal = max(totalReveal, reveal);
                        }
                        
                        // Create visible wave border (sonar ring)
                        float waveBorder = abs(distanceToWave - waveRadius);
                        float borderThickness = 0.5;
                        
                        if (waveBorder < borderThickness)
                        {
                            float borderFactor = 1.0 - saturate(waveBorder / borderThickness);
                            float waveVisual = borderFactor * waveIntensity;
                            maxWaveVisual = max(maxWaveVisual, waveVisual);
                        }
                    }
                    
                    // Static reveal around player (small constant area)
                    float2 center = float2(0.5, 0.5);
                    float distanceToPlayer = distance(input.uv, center) * 20.0;
                    float staticRevealRadius = _EchoRadius * 0.15;
                    
                    if (distanceToPlayer <= staticRevealRadius)
                    {
                        float staticReveal = 1.0 - saturate(distanceToPlayer / staticRevealRadius);
                        totalReveal = max(totalReveal, staticReveal * 0.8);
                    }
                    
                    // Clamp total reveal
                    totalReveal = saturate(totalReveal);
                    
                    if (totalReveal > 0.001)
                    {
                        // Area is revealed
                        half4 dimmedOriginal = col * _RevealIntensity;
                        half4 revealedArea = lerp(_RevealedColor, dimmedOriginal, 0.5);
                        
                        // Add sonar wave highlights
                        if (maxWaveVisual > 0.001)
                        {
                            revealedArea.rgb += _EchoColor.rgb * maxWaveVisual * 0.6;
                        }
                        
                        // Blend based on reveal strength
                        finalColor = lerp(_BlackoutColor, revealedArea, totalReveal);
                    }
                    else
                    {
                        // Area not revealed - pure blackout
                        finalColor = _BlackoutColor;
                    }
                }
                
                return finalColor;
            }
            ENDHLSL
        }
    }
    
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}