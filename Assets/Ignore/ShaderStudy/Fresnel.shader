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
                �w�肳�ꂽ�I�u�W�F�N�g��Ԃ̒��_�ʒu����J�����ւ�
                �I�u�W�F�N�g��ԕ�����Ԃ�(normalize�Ő��K��)
                */
                half3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
                o.vdotn = dot(viewDir, v.normal.xyz);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                /*�e���_�ɂ�����vdotn�̒l���v�Z�ɉ����ĕԂ��Ă���*/
                half fresnel = _F0 + (1.0h - _F0) * pow(1.0h - i.vdotn, 5);

                /*
                half��fixed4�ɕϊ�(��(fresnel,fresnel,fresnel,fresnel))�����̂�
                ��(�Z��)����(����)�n���̌��ʂ����Ԃ���Ȃ�
                */
                return fresnel;
            }
            ENDCG
        }
    }
}
