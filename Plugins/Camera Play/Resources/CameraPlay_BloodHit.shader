////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////


Shader "CameraPlay/BloodHit" {
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
	uniform sampler2D Texture2;
	uniform float _TimeX;
	uniform float _Fade;
	uniform float _PosX;
	uniform float _PosY;
	uniform float _cframe;
	uniform float4 color;
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
	float2 uv = i.texcoord;
	float2 duv = uv;
	duv.x += (sin((duv.y + (_Time.y * 0.20)) * 50) * 0.009) +
	(sin((duv.y + (_Time.y * 0.21)) * (50)) * 0.005);
	duv.y += (cos((duv.x + (_Time.y * 0.20)) * 50) * 0.009) +
		(cos((duv.x + (_Time.y * 0.21)) * (50)) * 0.005);


	float4 rx = tex2D(_MainTex, duv);
	
	float2 pos = float2(_PosX, _PosY);
	uv = uv - pos + float2(0.475, 0.475);

	float tm = _cframe;
	if (tm > 47) tm = 47;
	
	float x = 0;
	x = smoothstep(0.9, 1, uv.y);
	x += smoothstep(0.9, 1, 1 - uv.y);
	x += smoothstep(0.9, 1, uv.x);
	x += smoothstep(0.9, 1, 1 - uv.x);

	x = 1 - x;
	uv *= 0.25;
	
	uv.x += tm / 4;
	uv.y += (3-floor(tm/4))*0.25;
	

	float4 t2 = tex2D(Texture2, uv);
	float r = t2.r;
	if (tm > 31) r = t2.b;
	else
	if (tm > 15) r = t2.g;
	float l = 1-(tm / 48);

	float4 fr = tex2D(_MainTex, i.texcoord);
	
	r *= saturate(x);
	fr = lerp(fr, rx*0.2+color, r*_Fade);
	float c = saturate(0.25 - _cframe / 128);
	fr.r += c * color.r;
	fr.g += c * color.g;
	fr.b += c * color.b;

	return fr;
	}
		ENDCG
	}
	}
}