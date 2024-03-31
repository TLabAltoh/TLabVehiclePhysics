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
            Tags { "Renderpipeline" = "UniversalPipeline" }

            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            ZWrite Off

            Pass
            {
                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                struct Attributes
                {
                    half4 vertex : POSITION;
                    half2 uv : TEXCOORD0;
                    half4 color: COLOR;
                };

                struct Varyings
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

                Varyings vert(Attributes IN)
                {
                    Varyings OUT;
                    OUT.vertex = TransformObjectToHClip(IN.vertex.xyz);
                    OUT.uv = IN.uv;
                    OUT.color = IN.color;

                    return OUT;
                }

                half4 frag(Varyings IN) : SV_Target
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
                ENDHLSL
            }
        }
}
