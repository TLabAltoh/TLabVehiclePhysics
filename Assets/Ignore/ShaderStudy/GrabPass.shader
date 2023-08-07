Shader "Unlit/GrabPass"
{
    Properties
    {
        _RelativeRefractionIndex("Relative Refraction Index", Range(0.0, 1.0)) = 0.67
        [PowerSlider(5)]_Distance("Distance", Range(0.0, 100.0)) = 10.0
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100
        Cull Back
        ZWrite On
        ZTest LEqual
        ColorMask RGB

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                half4 vertex    : POSITION;
                half4 texcoord  : TEXCOORD0;
                half3 normal    : NORMAL;
            };

            struct v2f
            {
                half4 vertex    : SV_POSITION;
                half2 samplingViewportPos   : TEXCOORD0;
            };

            /*GrabPassの代替えテクスチャ。宣言しなくてもそのまま使える*/
            sampler2D _CameraOpaqueTexture;
            float _RelativeRefractionIndex;
            float _Distance;

            v2f vert (appdata v)
            {
                v2f o = (v2f)0;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex);

                // ワールド空間における法線を求める
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);

                // _WorldSpaceCameraPosは定義済み値
                half3 viewDir = normalize(worldPos - _WorldSpaceCameraPos.xyz);

                // 屈折方向を求める
                half3 refractDir = refract(viewDir, worldNormal, _RelativeRefractionIndex);

                // 屈折方向の先にある位置をサンプリング位置とする
                half3 samplingPos = worldPos + refractDir * _Distance;

                // サンプリング位置をプロジェクション変換
                half4 samplingScreenPos = mul(UNITY_MATRIX_VP, half4(samplingPos, 1.0));

                // w除算と正規化を行ってuv座標に変換(取得)
                o.samplingViewportPos = (samplingScreenPos.xy / samplingScreenPos.w) * 0.5 + 0.5;
                #if UNITY_UV_STARTS_AT_TOP
                o.samplingViewportPos.y = 1.0 - o.samplingViewportPos.y;
                #endif

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_CameraOpaqueTexture, i.samplingViewportPos);
            }
            ENDCG
        }
    }
}
