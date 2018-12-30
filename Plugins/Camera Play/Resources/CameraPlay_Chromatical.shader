//////////////////////////////////////
// CameraPlay - by VETASOFT 2017 /////
//////////////////////////////////////

Shader "CameraPlay/Chromatical" {
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
	    _TimeX("Time", Range(0.0, 1.0)) = 1.0
		_Fade("_Fade", Range(0.0, 1.0)) = 1.0
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
	uniform float _Fade;
	uniform float4 _ScreenResolution;
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
		float s = sin(1-uv.y)*uv.y;
		float b = sin(uv.y * 1024);
		s *= 0.5;
		float d = 0.5 - s;
		uv.x -= s;
		uv.x = uv.x/d;
		uv.x *= 0.375;
		uv.x += 0.125;
		uv.x = lerp(i.texcoord.x, uv.x, _Fade);
		float4 sampleX = tex2D(_MainTex, uv);
		float sample2 = tex2D(_MainTex, uv * float2(1 - (_Fade*0.04), 1) + float2(_Fade*0.02, 0)).r;
		float sample3 = tex2D(_MainTex, uv * float2(1 + (_Fade*0.04), 1) - float2(_Fade*0.02, 0)).b;
		sampleX.r = sample2;
		sampleX.b = sample3;
		float scanline = b*0.1*_Fade;
		sampleX -= scanline;
		return sampleX;
	}
		ENDCG
	}
	}
}