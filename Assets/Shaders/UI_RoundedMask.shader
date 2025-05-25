
Shader "UI/RoundedMask"
{
    Properties {
        _Radius ("Corner Radius", Range(0, 0.5)) = 0.1
        _Color ("Color", Color) = (1,1,1,1)
    }

    SubShader {
        Tags { "Queue" = "Overlay" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            Name "Mask"
            Stencil {
                Ref 1
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _Radius;
            fixed4 _Color;

            v2f vert(appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                float2 uvSize = float2(_ScreenParams.x / _ScreenParams.y, 1.0);
                float2 pos = (i.uv - 0.5) * uvSize;
                float2 dist = abs(pos);
                float edge = 0.5 - _Radius;

                if (dist.x > edge && dist.y > edge) {
                    float2 corner = dist - edge;
                    if (dot(corner, corner) > _Radius * _Radius)
                        return fixed4(0, 0, 0, 0); // transparent
                }

                return _Color;
            }
            ENDCG
        }
    }
}
