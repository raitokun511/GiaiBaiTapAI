Shader "UI/RoundedCorners"
{
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _Radius ("Corner Radius", Range(0,1)) = 0.2
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
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

            fixed4 _Color;
            float _Radius;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 uv = i.uv;
                float2 dist = abs(uv - 0.5);
                float edge = 0.5 - _Radius;
                if (dist.x > edge && dist.y > edge) {
                    float2 corner = dist - edge;
                    if (dot(corner, corner) > _Radius * _Radius)
                        discard;
                }
                return _Color;
            }
            ENDCG
        }
    }
}
