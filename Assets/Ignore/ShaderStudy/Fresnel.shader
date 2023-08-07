Shader "Unlit/Flesnel"
{
    Properties
    {
        [PowerSlider(0.1)] _F0("F0", Range(0.0, 1.0)) = 0.02
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                half3 normal : NORMAL;
            };

            struct v2f
            {
                float2 vdotn : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _F0;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                /*
                ObjSpaceViewDir()
                指定されたオブジェクト空間の頂点位置からカメラへの
                オブジェクト空間方向を返す(normalizeで正規化)
                */
                half3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
                o.vdotn = dot(viewDir, v.normal.xyz);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                /*各頂点におけるvdotnの値を計算に加えて返している*/
                half fresnel = _F0 + (1.0h - _F0) * pow(1.0h - i.vdotn, 5);

                /*
                halfがfixed4に変換(→(fresnel,fresnel,fresnel,fresnel))されるので
                白(濃い)か黒(薄い)系統の結果しか返されない
                */
                return fresnel;
            }
            ENDCG
        }
    }
}
