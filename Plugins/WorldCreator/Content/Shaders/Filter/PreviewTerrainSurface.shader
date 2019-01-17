Shader "Custom/WorldCreator/PreviewTerrainSurface" {
    Properties 
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _TextureSize ("Texture size", Range(2,8192)) = 1024
        _NormalStrength ("Normal strength", Range(0,100)) = 25
    }
    SubShader {
            Tags { "RenderType"="Opaque" }
            LOD 300
            
            CGPROGRAM

            #pragma surface frag BlinnPhong vertex:vert addshadow fullforwardshadows nolightmap
            #pragma target 5.0

            #include "UnityCG.cginc"
            
            int _TextureSize;
            float _NormalStrength; 
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
            
            struct Input 
            {
              float2 uv_MainTex;
            };
            
            struct appdata 
            {
                float4 vertex : POSITION;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
            };        
            
            void vert(inout appdata v) 
            {
              v.vertex.xyz += v.normal * GetHeight(v.texcoord) * 0.125f;
            }

            void frag(Input i, inout SurfaceOutput o)
            {
              #ifdef SHADER_API_D3D11
              float3 n = GetNormal(i.uv_MainTex, float2(1.0f, 1.0f) / _TextureSize);
              o.Normal = n;
              
              #endif          
              o.Albedo = fixed4(1,1,1,1);
              
            }

            ENDCG
        }
    FallBack "Diffuse"
}