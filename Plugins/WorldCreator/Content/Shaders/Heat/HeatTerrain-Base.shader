Shader "Hidden/TerrainEngine/HeatTerrain/Standard-Base" {
	Properties {
		_BasicColor ("Color", Color) = (1,1,1,1)
    _HeatColor ("Color", Color) = (1,0,0,1)
		_HeatTex ("Albedo (RGB)", 2D) = "white" {}
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

    sampler2D _HeatTex;
    half _Glossiness;
    half _Metallic;
    fixed4 _BasicColor;
    fixed4 _HeatColor;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) {
      float strength = tex2D (_HeatTex, IN.uv_MainTex).r;
			fixed4 c = lerp(_BasicColor, _HeatColor, strength);
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = 1;
		}

		ENDCG
	}

	FallBack "Diffuse"
}
