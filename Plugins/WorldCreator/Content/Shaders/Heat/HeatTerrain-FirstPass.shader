Shader "WorldCreator/Terrain/HeatTerrain" {
	Properties {
		_BasicColor ("Color", Color) = (1,1,1,1)
    _HeatColor ("Color", Color) = (1,0,0,1)
		_HeatTex ("Albedo (RGB)", 2D) = "white" {}
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
		#include "HeatMapTerrainCommon.cginc"

		void surf (Input IN, inout SurfaceOutputStandard o) {

      float strength = tex2D (_HeatTex, IN.uv_HeatTex).r;
			fixed4 c = lerp(_BasicColor, _HeatColor, strength);
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = 1;
		}
		ENDCG
	}

  Dependency "AddPassShader" = "Hidden/TerrainEngine/HeatTerrain/Standard-AddPass"
	Dependency "BaseMapShader" = "Hidden/TerrainEngine/HeatTerrain/Standard-Base"
}
