////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

Shader "CameraPlay/Noise" { 
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

float rand(float2 seed) 
{
float dotResult = dot(seed.xy, float2(12.9898,78.233));
float sins = sin(dotResult) * 43758.5453;
return frac(sins);
}

float4 frag (v2f i) : COLOR 
{
float2 uv = i.texcoord.xy;
float4 s = tex2D(_MainTex,uv);
float4 lineColor = float4(0,0,0,1);
uv.x = floor(uv.x * 640) / 640;
uv.y = floor(uv.y * 480) / 480;
float factor = rand(uv*floor(_TimeX * 20));
factor = saturate(factor*0.25*_Fade);

float4 finalColor = lerp(s, factor, _Fade*0.2);
return finalColor;
}
ENDCG
}
}
}
