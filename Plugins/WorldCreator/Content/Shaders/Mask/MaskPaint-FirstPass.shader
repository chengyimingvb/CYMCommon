Shader "Hidden/WorldCreator/MaskPaint" {
	Properties {
		_Mask("Mask", 2D) = "Black" {}
    _Brush("Brush", 2D) = "white" {}
    _BrushSize("BrushSize", Range(0,10)) = 1.0
    _BrushPosition("BrushPosition", Vector) = (0,0,0,0)
    _BrushStrength("BrushStrength", float) = 0.2
    _BrushRotation("BrushRotation", float) = 0.0
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
    
	}

	SubShader {
		Tags {
			"Queue" = "Geometry-100"
			"RenderType" = "Opaque"
		}

		CGPROGRAM
		#pragma surface surf Standard vertex:SplatmapVert finalcolor:SplatmapFinalColor finalgbuffer:SplatmapFinalGBuffer fullforwardshadows
		#pragma multi_compile_fog
		#pragma target 3.0
		// needs more than 8 texcoords
		#pragma exclude_renderers gles
		#include "UnityPBSLighting.cginc"

		#pragma multi_compile __ _TERRAIN_NORMAL_MAP

		#define TERRAIN_STANDARD_SHADER
		#define TERRAIN_SURFACE_OUTPUT SurfaceOutputStandard
		#include "MaskPaintTerrainCommon.cginc"

    sampler2D _Mask;
    sampler2D _Brush;
    float4 _BrushPosition;
    float _BrushRotation;
    float _BrushSize;
    float _BrushStrength;
    
		void surf (Input IN, inout SurfaceOutputStandard o) {
      
      float2 brushMin = _BrushPosition - _BrushSize.xx * 0.5f;
      float2 pixPos = (IN.uv_Mask - brushMin) / _BrushSize.xx;
      
      pixPos -= 0.5f;
      float cR = cos(_BrushRotation);
      float sR = sin(_BrushRotation);
      pixPos = float2(pixPos.x * cR - pixPos.y * sR, pixPos.x * sR + pixPos.y * cR);
      pixPos += 0.5f;
      
      float brushBorder = step(pixPos.x, 1.0f) * step(0.0f, pixPos.x) *
                          step(pixPos.y, 1.0f) * step(0.0f, pixPos.y);
      

      float brushValue = tex2D(_Brush, pixPos).r * _BrushStrength * brushBorder;
      float maskValue = tex2D(_Mask, IN.uv_Mask).r;
      
      float finalValue = saturate(maskValue + brushValue);
      fixed3 terrainColor = fixed3(1,1,1);
      
      o.Albedo = lerp(terrainColor, fixed3(1,0,0), finalValue);
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = 1;
		}
		ENDCG
	}

  Dependency "AddPassShader" = "Hidden/TerrainEngine/MaskPaint/Standard-AddPass"
	Dependency "BaseMapShader" = "Hidden/TerrainEngine/MaskPaint/Standard-Base"
}
