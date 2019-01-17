// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/WorldCreator/TerrainPreview" {
    Properties 
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _TextureSize ("Texture size", Range(2,8192)) = 1024
        _NormalStrength ("Normal strength", Range(0,100)) = 25
    }
    SubShader {
        Pass {
            CGPROGRAM

            
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #include "UnityCG.cginc"
            
            int _TextureSize;
            float _NormalStrength;
            
            struct vertexInput 
            {
              float4 vertex : POSITION;
              float4 normal : NORMAL;
              float4 texcoord0 : TEXCOORD0;
            };
            
            struct fragmentInput
            {
              float4 vertex : SV_POSITION;
              float4 uv : TEXCOORD0;
            };
            
            uniform sampler2D _MainTex;

#ifdef SHADER_API_D3D11     
            StructuredBuffer<float> _DispBuffer;            

            float GetHeight(float2 uv)
            {
              float2 start = uv * _TextureSize;
              float2 offset = start - floor(start);
              
              int2 leftTop = (int2)start;
              int2 rightTop = clamp(start + int2(1, 0), 0, _TextureSize - 1);
              int2 leftBot = clamp (start + int2(0, 1), 0, _TextureSize - 1);
              int2 rightBot = clamp(start + int2(1, 1), 0, _TextureSize - 1);
              
              float leftTopV  = _DispBuffer[leftTop.y * _TextureSize + leftTop.x];
              float rightTopV = _DispBuffer[rightTop.y * _TextureSize + rightTop.x];
              float leftBotV  = _DispBuffer[leftBot.y * _TextureSize + leftBot.x];
              float rightBotV = _DispBuffer[rightBot.y * _TextureSize + rightBot.x];
              
              return lerp(lerp(leftTopV, rightTopV, offset.x), lerp(leftBotV, rightBotV, offset.x), offset.y);
            }
            
            
            float3 GetNormal(float2 uv, float2 texelSize)
            {
              float l = GetHeight(uv + texelSize * float2(-1,  0));
              float r = GetHeight(uv + texelSize * float2( 1,  0));
              float t = GetHeight(uv + texelSize * float2( 0, -1));
              float b = GetHeight(uv + texelSize * float2( 0,  1));
              
              return normalize(float3(l - r, b - t, 1.0f / _NormalStrength * texelSize.x));
            }
#else
            float GetHeight(float2 uv) { return 0; }
            float3 GetNormal(float2 uv, float2 texelSize) { return 0; }
#endif
            
            fragmentInput vert(vertexInput v) 
            {
              fragmentInput o;
              o.vertex = UnityObjectToClipPos (v.vertex + float4(v.normal.xyz, 0.0f) * GetHeight(v.texcoord0) * 0.125f);
              o.uv = v.texcoord0;
              
              return o;
            }

            fixed4 frag(fragmentInput i) : SV_Target 
            {
              #ifdef SHADER_API_D3D11
              float3 n = GetNormal(i.uv, float2(1.0f, 1.0f) / _TextureSize);
              return fixed4(dot(n, float3(0,1,0)).xxx,1);
              #endif
              
            }

            ENDCG
        }
    }
}