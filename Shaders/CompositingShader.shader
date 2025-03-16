Shader "Spark2D/CompositingShader" {
    Properties {
        _AccumTex ("Accumulation Texture", 2D) = "white" {}
        _BackgroundTex ("Background Texture", 2D) = "white" {}
    }

    SubShader {
        Tags {
            "Queue"="Transparent" "RenderType"="Transparent"
        }
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _AccumTex;
            sampler2D _BackgroundTex;

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target {
                // Read from accumulation texture (RGB = color, A = opacity)
                float4 accumColor = tex2D(_AccumTex, i.uv);

                // Read background color
                float4 bgColor = tex2D(_BackgroundTex, i.uv);

                // Pre-multiply alpha
                float4 finalColor = float4(accumColor.rgb * accumColor.a, accumColor.a);

                // The blending will be handled by the GPU blend state (SrcAlpha OneMinusSrcAlpha)
                return finalColor;
            }
            ENDCG
        }
    }
}