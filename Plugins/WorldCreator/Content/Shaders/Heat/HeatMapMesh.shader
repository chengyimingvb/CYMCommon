Shader "Custom/HeatMapMesh" {
	Properties {
		_BasicColor ("Color", Color) = (1,1,1,1)
    _HeatColor ("Color", Color) = (1,0,0,1)
		_HeatTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _HeatTex;

		struct Input {
			float2 uv_HeatTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _BasicColor;
		fixed4 _HeatColor;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
      
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
	FallBack "Diffuse"
}
