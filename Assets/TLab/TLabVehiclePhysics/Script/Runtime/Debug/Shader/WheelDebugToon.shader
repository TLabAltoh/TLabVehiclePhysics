Shader "TLab/WheelDebugToon"
{
    // https://elekibear.com/post/20230101_02
    // https://qiita.com/flankids/items/a92b14834792a10798e5

    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _Alpha("Alpha", float) = 0.5

        _WireframeAliasing("Wireframe aliasing", float) = 1.5
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
            #pragma geometry geom

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

            struct g2f {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float3 barycentric : TEXCOORD1;
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

            // This applies the barycentric coordinates to each vertex in a triangle.
            [maxvertexcount(3)]
            void geom(triangle v2f IN[3], inout TriangleStream<g2f> triStream) {
                g2f o;

                o.vertex = IN[0].vertex;
                o.normal = IN[0].normal;
                o.uv = IN[0].uv;
                o.barycentric = float3(1.0, 0.0, 0.0);
                triStream.Append(o);

                o.vertex = IN[1].vertex;
                o.normal = IN[1].normal;
                o.uv = IN[1].uv;
                o.barycentric = float3(0.0, 1.0, 0.0);
                triStream.Append(o);

                o.vertex = IN[2].vertex;
                o.normal = IN[2].normal;
                o.uv = IN[2].uv;
                o.barycentric = float3(0.0, 0.0, 1.0);
                triStream.Append(o);
            }

            half4 _WireframeColour;
            half _WireframeAliasing;

            half4 frag(g2f i) : SV_Target
            {
                // Calculate the unit width based on triangle size.
                half3 unitWidth = fwidth(i.barycentric);
                // Alias the line a bit.
                half3 aliased = smoothstep(float3(0.0, 0.0, 0.0), unitWidth * _WireframeAliasing, i.barycentric);
                // Use the coordinate closest to the edge.
                half alpha = 1 - min(aliased.x, min(aliased.y, aliased.z));

                if (alpha > 0.5) {
                    return half4(_Color.r, _Color.g, _Color.b, alpha);
                }

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
