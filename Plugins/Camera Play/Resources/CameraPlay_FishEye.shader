////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

Shader "CameraPlay/FishEye" { 
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
	float2 aspectRatio = float2(1,1);
	float strength = sin(_Fade * 2.0) * 0.2; 
	float2 intensity = float2(strength * aspectRatio.x, strength * aspectRatio.y);
	float2 coords = uv;
	float2 m = float2(_PosX, _PosY);
	coords = (coords - m) * 2.0;
	float2 realCoordOffs=float2(0,0);
	realCoordOffs.x = (1.0 - coords.y * coords.y) * intensity.y * (coords.x);
	realCoordOffs.y = (1.0 - coords.x * coords.x) * intensity.x * (coords.y);
	float4 colorx = tex2D(_MainTex, uv - realCoordOffs);
	return colorx;
	

}
ENDCG
}
}
}
