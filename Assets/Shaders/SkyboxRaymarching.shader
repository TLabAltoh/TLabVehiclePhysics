Shader "Hidden/TLab/Skybox/Raymarching"
{
    Properties
    {
        _ColorUpper("Color Upper", Color) = (0.0, 0.0, 0.0)
        _ColorLower("Color Lower", Color) = (1.0, 1.0, 1.0)
    }
        SubShader
    {
        Tags { "Renderpipeline" = "UniversalPipeline" }

        Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                half4 vertex : POSITION;
                half3 uv : TEXCOORD0;
            };

            struct Varyings
            {
                half3 uv : TEXCOORD0;
                half4 vertex : SV_POSITION;
            };

            half3 _ColorUpper;
            half3 _ColorLower;

            half sphere(half3 pos, half radius)
            {
                return length(pos) - radius;
            }

            half3 repeat(half3 pos, half3 span)
            {
                return abs(fmod(pos, span)) - span * 0.5;
            }

            half getDistance(half3 pos)
            {
                return sphere(repeat(pos, 10.0), 1.0f);
            }

            half3 getNormal(half3 pos) {
                half d = 0.001;
                return normalize(half3(
                    getDistance(pos + half3(d, 0, 0)) - getDistance(pos + half3(-d, 0, 0)),
                    getDistance(pos + half3(0, d, 0)) - getDistance(pos + half3(0, -d, 0)),
                    getDistance(pos + half3(0, 0, d)) - getDistance(pos + half3(0, 0, -d))
                    ));
            }

            half3 raymarch(half3 cameraPos, half3 rayDir)
            {
                half3 pos = cameraPos;
                for (int i = 0; i < 200; i++) {
                    half d = getDistance(pos);
                    pos += d * rayDir;
                    if (d < 0.001) {
                        return getNormal(pos) * 0.5 + 0.5;
                    }
                }
                return 0;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.vertex = TransformObjectToHClip(IN.vertex.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 bgColor = half4(lerp(_ColorLower, _ColorUpper, IN.uv.y * 0.5 + 0.5), 1.0);

                half3 rayDir = normalize(IN.uv);

                return half4(raymarch(_WorldSpaceCameraPos, rayDir), 1.0);
            }
            ENDHLSL
        }
    }
}
