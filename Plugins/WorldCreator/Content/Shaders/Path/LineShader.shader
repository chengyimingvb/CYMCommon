Shader "Custom/LineShader"
{
  Properties
  {
    _Size("Size", Range(0, 100)) = 3
    _Smoothness("Smoothness", Range(0, 1)) = 0.5
  }

  SubShader
  {
    Pass
    {
      Tags { "RenderType" = "Opaque" }
      LOD 200
      Cull Off
      Blend Off
      ZWrite Off
      ZTest Less

      CGPROGRAM
      #pragma target 2.0
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
      };


      // **************************************************************
      // Vars															*
      // **************************************************************

      matrix _World;
      float _Size;
      float _Smoothness;
      float4x4 _VP;

      // **************************************************************
      // Shader Programs												*
      // **************************************************************

      // Vertex Shader ------------------------------------------------
      FS_INPUT VS_Main(Appdata v)
      {
        FS_INPUT output = (FS_INPUT)0;

        v.vertex.y += 0.001f;
        output.pos = mul(UNITY_MATRIX_VP, mul(_World, float4(v.vertex.xyz, 1.0f)));
        output.uv = v.uv;
        return output;
      }

      // Fragment Shader -----------------------------------------------
      float4 FS_Main(FS_INPUT input) : COLOR
      {
        float off = abs(input.uv.y - 0.5f) * 2.0f;
        off = smoothstep(1.0f, clamp(0.0f, 0.999f, 1.0f - _Smoothness), off);
        return float4(off.xxx,1);
      }

      ENDCG
    }

    Pass
    {
      Tags{ "RenderType" = "Opaque" }
      LOD 200
      Cull Off
      Blend One One
      BlendOp RevSub
      ZWrite Off
      ZTest Greater

      CGPROGRAM
      #pragma target 2.0
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
      };


      // **************************************************************
      // Vars															*
      // **************************************************************

      matrix _World;
      float _Size;
      float _Smoothness;
      float4x4 _VP;

      // **************************************************************
      // Shader Programs												*
      // **************************************************************

      // Vertex Shader ------------------------------------------------
      FS_INPUT VS_Main(Appdata v)
      {
        FS_INPUT output = (FS_INPUT)0;

        v.vertex.y += 0.001f;
        output.pos = mul(UNITY_MATRIX_VP, mul(_World, float4(v.vertex.xyz, 1.0f)));
        output.uv = v.uv;
        return output;
      }

      // Fragment Shader -----------------------------------------------
      float4 FS_Main(FS_INPUT input) : COLOR
      {
        float off = abs(input.uv.y - 0.5f) * 2.0f;
        off = smoothstep(1.0f, clamp(0.0f, 0.999f, 1.0f - _Smoothness), off);
        return float4(off.xxx,1);
      }
      ENDCG
    }
  }
}
