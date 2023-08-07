Shader "Unlit/MappingToScreen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        /*Level of Detail�̗��Ń��x���̒Ⴂ�n�[�h�E�F�A�ł͕\�������Ȃ��݂����ȔF��*/
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                /*�ό`�����邽�߂Ɉ����I�u�W�F�N�g�̒��_���W*/
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;

                /*�F�����邽�߂Ɉ����I�u�W�F�N�g�̒��_���W*/
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;

            v2f vert (appdata v)
            {
                v2f o;

                /*���̂��I�u�W�F�N�g��Ԃ���
                �N���b�v���(�J�������Ƃ炦���������Ԃ���ɂ���
                (-1,-1,-1) �` (1,1,1)�Ő��K���������)�֕ϊ����饥�
                
                ����ł��邪�A���̂��߂ɍŌ�uw���Z�v�������ōs��Ȃ��Ƃ����Ȃ�*/
                o.vertex = UnityObjectToClipPos(v.vertex);

                /*Shader�m�[�gp1���Q��*/
                o.uv = ComputeScreenPos(o.vertex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                /*w���Z�����0�`1�̒l��������*/
                half2 pos = i.uv.xy / i.uv.w;

                /*�e�N�X�`�����w���uv���W�ŃT���v�����O
                ���̂Ƃ��e�N�X�`����uv�͕`��̈�Ɏl�������킹�Ă͂��邪�A
                (0,0) �` (1,1)�Œ�`����Ă���*/
                fixed4 col = tex2D(_MainTex, pos);

                return col;
            }
            ENDCG
        }
    }
}
