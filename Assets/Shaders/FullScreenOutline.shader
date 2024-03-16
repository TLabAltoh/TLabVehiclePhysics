Shader "Hidden/FullScreenOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _OutlineColor ("Outline Color", Color) = (0.0, 0.0, 0.0, 1.0)

        _Threshold ("Threshold", Float) = 0.001
    }
    SubShader
    {
        Tags { "Renderpipeline" = "UniversalPipeline" }

        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.vertex = TransformObjectToHClip(IN.vertex.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            sampler2D _MainTex;

            float4 _MainTex_TexelSize;

            float _Threshold;

            half4 _OutlineColor;

            TEXTURE2D(_CameraDepthTexture);

            SAMPLER(sampler_CameraDepthTexture);

            half4 frag(Varyings IN) : SV_Target
            {
                half4 col = tex2D(_MainTex, IN.uv);

                half depthR = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, IN.uv + float2(_MainTex_TexelSize.x, 0.0));
                half depthL = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, IN.uv - float2(_MainTex_TexelSize.x, 0.0));

                half depthT = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, IN.uv + float2(0.0, _MainTex_TexelSize.y));
                half depthB = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, IN.uv - float2(0.0, _MainTex_TexelSize.y));

                half deltaTB = Linear01Depth(depthT, _ZBufferParams) - Linear01Depth(depthB, _ZBufferParams);
                half deltaRL = Linear01Depth(depthR, _ZBufferParams) - Linear01Depth(depthL, _ZBufferParams);

                half lerp = abs(deltaTB) > _Threshold || abs(deltaRL) > _Threshold;

                col = (1.0 - lerp) * col + lerp * _OutlineColor;

                return col;

                //half linear01Depth = Linear01Depth(depth, _ZBufferParams);
                //return half4(linear01Depth, linear01Depth, linear01Depth, linear01Depth);
            }
            ENDHLSL
        }
    }
}
