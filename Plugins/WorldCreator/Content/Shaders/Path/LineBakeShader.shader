Shader "Custom/LineBakeShader"
{
  Properties
  {
    _Size("Size", Range(0, 1000)) = 100
    _AreaStart("_AreaStart", Vector) = (0,0,0)
    _AreaEnd("_AreaEnd", Vector) = (1000,0,1000)
    _Smoothness("Smoothness", Range(0, 1)) = 0.5
    _Strength("Strength", Range(0, 1)) = 1
  }

    SubShader
  {
    Pass
  {
    Tags{ "RenderType" = "Transparent" }
    LOD 200
    Cull Off
    Blend One Zero, One Zero
    BlendOp Add, Add
    

    CGPROGRAM
#pragma target 3.0
#pragma vertex VS_Main
#pragma fragment FS_Main
#include "UnityCG.cginc" 

    // **************************************************************
    // Data structures												*
    // **************************************************************
    struct Appdata
  {
    float4 vertex : POSITION;
    float2 uv     : TEXCOORD0;
  };

  struct FS_INPUT
  {
    float4	pos		: POSITION;
    float2	uv    : TEXCOORD;
    float   depth : DEPTH;
  };


  // **************************************************************
  // Vars															*
  // **************************************************************

  float _Size;
  float _Smoothness;
  float _Strength;
  float3 _AreaStart;
  float3 _AreaEnd;

  // **************************************************************
  // Shader Programs												*
  // **************************************************************

  // Vertex Shader ------------------------------------------------
  FS_INPUT VS_Main(Appdata v)
  {
    FS_INPUT output = (FS_INPUT)0;

    output.pos = float4(v.vertex.xz * 2.0f - 1.0f, 0.0f, 1.0f);
    
#if UNITY_UV_STARTS_AT_TOP
      output.pos.y *= -1;
#endif
    output.uv = v.uv;
    output.depth = v.vertex.y;

    return output;
  }


  // Fragment Shader -----------------------------------------------
  float4 FS_Main(FS_INPUT input) : COLOR
  {    
    float off = abs(input.uv.y - 0.5f) * 2.0f;
    off = smoothstep(1.0f, clamp(0.0f, 0.999f, 1.0f - _Smoothness), off);

    return float4(input.depth.xxx,off * _Strength);
  }

    ENDCG
  }
  }
}
