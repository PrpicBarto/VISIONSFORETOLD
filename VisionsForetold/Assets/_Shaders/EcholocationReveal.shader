Shader "Custom/EcholocationReveal"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _RevealColor ("Reveal Color (Darker)", Color) = (0.2, 0.5, 0.7, 0.6)
        _RevealStrength ("Reveal Strength (Lower=Darker)", Range(0, 2)) = 0.8
        _RevealPulse ("Reveal Pulse Speed", Range(0, 10)) = 2.0
        _EdgeGlow ("Edge Glow (Lower=Subtle)", Range(0, 5)) = 0.5
    }
    
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 200
        
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
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
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float fogCoord : TEXCOORD3;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            // Echolocation reveal data
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half4 _RevealColor;
                float _RevealStrength;
                float _RevealPulse;
                float _EdgeGlow;
                
                // Reveal positions from EchoRevealSystem
                int _RevealCount;
                float4 _RevealPositions[50];
                float _RevealRadii[50];
                float _RevealStrengths[50];
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Sample base texture
                half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                
                // Basic lighting
                Light mainLight = GetMainLight();
                float3 normalWS = normalize(input.normalWS);
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 lighting = NdotL * mainLight.color + half3(0.2, 0.2, 0.2);
                baseColor.rgb *= lighting;
                
                // Calculate reveal effect
                float revealAmount = 0.0;
                float edgeGlow = 0.0;
                
                for (int i = 0; i < _RevealCount; i++)
                {
                    float3 revealPos = _RevealPositions[i].xyz;
                    float revealRadius = _RevealRadii[i];
                    float revealStrength = _RevealStrengths[i];
                    
                    // Distance from this reveal position
                    float dist = distance(input.positionWS, revealPos);
                    
                    // Check if within reveal radius
                    if (dist < revealRadius)
                    {
                        // Calculate falloff
                        float falloff = 1.0 - saturate(dist / revealRadius);
                        
                        // Smooth falloff
                        falloff = pow(falloff, 2.0);
                        
                        // Apply strength
                        revealAmount = max(revealAmount, falloff * revealStrength);
                        
                        // Bold edge glow effect with multiple layers
                        float edgeDist = abs(dist - revealRadius * 0.5);
                        
                        // Wider, softer edge
                        float edgeSoft = 1.0 - saturate(edgeDist / (revealRadius * 0.4));
                        edgeSoft = pow(edgeSoft, 2.0);
                        
                        // Sharp, bright edge
                        float edgeSharp = 1.0 - saturate(edgeDist / (revealRadius * 0.2));
                        edgeSharp = pow(edgeSharp, 4.0);
                        
                        // Combine for bold outline
                        float edgeCombined = max(edgeSoft * 0.5, edgeSharp);
                        edgeGlow = max(edgeGlow, edgeCombined * revealStrength * 1.5);
                    }
                }
                
                // Apply reveal effect (subtle and dark)
                if (revealAmount > 0.01)
                {
                    // Very subtle pulsing
                    float pulse = (sin(_Time.y * _RevealPulse) + 1.0) * 0.5;
                    pulse = pulse * 0.1 + 0.9; // Range 0.9 to 1.0 (barely visible pulse)
                    
                    // Minimal blend with reveal color (much darker)
                    half3 revealedColor = lerp(baseColor.rgb, _RevealColor.rgb * _RevealStrength, revealAmount * 0.15);
                    
                    // Very subtle edge glow
                    revealedColor += _RevealColor.rgb * edgeGlow * _EdgeGlow * pulse * 0.5;
                    
                    // Add extra bright edge accent
                    half3 edgeAccent = _RevealColor.rgb * pow(edgeGlow, 2.0) * _EdgeGlow * 0.3;
                    revealedColor += edgeAccent * pulse;
                    
                    // Minimal brightness boost (keep it dark)
                    revealedColor *= (1.0 + revealAmount * 0.1);
                    
                    baseColor.rgb = revealedColor;
                }
                
                // Apply fog
                baseColor.rgb = MixFog(baseColor.rgb, input.fogCoord);
                
                return baseColor;
            }
            ENDHLSL
        }
    }
    
    // Fallback for Built-in Pipeline
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _RevealColor;
            float _RevealStrength;
            float _RevealPulse;
            float _EdgeGlow;
            
            int _RevealCount;
            float4 _RevealPositions[50];
            float _RevealRadii[50];
            float _RevealStrengths[50];
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // Basic lighting
                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float NdotL = max(0, dot(normalize(i.worldNormal), lightDir));
                col.rgb *= NdotL * 0.8 + 0.2;
                
                // Reveal effect
                float revealAmount = 0.0;
                float edgeGlow = 0.0;
                
                for (int idx = 0; idx < _RevealCount; idx++)
                {
                    float3 revealPos = _RevealPositions[idx].xyz;
                    float revealRadius = _RevealRadii[idx];
                    float revealStrength = _RevealStrengths[idx];
                    
                    float dist = distance(i.worldPos, revealPos);
                    
                    if (dist < revealRadius)
                    {
                        float falloff = 1.0 - saturate(dist / revealRadius);
                        falloff = pow(falloff, 2.0);
                        revealAmount = max(revealAmount, falloff * revealStrength);
                        
                        // Bold edge with multiple layers
                        float edgeDist = abs(dist - revealRadius * 0.5);
                        float edgeSoft = 1.0 - saturate(edgeDist / (revealRadius * 0.4));
                        edgeSoft = pow(edgeSoft, 2.0);
                        float edgeSharp = 1.0 - saturate(edgeDist / (revealRadius * 0.2));
                        edgeSharp = pow(edgeSharp, 4.0);
                        float edgeCombined = max(edgeSoft * 0.5, edgeSharp);
                        edgeGlow = max(edgeGlow, edgeCombined * revealStrength * 1.5);
                    }
                }
                
                if (revealAmount > 0.01)
                {
                    float pulse = (sin(_Time.y * _RevealPulse) + 1.0) * 0.5;
                    pulse = pulse * 0.1 + 0.9;
                    
                    fixed3 revealedColor = lerp(col.rgb, _RevealColor.rgb * _RevealStrength, revealAmount * 0.15);
                    revealedColor += _RevealColor.rgb * edgeGlow * _EdgeGlow * pulse * 0.5;
                    
                    // Add edge accent for bolder outline
                    fixed3 edgeAccent = _RevealColor.rgb * pow(edgeGlow, 2.0) * _EdgeGlow * 0.3;
                    revealedColor += edgeAccent * pulse;
                    
                    revealedColor *= (1.0 + revealAmount * 0.1);
                    
                    col.rgb = revealedColor;
                }
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Diffuse"
}
