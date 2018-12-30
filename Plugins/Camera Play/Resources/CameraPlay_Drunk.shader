////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

Shader "CameraPlay/Drunk" {
Properties 
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_TimeX ("Time", Range(0.0, 1.0)) = 1.0
_Distortion ("_Distortion", Range(0.0, 1.0)) = 0.3
_ScreenResolution ("_ScreenResolution", Vector) = (0.,0.,0.,0.)
_Value ("_Value", Range(0.0, 20.0)) = 6.0
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
uniform float4 _ScreenResolution;
uniform float _Value;
uniform float _Speed;
uniform float _Wavy;
uniform float _Distortion;
uniform float _DistortionWave;
uniform float _Fade;
uniform float _Colored;

uniform float _ColoredChange;
uniform float _ChangeRed;
uniform float _ChangeGreen;
uniform float _ChangeBlue;

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
float t = _TimeX*_Speed;
float drunk 	 = (sin(t*2.0));
float unitDrunk1 = (sin(t*1.2)+1.0)/2.0;
float unitDrunk2 = (sin(t*1.8)+1.0)/2.0;

float2 normalizedCoord = fmod((i.texcoord.xy + (float2(0, drunk) / _ScreenResolution.x)), 1.0);
normalizedCoord.x = pow(normalizedCoord.x, lerp(1.25, 0.85, unitDrunk1));
normalizedCoord.y = pow(normalizedCoord.y, lerp(0.85, 1.25, unitDrunk2));

float2 normalizedCoord2 = fmod((i.texcoord.xy + (float2(drunk, 0.) / _ScreenResolution.x)), 1.0);	
normalizedCoord2.x = pow(normalizedCoord2.x, lerp(0.95, 1.1, unitDrunk2));
normalizedCoord2.y = pow(normalizedCoord2.y, lerp(1.1, 0.95, unitDrunk1));

float2 normalizedCoord3 = i.texcoord.xy;

normalizedCoord = lerp(normalizedCoord3, normalizedCoord, _Wavy);
normalizedCoord2 = lerp(normalizedCoord3, normalizedCoord2, _Wavy);
float4 color  = tex2D(_MainTex, normalizedCoord);	
float dist = (color.x*0.5*normalizedCoord.x)* _Distortion;
float4 color2 = tex2D(_MainTex, normalizedCoord2 + float2(dist, dist));
dist += (color.x*color2.y*0.5*normalizedCoord2.x)* _Distortion;

float y =
0.7*sin((normalizedCoord3.y + t) * 4.0) * 0.038 +
0.3*sin((normalizedCoord3.y + t) * 8.0) * 0.010 +
0.05*sin((normalizedCoord3.y + t) * 40.0) * 0.05;

float x =
0.5*sin((normalizedCoord3.y + t) * 5.0) * 0.1 +
0.2*sin((normalizedCoord3.x + t) * 10.0) * 0.05 +
0.2*sin((normalizedCoord3.x + t) * 30.0) * 0.02;

normalizedCoord3.x += _DistortionWave*x;
normalizedCoord3.y += _DistortionWave*y;
float4 color3 = tex2D(_MainTex, normalizedCoord3+float2(dist,dist));
float4 MemoColor = color3;
float4 finalColor = lerp( lerp(color, color2, lerp(_Colored-0.2, _Colored+0.2, unitDrunk1)), color3, 0.6);
finalColor.yz = lerp(finalColor.yz- _ColoredChange, normalizedCoord3, 0.4);
finalColor = lerp(MemoColor,finalColor,_Fade);
finalColor.r += _ChangeRed;
finalColor.g += _ChangeGreen;
finalColor.b += _ChangeBlue;
return finalColor;	




}

ENDCG
}

}
}