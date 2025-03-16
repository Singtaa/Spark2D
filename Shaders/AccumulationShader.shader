Shader "Spark2D/AccumulationShader" {
    Properties {
        _MainTex ("Stamp Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range(0, 1)) = 1
    }

    SubShader {
        Tags {
            "Queue"="Transparent" "RenderType"="Transparent"
        }
        // Use additive blending for accumulation
        Blend One One
        ZWrite Off
        Cull Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _Color;
            float _Intensity;

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            float4 frag(v2f i) : SV_Target {
                float4 stampColor = tex2D(_MainTex, i.uv) * i.color;
                float alpha = stampColor.a * _Intensity;
                
                // For accumulation buffer, we output:
                // RGB = color (not pre-multiplied by alpha)
                // A = opacity/coverage
                return float4(stampColor.rgb * alpha, alpha);
            }
            ENDCG
        }
    }
}