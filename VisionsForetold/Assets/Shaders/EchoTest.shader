Shader "Custom/EchoTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TestRadius ("Test Radius", Range(0,1)) = 0.3
    }
    
    SubShader
    {
        Tags { "RenderType"="Overlay" }
        LOD 100
        
        Pass
        {
            ZWrite Off
            ZTest Always
            
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
            float _TestRadius;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Simple test: circular reveal in center of screen
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);
                
                if (dist < _TestRadius)
                {
                    // Inside circle - show dimmed original
                    return col * 0.4;
                }
                else
                {
                    // Outside circle - black
                    return fixed4(0, 0, 0, 1);
                }
            }
            ENDCG
        }
    }
}