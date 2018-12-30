// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/GUI Clear Text Shader"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}

	_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		//	[MaterialToggle] PixelSnap ("Pixel snap", Float) = 1

		// Soft Mask
		[PerRendererData] _SoftMask("Mask", 2D) = "white" {}
	}

		SubShader
	{
		Tags
	{
		"Queue" = "Transparent"
		"IgnoreProjector" = "True"
		"RenderType" = "Transparent"
		"PreviewType" = "Plane"
		"CanUseSpriteAtlas" = "True"
	}

		Stencil
	{
		Ref[_Stencil]
		Comp[_StencilComp]
		Pass[_StencilOp]
		ReadMask[_StencilReadMask]
		WriteMask[_StencilWriteMask]
	}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Fog{ Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask[_ColorMask]

		Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"

		// Soft Mask
#include "SoftMask.cginc"

		// Soft Mask
#pragma multi_compile __ SOFTMASK_SIMPLE SOFTMASK_SLICED SOFTMASK_TILED

		struct appdata_t
	{
		float4 vertex   : POSITION;
		float4 color    : COLOR;
		float2 texcoord : TEXCOORD0;
	};

	struct v2f
	{
		float4 vertex   : SV_POSITION;
		fixed4 color : COLOR;
		half2 uv[3]  : TEXCOORD0;
		// Soft Mask
		SOFTMASK_COORDS(3)
	};

	uniform float4 _MainTex_TexelSize;

	v2f vert(appdata_t IN)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(IN.vertex);
		half2 uv = IN.texcoord;
		o.uv[0] = uv;
		o.uv[1] = uv + float2(+_MainTex_TexelSize.x / 12000.0f,0);
		o.uv[2] = uv + float2(-_MainTex_TexelSize.x / 12000.0f,0);
		o.color = IN.color;
		//	o.vertex = UnityPixelSnap (o.vertex);
		SOFTMASK_CALCULATE_COORDS(o, IN.vertex) // Soft Mask
			return o;
	}

	sampler2D _MainTex;

	fixed4 frag(v2f i) : SV_Target
	{
		float c = tex2D(_MainTex, i.uv[0]).a;
	float r = tex2D(_MainTex, i.uv[1]).a;
	float l = tex2D(_MainTex, i.uv[2]).a;

	half4 color = i.color;

	if (c > 0.5f) {
		c *= 1.0f + c * c / 1.5f;
	}

	color.a *= c;

	if ((l < 0.35f && r < 0.35f)) {
		if (r > l && c <= r) {
			color.b *= 1.0f;
			color.b += 0.1f;
			color.g *= 0.5f;
			color.g += 0.05f;
			color.r *= 0.15f;
			color.r += 0.015f;
		}
		else if (l > r && c <= l) {
			color.r *= 1.0f;
			color.r += 0.1f;
			color.g *= 0.3f;
			color.g += 0.03f;
			color.b *= 0.25f;
			color.b += 0.025f;
		}
	}

	color.a *= SOFTMASK_GET_MASK(i); // Soft Mask

	clip(color.a - 0.01f);
	return color;
	}
		ENDCG
	}
	}
}
