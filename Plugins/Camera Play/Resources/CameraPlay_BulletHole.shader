////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////


Shader "CameraPlay/BulletHole" {
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

	float x = 0;
	float2 pos = float2(_PosX, _PosY);
	uv = uv - pos + float2(0.475, 0.475);

	x = smoothstep(0.739, 0.74, uv.y);
	x += smoothstep(0.739, 0.74, 1 - uv.y);
	x += smoothstep(0.739, 0.74, uv.x);
	x += smoothstep(0.739, 0.74, 1 - uv.x);

	x = 1 - x;
	uv *= 2;
	uv *= 0.25;

	float tm = _cframe;
	if (tm > 47) tm = 47;
	uv.x += tm / 4;
	uv.y += (3-floor(tm/4))*0.25;
	
	uv.x -= 0.125;
	uv.y -= 0.125;

	float4 t2 = tex2D(Texture2, uv);
	float r = t2.r;
	if (tm > 31) r = t2.b;
	else
	if (tm > 15) r = t2.g;
	float l = 1-(tm / 48);
	r *= saturate(x);

	float4 fr = tex2D(_MainTex, i.texcoord+float2(r*0.35*l,0));
	fr += r*_Fade;

	return fr;
	}
		ENDCG
	}
	}
}