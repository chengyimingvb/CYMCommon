// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Hidden/WC/Splatmap/Standard-Base" {
	Properties {
    [HideInInspector]_Gradient ("Gradient (RGB)", 2D) = "white" {}
    [HideInInspector]_TerrainHeight ("Terrain Height", Range(0.0, 1000.0)) = 1.0	
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

    sampler2D _Gradient;
    float _TerrainHeight;
    
    struct Input
    {
      UNITY_FOG_COORDS(0)
      float3 worldPos;
    };
    
		void surf (Input IN, inout SurfaceOutputStandard o) {
      float height = mul(unity_WorldToObject, float4(IN.worldPos, 1.0f)).y;
			o.Albedo = tex2D(_Gradient, float2(height / _TerrainHeight, 0.5f));
			o.Alpha = 1.0f;
			o.Smoothness = 0.0f;
			o.Metallic = 0.0f;
		}

		ENDCG
	}

	FallBack "Diffuse"
}
