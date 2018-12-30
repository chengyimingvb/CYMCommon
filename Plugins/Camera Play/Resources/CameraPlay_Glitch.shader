////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

Shader "CameraPlay/Glitch" {
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

	float sat(float t) { return clamp(t, 0.0, 1.0); }
	float2 sat(float2 t) { return clamp(t, 0.0, 1.0); }
	float rand(float2 n) { return frac(sin(dot(n.xy, float2(12.9898, 78.233))) * 43758.5453); }
	float trunc(float x, float num_levels) { return floor(x*num_levels) / num_levels; }
	float2 trunc(float2 x, float2 num_levels) { return floor(x*num_levels) / num_levels; }

	

	float4 frag(v2f i) : COLOR
	{
	float2 uv = i.texcoord.xy;
	float ct = trunc(_Time.y, 4.0);
	float change_rnd = rand(trunc(uv.yy,float2(16,16)) + 150.0 * ct);
	float tf = 16.0*change_rnd;
	float t = 5.0 * trunc(_Time.y, tf);
	float vt_rnd = 0.5 * rand(trunc(uv.yy + t, float2(11,11)));
	vt_rnd += 0.5 * rand(trunc(uv.yy + t, float2(7,7)));
	vt_rnd = vt_rnd * 2.0 - 1.0;
	vt_rnd = sign(vt_rnd) * sat((abs(vt_rnd) - 0.6) / (0.4));
	vt_rnd = lerp(0, vt_rnd, _Fade*2);
	float2 uv_nm = uv;
	uv_nm = sat(uv_nm + float2(0.1*vt_rnd, 0));
	float rn = trunc(_Time.y, 8.0);
	float rnd = rand(float2(rn,rn));
	uv_nm.y = (rnd>lerp(1.0, 0.975, sat(0.4))) ? 1.0 - uv_nm.y : uv_nm.y;
	uv_nm.y = (rnd>lerp(1.0, 0.905, sat(0.2))) ? 1.0 - uv_nm.y : uv_nm.y;
	float4 sampleX = tex2D(_MainTex, uv_nm);
	_Fade *= 2;
	float sample2 = tex2D(_MainTex, uv_nm * float2(1 - (_Fade*0.004), 1) + float2(_Fade*0.002, 0)).r;
	float sample3 = tex2D(_MainTex, uv_nm * float2(1 + (_Fade*0.004), 1) - float2(_Fade*0.002, 0)).b;
	sampleX.r = sample2;
	sampleX.b = sample3;
	return sampleX;
	}
		ENDCG
	}
	}
}