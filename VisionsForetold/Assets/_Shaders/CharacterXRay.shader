Shader "Custom/CharacterXRay"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _XRayColor ("X-Ray Color", Color) = (0.2, 0.8, 1.0, 0.8)
        _XRayStrength ("X-Ray Strength", Range(0, 1)) = 0.8
        _RimPower ("Rim Power", Range(0.1, 8.0)) = 3.0
    }
    
    SubShader
    {
        // Normal rendering pass (when not occluded)
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        
        Pass
        {
            Name "NORMAL"
            Tags { "LightMode" = "UniversalForward" }
            
            ZWrite On
            ZTest LEqual
            Cull Back
            
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
                float fogCoord : TEXCOORD2;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.normalWS = normalInput.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                
                Light mainLight = GetMainLight();
                float3 normalWS = normalize(input.normalWS);
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                
                half3 lighting = NdotL * mainLight.color + half3(0.2, 0.2, 0.2);
                baseColor.rgb *= lighting;
                
                baseColor.rgb = MixFog(baseColor.rgb, input.fogCoord);
                
                return baseColor;
            }
            ENDHLSL
        }
        
        // X-Ray pass (when occluded - renders THROUGH walls)
        Pass
        {
            Name "XRAY"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            
            ZWrite Off
            ZTest Greater // Only render when BEHIND something
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Back
            
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
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };
            
            CBUFFER_START(UnityPerMaterial)
                half4 _XRayColor;
                float _XRayStrength;
                float _RimPower;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.screenPos = ComputeScreenPos(output.positionCS);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Rim lighting effect
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                float rim = 1.0 - saturate(dot(normalWS, viewDirWS));
                rim = pow(rim, _RimPower);
                
                // Pulsing effect
                float pulse = (sin(_Time.y * 3.0) + 1.0) * 0.5;
                pulse = pulse * 0.3 + 0.7; // Range 0.7 to 1.0
                
                half4 color = _XRayColor;
                color.rgb *= rim * pulse;
                color.a = _XRayStrength * rim;
                
                return color;
            }
            ENDHLSL
        }
    }
    
    // Fallback for Built-in Pipeline
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        
        // Normal pass
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
                float3 worldNormal : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                float3 worldNormal = normalize(i.worldNormal);
                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float NdotL = max(0, dot(worldNormal, lightDir));
                col.rgb *= NdotL * 0.8 + 0.2;
                return col;
            }
            ENDCG
        }
        
        // X-Ray pass
        Pass
        {
            ZWrite Off
            ZTest Greater
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };
            
            fixed4 _XRayColor;
            float _XRayStrength;
            float _RimPower;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = _WorldSpaceCameraPos - worldPos;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                float3 normalWS = normalize(i.worldNormal);
                float3 viewDir = normalize(i.viewDir);
                float rim = 1.0 - saturate(dot(normalWS, viewDir));
                rim = pow(rim, _RimPower);
                
                float pulse = (sin(_Time.y * 3.0) + 1.0) * 0.5;
                pulse = pulse * 0.3 + 0.7;
                
                fixed4 col = _XRayColor;
                col.rgb *= rim * pulse;
                col.a = _XRayStrength * rim;
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Diffuse"
}
