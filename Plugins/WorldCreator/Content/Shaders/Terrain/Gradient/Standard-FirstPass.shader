// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "WorldCreator/Terrain/Gradient" {
	Properties {
		// set by terrain engine
		[HideInInspector] _Control ("Control (RGBA)", 2D) = "red" {}
    [HideInInspector]_Gradient ("Gradient (RGB)", 2D) = "white" {}
    [HideInInspector]_TerrainHeight ("Terrain Height", Range(0.0, 1000.0)) = 1.0	
    
		// used in fallback on old cards & base map
		[HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
		[HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)
	}

	SubShader {
		Tags {
			"SplatCount" = "4"
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
    
    sampler2D _Gradient;
    float _TerrainHeight;
    
    struct Input
    {
      UNITY_FOG_COORDS(0)
      float3 localPos;
    };
    
    void SplatmapVert(inout appdata_full v, out Input data)
    {
      UNITY_INITIALIZE_OUTPUT(Input, data);
      float4 pos = UnityObjectToClipPos (v.vertex);
      UNITY_TRANSFER_FOG(data, pos);
      data.localPos = v.vertex.xyz;
    
    #ifdef _TERRAIN_NORMAL_MAP
      v.tangent.xyz = cross(v.normal, float3(0,0,1));
      v.tangent.w = -1;
    #endif
    }

		void surf (Input IN, inout SurfaceOutputStandard o) {    
			o.Albedo = tex2D(_Gradient, float2(IN.localPos.y / _TerrainHeight, 0.5f));
			o.Alpha = 1.0f;
			o.Smoothness = 0.0f;
			o.Metallic = 0.0f;
		}
    
    void SplatmapFinalColor(Input IN, TERRAIN_SURFACE_OUTPUT o, inout fixed4 color)
    {
      color *= o.Alpha;
      #ifdef TERRAIN_SPLAT_ADDPASS
        UNITY_APPLY_FOG_COLOR(IN.fogCoord, color, fixed4(0,0,0,0));
      #else
        UNITY_APPLY_FOG(IN.fogCoord, color);
      #endif
    }
    
    void SplatmapFinalPrepass(Input IN, TERRAIN_SURFACE_OUTPUT o, inout fixed4 normalSpec)
    {
      normalSpec *= o.Alpha;
    }
    
    void SplatmapFinalGBuffer(Input IN, TERRAIN_SURFACE_OUTPUT o, inout half4 diffuse, inout half4 specSmoothness, inout half4 normal, inout half4 emission)
    {
      diffuse.rgb *= o.Alpha;
      specSmoothness *= o.Alpha;
      normal.rgb *= o.Alpha;
      emission *= o.Alpha;
    }
    
		ENDCG
	}
  
	Dependency "AddPassShader" = "Hidden/WC/Splatmap/Standard-AddPass"
	Dependency "BaseMapShader" = "Hidden/WC/Splatmap/Standard-Base"

	Fallback "Nature/Terrain/Diffuse"
}
