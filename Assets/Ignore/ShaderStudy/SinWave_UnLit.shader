Shader "Unlit/SinWave_UnLit"
{
    /*�v���O�����̖��O��()�͂��Ȃ��悤�ɂ���
    (���ʂ����������Ȃ邽��)*/

    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _WaveLength("WaveLength", float) = 5.0
        _YabaiyoCoefficient("YabaiyoCoefficient", float) = 1.0
        _WaveDir("WaveDir", Vector) = (0.0, 1.0, 0.0, 0.0)
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _YabaiyoCoefficient;
            float _WaveLength;
            float4 _WaveDir;

            /*const�ɐݒ肵���l�͕ύX�ł��Ȃ�(����ƃG���[�ɂȂ�)*/
            static const float PI = 3.14159265f;
            static const float g = 9.81f;

            v2f vert (appdata v)
            {
                float _2pi_per_L = 2.0f * PI / _WaveLength;
                float A = _YabaiyoCoefficient * _WaveLength / 14.0f;

                /*�g�̍������v�Z*/
                float d = dot(_WaveDir.xy, v.vertex.xz);
                float B = sqrt(_2pi_per_L * g) * _Time.y;
                float H = A * sin(_2pi_per_L * d - B);

                /*�g�̍�����y���W�ɗ^����*/
                v.vertex.y = H;

                /*���_�@�����Z�o
                �����������ʂ̋��ʕ����������ɂ܂Ƃ߂�*/
                float cosV = cos(_2pi_per_L * d - B);

                /*x,z���ꂼ��̕Δ����̌���*/
                float dx = A * _2pi_per_L * _WaveDir.x * cosV;
                float dz = A * _2pi_per_L * _WaveDir.y * cosV;

                /*�O�ς��v�Z���Ă��̌��ʂ̐��K�����@���ɂȂ�
                �����ɂ͂��̌��ʂ����\��*/
                float3 normal = normalize(float3(-dx, 1.0f, -dz));

                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(normal);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                return col;
            }
            ENDCG
        }
    }
}
