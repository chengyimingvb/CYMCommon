// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "WorldCreator/HeatTiled"
{
  Properties
  {
    // Texture Distribution
    _DistributionTexture("DistributionMap", 2D) = "white" {}
  
    // Heightmap
    _HeightMap0("HeightMap0", 2D) = "black" {} // 0 | 0
    _HeightMap1("HeightMap1", 2D) = "black" {} //-1 |-1
    _HeightMap2("HeightMap2", 2D) = "black" {} // 0 |-1
    _HeightMap3("HeightMap3", 2D) = "black" {} // 1 |-1
    _HeightMap4("HeightMap4", 2D) = "black" {} //-1 | 0
    _HeightMap5("HeightMap5", 2D) = "black" {} // 1 | 0
    _HeightMap6("HeightMap6", 2D) = "black" {} //-1 | 1
    _HeightMap7("HeightMap7", 2D) = "black" {} // 0 | 1
    _HeightMap8("HeightMap8", 2D) = "black" {} // 1 | 1

    _HeightMapSize("HeightMapSize", Int) = 0
    
    _EnabledHeightMaps0("EnabledHeightMaps0", Vector) = (0,0,0,0)
    _EnabledHeightMaps1("EnabledHeightMaps1", Vector) = (0,0,0,0)

    // Color Pick Texture
    _ColorPickMap("ColorPickMap", 2D) = "black" {}
    _ColorPickColor("ColorPickColor", Vector) = (0,0,0,1)
    _ColorPickThreshold("ColorPickThreshold", Float) = 1
    _ColorPickSmoothness("ColorPickSmoothness", Float) = 0

    // Mask
    _MaskTexture("Mask", 2D) = "white" {}
    _MaskInv("Mask Invert", Float) = 0
    _MaskTreshold("Mask Treshold", Float) = 0
    _MaskContrast("Mask Contrast", Float) = 0

    // Height Noise
    _HeightNoiseTexture("HeightNoise", 2D) = "black" {}
    _HeightNoiseScale("HeightNoiseScale", Float) = 0

    // Terrain Height
    _MinHeight("MinHeight", Float) = 0
    _MaxHeight("MaxHeight", Float) = 1
    _HeightSmoothnessTop("HeightSmoothnessTop", Float) = 0
    _HeightSmoothnessBottom("HeightSmoothnessBottom", Float) = 0

    // Sea Level
    _SeaLevelSign("SeaLevelSign", Int) = 0
    _SeaLevel("SeaLevel", Float) = 0

    // Sun
    _SunDirection("SunDirection", Vector) = (0,0,0,0)
    _SunEnabled("SunEnabled", Int) = 0
    _SunSmoothness("SunSmoothness", Float) = 1

    // Slope
    _MinSlope("MinSlope", float) = 0
    _MaxSlope("MaxSlope", float) = 90
    _SlopeDiv("SlopeDiv", float) = 0
    _SlopeMul("SlopeMul", float) = 0
    _SlopeSmoothnessTop("SlopeSmoothnessTop", float) = 0.1
    _SlopeSmoothnessBottom("SlopeSmoothnessBottom", float) = 0.1

    // Weight
    _WeightMul("WeightMultiplicator", float) = 1.0

    // Cavity
    _CavityType("CavityType", Int) = 0 // 0 == None | 1 == Convex | -1 == Concave
    _Steps("Steps", Int) = 5
    _StepSize("StepSize", Float) = 10
    _Strength("Strength", Float) = 1
  }
    SubShader
    {
      // No culling or depth
      Cull Off ZWrite Off ZTest Always

      Pass
      {
        CGPROGRAM
        #pragma target 3.0
        #pragma vertex vert
        #pragma fragment frag

        #include "UnityCG.cginc"

        struct appdata
        {
          float4 vertex : POSITION;
          float2 uv : TEXCOORD0;
        };

        struct v2f
        {
          float2 uv : TEXCOORD0;
          float4 vertex : SV_POSITION;
        };
        
        // Distribution
        sampler2D _DistributionTexture;

        // Heightmap
        sampler2D _HeightMap0;
        sampler2D _HeightMap1;
        sampler2D _HeightMap2;
        sampler2D _HeightMap3;
        sampler2D _HeightMap4;
        sampler2D _HeightMap5;
        sampler2D _HeightMap6;
        sampler2D _HeightMap7;
        sampler2D _HeightMap8;
        int _HeightMapSize;
        
        float4 _EnabledHeightMaps0;
        float4 _EnabledHeightMaps1;

        // Color picking
        sampler2D _ColorPickMap;
        float4 _ColorPickColor;
        float _ColorPickThreshold;
        float _ColorPickSmoothness;

        // Mask
        sampler2D _MaskTexture;
        float _MaskInv; 
        float _MaskTreshold;
        float _MaskContrast;

        // Height Noise
        sampler2D _HeightNoiseTexture;
        float _HeightNoiseScale;

        // Terrain Height
        float _MinHeight;
        float _MaxHeight;
        float _HeightSmoothnessTop;
        float _HeightSmoothnessBottom;

        // Sea Level
        int _SeaLevelSign;
        float _SeaLevel;

        // Sun
        float3 _SunDirection;
        int _SunEnabled;
        float _SunSmoothness;

        // Slope
        float _MinSlope;
        float _MaxSlope;
        float _SlopeSmoothnessTop;
        float _SlopeSmoothnessBottom;
        float _SlopeDiv;
        float _SlopeMul;

        // Weight
        float _WeightMul;

        // Cavity
        int _CavityType;
        int _Steps;
        float _StepSize;
        float _Strength;

        // Helper Functions
        ///////////////////
        float GetHeight(float2 uv)
        { 
          if (uv.y < 0.0f)
          {
            if (uv.x < 0.0f && _EnabledHeightMaps1[1] > 0.5f)
              return tex2Dlod(_HeightMap6, float4(1.0f + uv.x, 1.0f + uv.y, 0, 0));
            else if(uv.x > 1.0f && _EnabledHeightMaps1[3] > 0.5f)
              return tex2Dlod(_HeightMap8, float4(uv.x - 1.0f, 1.0f + uv.y, 0, 0));
            else if(_EnabledHeightMaps1[2] > 0.5f)
              return tex2Dlod(_HeightMap7, float4(uv.x, 1.0f + uv.y, 0, 0));
          }
          else if (uv.y > 1.0f)
          {
            if (uv.x < 0.0f && _EnabledHeightMaps0[0] > 0.5f)
              return tex2Dlod(_HeightMap1, float4(1.0f + uv.x, uv.y - 1.0f, 0, 0));
            else if (uv.x > 1.0f  && _EnabledHeightMaps0[2] > 0.5f)
              return tex2Dlod(_HeightMap3, float4(uv.x - 1.0f, uv.y - 1.0f, 0, 0));
            else if(_EnabledHeightMaps0[1] > 0.5f)
              return tex2Dlod(_HeightMap2, float4(uv.x, uv.y - 1.0f, 0, 0));
          }
          else
          {
            if (uv.x < 0.0f && _EnabledHeightMaps0[3] > 0.5f)
              return tex2Dlod(_HeightMap4, float4(1.0f + uv.x, uv.y, 0, 0));
            if (uv.x > 1.0f && _EnabledHeightMaps1[0] > 0.5f)
              return tex2Dlod(_HeightMap5, float4(uv.x - 1.0f, uv.y, 0, 0));        
          }
          return tex2Dlod(_HeightMap0, float4(uv.x, uv.y, 0, 0));
        }

        float GetSlope(float2 uv, float height)
        {
          const float pixSize = 1.0f / _HeightMapSize;
          
          return (float)(atan(abs(GetHeight(uv + float2(0.0f,  pixSize)).r - height) * _SlopeMul) * _SlopeDiv +
                         atan(abs(GetHeight(uv + float2(0.0f, -pixSize)).r - height) * _SlopeMul) * _SlopeDiv +
                         atan(abs(GetHeight(uv + float2( pixSize, 0.0f)).r - height) * _SlopeMul) * _SlopeDiv +
                         atan(abs(GetHeight(uv + float2(-pixSize, 0.0f)).r - height) * _SlopeMul) * _SlopeDiv) * 0.25f;
        }
        
        float3 GetNormal(float2 uv)
        {
          const float pixSize = 1.0f / _HeightMapSize;

          float l = GetHeight(uv + float2(-pixSize, 0.0f));
          float r = GetHeight(uv + float2( pixSize, 0.0f));
          float b = GetHeight(uv + float2(0.0f, pixSize));
          float t = GetHeight(uv + float2(0.0f,-pixSize));          
          
          const float NormalStrength = 50.0f;

          return normalize(float3(l-r,1.0f /NormalStrength,t-b));
        }

        float GetCavity(float2 uv, float height)
        {
          const float pixSize = (1.0f / _HeightMapSize) * _StepSize;

          float h = 0.0f;
          float thisH = height;
          const int count = (_Steps * 2 + 1) * (_Steps * 2 + 1);
          
          for (int i = -_Steps; i <= _Steps; i++)
          {
            for (int j = -_Steps; j <= _Steps; j++)
            {
              float2 offPos = uv + float2(j * pixSize, i * pixSize);
              h += GetHeight(offPos).r;
            }
          }

          float areaHeight = (h / count);
          float heightOff = thisH - areaHeight;

          return saturate(heightOff * _CavityType * _Strength * 5) ;
        }

        v2f vert(appdata v)
        {
          v2f o;
          o.vertex = UnityObjectToClipPos(v.vertex);
          o.uv = v.uv;
          return o;
        }

        fixed4 frag(v2f input) : SV_Target
        {
          float height = GetHeight(input.uv);
          float slope = GetSlope(input.uv, height);
          float weight = 0.0f;
          float slopeWeight = 1.0f;

          // Calculate height noise
          /////////////////////////
          float heightNoise = tex2D(_HeightNoiseTexture, input.uv).r * _HeightNoiseScale;
          height = saturate(height + heightNoise);

          // Calculate height weight
          //////////////////////////
          float minOff, maxOff;
          minOff = (height - _MinHeight) / -_HeightSmoothnessBottom;
          maxOff = (_MaxHeight - height) / -_HeightSmoothnessTop;
          minOff = saturate(1.0f - minOff);
          maxOff = saturate(1.0f - maxOff);
          float heightWeight = minOff * maxOff;

          // Calculate Sea Level
          //////////////////////
          if (_SeaLevelSign == 1)
            heightWeight *= step(_SeaLevel, height);
          else if(_SeaLevelSign == -1)
            heightWeight *= step(height, _SeaLevel);

          if (_SunEnabled == 1)
          {
            // Calculate sun direction weight
            /////////////////////////////////
            float sunDot = saturate(dot(GetNormal(input.uv), normalize(-_SunDirection)));
            slopeWeight = pow(sunDot, _SunSmoothness);
          }
          else
          {
            // Calculate slope weight
            /////////////////////////            
            if (max(abs(_SlopeSmoothnessTop), abs(_SlopeSmoothnessBottom)) > 0.01f)
            {
              minOff = ((slope - _MinSlope) / 90.0f) / -_SlopeSmoothnessBottom;
              maxOff = ((_MaxSlope - slope) / 90.0f) / -_SlopeSmoothnessTop;
              
              minOff = saturate(1.0f - minOff);
              maxOff = saturate(1.0f - maxOff);
            }
            else
            {
              minOff = step(_MinSlope, slope);
              maxOff = step(slope, _MaxSlope);
            }

            slopeWeight = minOff * maxOff;        
          }

          // Calculate Cavity
          ///////////////////
          float cavityWeight = 1.0f;
          if(_CavityType != 0)
            cavityWeight = GetCavity(input.uv, height);

          // Apply Mask
          /////////////
          float maskWeight = saturate((abs(_MaskInv - tex2D(_MaskTexture, input.uv)) - _MaskTreshold) / (1.0f - _MaskTreshold));
          maskWeight = saturate((maskWeight - 0.5f) * _MaskContrast + 0.5f);
          
          // Apply Distribution
          ///////////////
          float distribution = tex2D(_DistributionTexture, input.uv).r;
          
          // Apply Color Picking          
          float3 color = tex2D(_ColorPickMap, input.uv).rgb;
          float3 off = abs(color - _ColorPickColor.xyz);
          off = smoothstep(_ColorPickThreshold.xxx, 0, off) * step(off, _ColorPickThreshold.xxx);
          float picking = saturate(min(min(off.x, off.y), off.z) / clamp(0.001f, 1.0f, _ColorPickSmoothness));

          // Calculate final weight
          /////////////////////////
          float result = heightWeight * slopeWeight * cavityWeight * maskWeight * _WeightMul * distribution * picking;

          return fixed4(saturate(result),0,0,1);
        }
        ENDCG
      }
    }
}
