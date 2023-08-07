Shader "Unlit/MappingToScreen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        /*Level of Detailの略でレベルの低いハードウェアでは表示させないみたいな認識*/
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                /*変形させるために扱うオブジェクトの頂点座標*/
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;

                /*色をつけるために扱うオブジェクトの頂点座標*/
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;

            v2f vert (appdata v)
            {
                v2f o;

                /*物体をオブジェクト空間から
                クリップ空間(カメラがとらえた視錐台空間を基準にした
                (-1,-1,-1) ～ (1,1,1)で正規化した空間)へ変換する･･･
                
                つもりであるが、そのために最後「w除算」を自分で行わないといけない*/
                o.vertex = UnityObjectToClipPos(v.vertex);

                /*Shaderノートp1を参照*/
                o.uv = ComputeScreenPos(o.vertex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                /*w除算すれば0～1の値が得られる*/
                half2 pos = i.uv.xy / i.uv.w;

                /*テクスチャを指定のuv座標でサンプリング
                このときテクスチャのuvは描画領域に四隅を合わせてはいるが、
                (0,0) ～ (1,1)で定義されている*/
                fixed4 col = tex2D(_MainTex, pos);

                return col;
            }
            ENDCG
        }
    }
}
