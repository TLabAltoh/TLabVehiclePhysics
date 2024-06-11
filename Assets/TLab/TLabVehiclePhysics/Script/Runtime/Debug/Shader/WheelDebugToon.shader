Shader "TLab/WheelDebugToon"
{
    // https://elekibear.com/post/20230101_02
    // https://qiita.com/flankids/items/a92b14834792a10798e5

    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _Alpha("Alpha", float) = 0.5
    }

    SubShader
    {
        Tags {  "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            float4 _Color;
            float _Alpha;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.normal = TransformObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half nl = max(0, dot(i.normal, _MainLightPosition.xyz));
                if (nl <= 0.01f) {
                    nl = 0.3f;
                }
                else if (nl <= 0.3f) {
                    nl = 0.5f;
                }
                else {
                    nl = 1.0f;
                }

                half4 col = _Color * nl;
                col.a = _Alpha;
                return col;
            }
            ENDHLSL
        }
    }
}
