Shader "Hidden/TLab/UI/Fade"
{
    Properties
    {
         [HideInInspector] _MainTex("Texture", 2D) = "white" {}

        _Dist("Dist", float) = 0.0

        _Angle("Angle", Range(0.0, 3.14)) = 0.0

        _XOffset("XOffset", float) = 0.0

        _YOffset("YOffset", float) = 0.0
    }
        SubShader
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }

            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha

            LOD 100
            ZTest Off
            ZWrite Off

            Pass
            {
                CGPROGRAM
                #pragma fragmentoption ARB_precision_hint_fastest
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"
                #include "UnityUI.cginc"

                struct appdata
                {
                    half4 vertex : POSITION;
                    half2 uv : TEXCOORD0;
                    half4 color: COLOR;
                };

                struct v2f
                {
                    half2 uv : TEXCOORD0;
                    half4 vertex : SV_POSITION;
                    half4 color: COLOR;
                };

                sampler2D _MainTex;
                half4 _MainTex_TexelSize;

                half _Dist;
                half _Angle;
                half _XOffset;
                half _YOffset;

                v2f vert(appdata IN)
                {
                    v2f OUT;
                    OUT.vertex = UnityObjectToClipPos(IN.vertex.xyz);
                    OUT.uv = IN.uv;
                    OUT.color = IN.color;

                    return OUT;
                }

                half4 frag(v2f IN) : SV_Target
                {
                    half cosAngle = cos(_Angle);
                    half sinAngle = sin(_Angle);

                    half2 uv;

                    uv.x = (IN.uv.x - _XOffset) * cosAngle + (IN.uv.y - _YOffset) * sinAngle;
                    uv.y = (IN.uv.y - _YOffset) * cosAngle - (IN.uv.x - _XOffset) * sinAngle;

                    uv.x += _XOffset;
                    uv.y += _YOffset;

                    IN.uv.x -= _XOffset;
                    IN.uv.y -= _YOffset;

                    half grad = sinAngle / cosAngle;
                    half dist = abs(grad * IN.uv.x - IN.uv.y) / sqrt(1 * 1 + grad * grad);

                    if (dist <= _Dist) {
                        return half4(0, 0, 0, 0);
                    }

                    half2 offset = half2(-sinAngle, cosAngle) * (IN.uv.y > -grad * IN.uv.x) * (dist - _Dist);

                    half4 color = tex2D(_MainTex, uv) * IN.color;

                    return color;
                }
                ENDCG
            }
        }
}
