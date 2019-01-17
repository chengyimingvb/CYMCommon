Shader "Hidden/TerrainEngine/MaskPaint/Standard-Base" {
	Properties {		
		_Mask("Mask", 2D) = "Black" {}
    _Brush("Brush", 2D) = "white" {}
    _BrushSize("BrushSize", Range(0,10)) = 1.0
    _BrushPosition("BrushPosition", Vector) = (0,0,0,0)
    _BrushStrength("BrushStrength", float) = 0.2
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}

	SubShader {
		Tags {
			"RenderType" = "Opaque"
			"Queue" = "Geometry-100"
		}
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0
		// needs more than 8 texcoords
		#pragma exclude_renderers gles
		#include "UnityPBSLighting.cginc"

		struct Input {
			float2 uv_Mask;
		};

    sampler2D _Mask;
    sampler2D _Brush;
    float4 _BrushPosition;
    float _BrushSize;
    float _BrushStrength;
    float _Glossiness;
		float _Metallic;
    
		void surf (Input IN, inout SurfaceOutputStandard o) {
      
      float2 brushMin = _BrushPosition - _BrushSize.xx * 0.5f;
      float2 pixPos = (IN.uv_Mask - brushMin) / _BrushSize.xx;
      
      float brushBorder = step(pixPos.x, 1) * step(0, pixPos.x) *
                          step(pixPos.y, 1) * step(0, pixPos.y);
      
      float brushValue = tex2D(_Brush, pixPos).r * _BrushStrength * brushBorder;
      float maskValue = tex2D(_Mask, IN.uv_Mask).r;
      
      float finalValue = saturate(maskValue + brushValue);
      fixed3 terrainColor = fixed3(0,0,0);
      
      o.Albedo = lerp(terrainColor, fixed3(1,1,1), finalValue);
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = 1;
		}

		ENDCG
	}

	FallBack "Diffuse"
}
