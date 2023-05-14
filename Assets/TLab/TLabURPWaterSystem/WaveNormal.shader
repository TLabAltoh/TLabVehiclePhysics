Shader "Unlit/WaveNormal"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_RefTex("Ref",2D) = "black" {}
		_BumpMap("Normalmap", 2D) = "bump" {}
		_BumpAmt("BumpAmt", Range(0,9999)) = 0
		_WaveTex("Wave",2D) = "gray" {}
		_ParallaxScale("Parallax Scale", Float) = 1
		_NormalScaleFactor("Normal Scale Factor", Float) = 1
	}
	SubShader
	{
		Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
		ZWrite On
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha

		CGINCLUDE
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
			float4 ref : TEXCOORD1;
		};

		sampler2D _MainTex;
		sampler2D _RefTex;
		sampler2D _BumpMap;
		sampler2D _WaveTex;
		float4 _MainTex_ST;
		float4 _RefTex_TexelSize;
		float4 _WaveTex_TexelSize;
		float4x4 _RefW;
		float4x4 _RefVP;
		float _BumpAmt;
		float _ParallaxScale;
		float _NormalScaleFactor;

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.ref = mul(_RefVP, mul(_RefW, v.vertex));
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			/* ”g“®•û’ö®‚Ì‰ğ‚ğ_WaveTex‚Åó‚¯æ‚èA”g‚É‚æ‚é˜c‚İ‹ï‡‚ğbump‚É‰ÁZ */
			/* _WaveTex‚Í”g‚Ì‚‚³‚È‚Ì‚ÅA‚‚³‚Ì•Ï‰»—Ê‚©‚ç–@ü‚ğ‹‚ß‚é			*/
			float2 shiftX = { _WaveTex_TexelSize.x,  0 };
			float2 shiftZ = { 0, _WaveTex_TexelSize.y };
			shiftX *= _ParallaxScale * _NormalScaleFactor;
			shiftZ *= _ParallaxScale * _NormalScaleFactor;

			/* Še²‚Å—¼•ûŒü‚Å‚Ì”g‚Ì‚‚³‚Ì•Ï‰»—Ê‚ğæ“¾ */
			float3 texX = 2 * tex2Dlod(_WaveTex, float4(i.uv.xy + shiftX, 0, 0)) - 1;
			float3 texx = 2 * tex2Dlod(_WaveTex, float4(i.uv.xy - shiftX, 0, 0)) - 1;
			float3 texZ = 2 * tex2Dlod(_WaveTex, float4(i.uv.xy + shiftZ, 0, 0)) - 1;
			float3 texz = 2 * tex2Dlod(_WaveTex, float4(i.uv.xy - shiftZ, 0, 0)) - 1;
			float3 du = { 1, 0, _NormalScaleFactor * (texX.x - texx.x) };
			float3 dv = { 0, 1, _NormalScaleFactor * (texZ.x - texz.x) };

			/* Še²‚Ì•Ï‰»—Ê‚ÌŠOÏ(‚»‚Ì–Ê‚É‚¨‚¯‚é–@ü‚ÌŒü‚«)‚ğ‹‚ß‚é */
			float2 bump = (normalize(cross(du, dv)) + 1) * 0.5;			// 0`1‚Ì”ÍˆÍ‚É–ß‚·

			return float4(bump.x, bump.x, bump.x, bump.x);
		}

		ENDCG

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
}
