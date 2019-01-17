    Shader "Hidden/HeatMapTess" {
        Properties {
            _Tess ("Tessellation", Range(1,32)) = 15
            _Displacement ("Displacement", Range(0, 1.0)) = 0.125
            _Color ("Color", color) = (1,1,1,0)
            _MainTex("Texture", 2D) = "black" { }
            _SpecColor ("Spec color", color) = (0.5,0.5,0.5,0.5)
            _TextureSize ("Texture size", Range(2,8192)) = 1024
            _NormalStrength ("Normal strength", Range(0,1)) = 0.06
            _BasicColor ("Color", Color) = (1,1,1,1)
            _HeatColor ("Color", Color) = (1,0,0,1)

            _HeatMap("HeatMap", 2D) = "Black" {}
        }
        SubShader {
            Tags { "RenderType"="Opaque" "PreviewType"="Plane" }
            LOD 300
            
            
            CGPROGRAM
            #pragma surface surf BlinnPhong addshadow fullforwardshadows vertex:disp tessellate:tess nolightmap
            #pragma target 5.0
            #include "Tessellation.cginc"

            
            struct appdata {
                float4 vertex : POSITION;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
            };

            
            float _Tess;
            float4 tess () {
                return _Tess;
            }

            float _TextureSize;
            float _Displacement;
            float _NormalStrength;
            fixed4 _BasicColor;
            fixed4 _HeatColor;

            sampler2D _HeatMap;

            #ifdef SHADER_API_D3D11            
            StructuredBuffer<float> _DispBuffer; 
    
            float GetHeight(float2 uv)
            {
              float2 start = uv * (_TextureSize - 1);
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
            
            void disp (inout appdata v)
            {
              v.texcoord.xy = float2(1.0f, 1.0f) - v.texcoord.xy;
              float d = GetHeight(v.texcoord.xy) * _Displacement;
              v.vertex.xyz += v.normal * d;
              v.normal = GetNormal(v.texcoord.xy, float2(1.0f,1.0f) / _TextureSize);
            }

            struct Input 
            {
                float2 uv_MainTex;
            };

            fixed4 _Color;

            void surf (Input IN, inout SurfaceOutput o) {
                float2 uv = IN.uv_MainTex;

                o.Albedo = lerp(_BasicColor, _HeatColor, tex2D(_HeatMap, uv).r);
                
                o.Specular = 0.0;
                o.Gloss = 0.0;                
            }

            ENDCG
        }
        FallBack "Diffuse"
    }