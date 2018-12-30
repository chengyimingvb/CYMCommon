////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

Shader "CameraPlay/Zoom" { 
Properties 
{
_MainTex ("Base (RGB)", 2D) = "white" {}
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
uniform float _PosX;
uniform float _PosY;
uniform float _Fade;
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

};
v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;

return OUT;
}

float4 frag (v2f i) : COLOR
{
	float2 uv = i.texcoord.xy;
	uv = uv / 2;
	float2 m = float2(_PosX, _PosY);
	uv += m/2;
	uv=lerp(i.texcoord.xy, uv, _Fade);
	float4 txt = tex2D(_MainTex, uv);
	return txt;
}

ENDCG
}
}
}
