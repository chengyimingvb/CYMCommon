////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

Shader "CameraPlay/ShockWave"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
	_TimeX("Time", Range(0.0, 1.0)) = 1.0
		_ScreenResolution("_ScreenResolution", Vector) = (0.,0.,0.,0.)
	}
		SubShader
	{
		Pass
	{

		ZTest Always
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma target 3.0
		#pragma glsl
		#include "UnityCG.cginc"
		uniform sampler2D _MainTex;
		uniform float _TimeX;
		uniform float _Value;
		uniform float _Value2;
		uniform float _Value3;
		uniform float _Value4;
		uniform float4 _ScreenResolution;
		uniform float2 _MainTex_TexelSize;

	struct appdata_t
	{
		float4 vertex   : POSITION;
		float4 color    : COLOR;
		float2 texcoord : TEXCOORD0;
	};
	struct v2f
	{
		float2 texcoord  : TEXCOORD0;
		float4 vertex   : SV_POSITION;
		float4 color : COLOR;
	};
	v2f vert(appdata_t IN)
	{
		v2f OUT;
		OUT.vertex = UnityObjectToClipPos(IN.vertex);
		OUT.texcoord = IN.texcoord;
		OUT.color = IN.color;
		return OUT;
	}
	float4 frag(v2f i) : COLOR
	{
		float2 uv = i.texcoord.xy;
		float2 uv2 = uv;
		#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
		uv.y = 1 - uv.y;
		#endif
		float Dist = distance(uv, float2(_Value, _Value2));
		float Diff = (Dist - _Value3);
		float v = (1.0 - pow(abs(Diff * 10.0), 0.8));
		float vt = v * 0.02 * _Value4;
		vt = saturate(vt);
		uv2.x = uv2.x - vt;
		uv2.y = uv2.y - vt;
		float4 Color = tex2D(_MainTex, uv2);
		Color.rgb += float3(vt, vt, vt);
		

		
		return Color;
	}
		ENDCG
	}
	}
}
