////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

Shader "CameraPlay/SniperScope" { 
Properties 
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_TimeX ("Time", Range(0.0, 1.0)) = 1.0
_ScreenResolution ("_ScreenResolution", Vector) = (0.,0.,0.,0.)
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
uniform float _Fade;
uniform float _TimeX;
uniform float _Value;
uniform float _Value2;
uniform float4 _ScreenResolution;
uniform float _Cible;
uniform float _Distortion;
uniform float _ExtraColor;
uniform float _PosX;
uniform float _PosY;
uniform float4 _Tint;
uniform float _ExtraLight;
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
_Value = lerp(2.5,_Value,_Fade);
_Cible *= _Fade;
float2 uv = i.texcoord.xy;
float2 uvx = uv;
float2 center = float2(_PosX, _PosY);
uvx += center;
center += float2(0.5, 0.5);
float2 px = float2(1-center.x - 0.003, 1-center.x + 0.003);
float2 py = float2(1-center.y - 0.003, 1-center.y + 0.003);
float drawline = smoothstep(px.x,px.y, uv);
drawline *= smoothstep(px.y, px.x, uv);
float drawline2 = smoothstep(py.x, py.y, float2(uv.y, uv.x));
drawline2 *= smoothstep(py.y, py.x, float2(uv.y, uv.x));
drawline += drawline2;
drawline *= 4;
drawline *= _Cible; 
uvx.x = uvx.x / _ScreenResolution.z;
float2 uv2 = length(float2(0.5/ _ScreenResolution.z,0.5) - uvx);
float4 rgb=float4(0,0,0,0);
float dist2 = 1.0 - smoothstep(_Value,_Value-_Value2, uv2);
rgb.r = dist2*0.25;
dist2 += smoothstep(_Value, _Value - _Value2 - 0.1, uv2);
rgb.rg += dist2*0.125;
dist2 -= smoothstep(_Value, _Value - _Value2 - 0.2, uv2);
rgb.rg *= dist2;
drawline= drawline*(1-dist2)*dist2+(drawline*0.1);
dist2 += drawline;
float d = dist2*_Distortion;
float3 black=float3(0.0,0.0,0.0);
float4 tex = tex2D(_MainTex, uv+float2(d*0.05,d*0.05));
tex = lerp(tex, tex*_Tint, _Tint.a*_Fade);
tex += rgb*_ExtraColor; 
tex += drawline;
_ExtraLight *= _Fade;
float3 ret=lerp(tex*(1 + _ExtraLight),black,dist2);

return  float4( ret, 1.0 );
}
ENDCG
}
}
}
