Shader "Hidden/BlackMetalPostProcess"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _EffectIntensity;
            float _Contrast;
            float _Brightness;
            float _Desaturation;
            float _GrainIntensity;
            float _GrainSize;
            float _VignetteIntensity;
            float _ScanLines;
            float _CustomTime;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Random noise function
            float rand(float2 co)
            {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }

            // Film grain
            float grain(float2 uv, float time)
            {
                float noise = rand(uv * _GrainSize + time);
                return noise * 2.0 - 1.0;
            }

            // Desaturate to grayscale
            float3 desaturate(float3 color, float amount)
            {
                float gray = dot(color, float3(0.299, 0.587, 0.114));
                return lerp(color, float3(gray, gray, gray), amount);
            }

            // Vignette effect
            float vignette(float2 uv, float intensity)
            {
                float2 center = uv - 0.5;
                float dist = length(center);
                return 1.0 - smoothstep(0.3, 0.8, dist) * intensity;
            }

            // High contrast adjustment
            float3 adjustContrast(float3 color, float contrast, float brightness)
            {
                // Apply brightness
                color = color + brightness;
                
                // Apply contrast
                color = (color - 0.5) * contrast + 0.5;
                
                return color;
            }

            // Scan lines
            float scanLine(float2 uv, float density)
            {
                return sin(uv.y * density) * 0.5 + 0.5;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                // Desaturate (black metal is mostly black/white/gray)
                col.rgb = desaturate(col.rgb, _Desaturation);

                // Apply contrast and brightness for harsh look
                col.rgb = adjustContrast(col.rgb, _Contrast, _Brightness);

                // Add film grain
                if (_GrainIntensity > 0)
                {
                    float grainValue = grain(i.uv, _CustomTime);
                    col.rgb += grainValue * _GrainIntensity;
                }

                // Apply vignette (darker edges)
                if (_VignetteIntensity > 0)
                {
                    float vig = vignette(i.uv, _VignetteIntensity);
                    col.rgb *= vig;
                }

                // Add scan lines (optional)
                if (_ScanLines > 0)
                {
                    float scan = scanLine(i.uv, _ScanLines);
                    col.rgb *= lerp(1.0, scan, 0.1);
                }

                // Clamp harsh blacks and whites
                col.rgb = saturate(col.rgb);

                // Blend with original based on intensity
                fixed4 original = tex2D(_MainTex, i.uv);
                col.rgb = lerp(original.rgb, col.rgb, _EffectIntensity);

                return col;
            }
            ENDCG
        }
    }
}
