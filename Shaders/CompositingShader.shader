Shader "Spark2D/CompositingShader" {
    Properties {
        _AccumTex ("Accumulation Texture", 2D) = "white" {}
        _BackgroundTex ("Background Texture", 2D) = "white" {}
        _BlendMode ("Blend Mode", Float) = 0
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
            float _BlendMode;

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
                
                float4 finalColor;
                
                // Apply different blend modes based on _BlendMode value
                if (_BlendMode < 0.5) {
                    // Normal blend - standard alpha blending
                    finalColor = float4(accumColor.rgb, accumColor.a);
                }
                else if (_BlendMode < 1.5) {
                    // Additive blend - add colors together
                    // We still use SrcAlpha OneMinusSrcAlpha GPU blend, but modify our color
                    // to achieve additive-like effect
                    float3 additiveColor = bgColor.rgb + accumColor.rgb * accumColor.a;
                    finalColor = float4(additiveColor, accumColor.a);
                }
                else if (_BlendMode < 2.5) {
                    // Multiply blend - multiply colors
                    float3 multipliedColor = lerp(bgColor.rgb, bgColor.rgb * accumColor.rgb, accumColor.a);
                    finalColor = float4(multipliedColor, accumColor.a);
                }
                else {
                    // Screen blend - inverse multiply of inverse colors
                    float3 screenColor = lerp(bgColor.rgb, 1.0 - (1.0 - bgColor.rgb) * (1.0 - accumColor.rgb), accumColor.a);
                    finalColor = float4(screenColor, accumColor.a);
                }
                
                return finalColor;
            }
            ENDCG
        }
    }
}