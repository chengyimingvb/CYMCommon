////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

Shader "CameraPlay/Thermavision" {
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
float4 color    : COLOR;
};   

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;

return OUT;
}


float4 frag (v2f i) : COLOR
{
	float4 pixcol = tex2D(_MainTex, i.texcoord.xy);
	float4 colors[3];
	colors[0] = float4(0.,0.,1.,1.);
	colors[1] = float4(1.,1.,0.,1.);
	colors[2] = float4(1.,0.,0.,1.);
	float lum = (pixcol.r + pixcol.g + pixcol.b) / 3.;
	float4 thermal;
	float _Value = 1;
	if (lum < 1) thermal = lerp(colors[0],colors[2],(lum - float(0)*_Value) / _Value);
	if (lum >= 1) thermal = lerp(colors[1],colors[2],(lum - float(1)*_Value) / _Value);
	thermal = lerp(pixcol, thermal, _Fade);
	return thermal;

}

ENDCG
}

}
}