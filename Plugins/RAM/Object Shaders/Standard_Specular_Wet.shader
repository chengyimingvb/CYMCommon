Shader "NatureManufacture Shaders/Water/Standard Specular Wet"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("MainTex ", 2D) = "white" {}
		_WetColor("Wet Color", Color) = (0.6691177,0.6691177,0.6691177,1)
		_SmoothnessPower("Smoothness Power", Range( 0 , 2)) = 1
		[NoScaleOffset]_BumpMap("BumpMap", 2D) = "bump" {}
		_BumpScale("BumpScale", Range( 0 , 2)) = 1
		[NoScaleOffset]_SpecularRGBSmoothnessA("Specular (RGB) Smoothness (A)", 2D) = "white" {}
		_SpecularPower("Specular Power", Range( 0 , 2)) = 1
		_WetSmoothness("Wet Smoothness", Range( 0 , 100)) = 0
		[NoScaleOffset]_AmbientOcclusionG("Ambient Occlusion (G)", 2D) = "white" {}
		_AmbientOcclusionPower("Ambient Occlusion Power", Range( 0 , 1)) = 1
		_DetailMask("DetailMask", 2D) = "white" {}
		_DetailAlbedoPower("Detail Albedo Power", Range( 0 , 2)) = 0
		_DetailAlbedoMap("DetailAlbedoMap", 2D) = "black" {}
		_DetailNormalMap("DetailNormalMap", 2D) = "bump" {}
		_DetailNormalMapScale("DetailNormalMapScale", Range( 0 , 5)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		ZTest LEqual
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf StandardSpecular keepalpha addshadow fullforwardshadows dithercrossfade 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform float _BumpScale;
		uniform sampler2D _BumpMap;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float _DetailNormalMapScale;
		uniform sampler2D _DetailNormalMap;
		uniform sampler2D _DetailAlbedoMap;
		uniform float4 _DetailAlbedoMap_ST;
		uniform sampler2D _DetailMask;
		uniform float4 _DetailMask_ST;
		uniform float4 _Color;
		uniform float _DetailAlbedoPower;
		uniform float4 _WetColor;
		uniform sampler2D _SpecularRGBSmoothnessA;
		uniform float _SpecularPower;
		uniform float _SmoothnessPower;
		uniform float _WetSmoothness;
		uniform sampler2D _AmbientOcclusionG;
		uniform float _AmbientOcclusionPower;

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float3 tex2DNode4 = UnpackScaleNormal( tex2D( _BumpMap, uv_MainTex ), _BumpScale );
			float2 uv_DetailAlbedoMap = i.uv_texcoord * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
			float3 normalizeResult202 = normalize( BlendNormals( tex2DNode4 , UnpackScaleNormal( tex2D( _DetailNormalMap, uv_DetailAlbedoMap ), _DetailNormalMapScale ) ) );
			float2 uv_DetailMask = i.uv_texcoord * _DetailMask_ST.xy + _DetailMask_ST.zw;
			float4 tex2DNode195 = tex2D( _DetailMask, uv_DetailMask );
			float3 lerpResult193 = lerp( tex2DNode4 , normalizeResult202 , tex2DNode195.a);
			o.Normal = lerpResult193;
			float4 temp_output_44_0 = ( tex2D( _MainTex, uv_MainTex ) * _Color );
			float4 blendOpSrc189 = temp_output_44_0;
			float4 blendOpDest189 = ( _DetailAlbedoPower * tex2D( _DetailAlbedoMap, uv_DetailAlbedoMap ) );
			float4 lerpResult192 = lerp( temp_output_44_0 , (( blendOpDest189 > 0.5 ) ? ( 1.0 - ( 1.0 - 2.0 * ( blendOpDest189 - 0.5 ) ) * ( 1.0 - blendOpSrc189 ) ) : ( 2.0 * blendOpDest189 * blendOpSrc189 ) ) , ( _DetailAlbedoPower * tex2DNode195.a ));
			float temp_output_261_0 = ( 1.0 - ( i.vertexColor / float4( 1,1,1,1 ) ).r );
			float4 lerpResult272 = lerp( lerpResult192 , ( lerpResult192 * _WetColor ) , temp_output_261_0);
			o.Albedo = lerpResult272.rgb;
			float4 tex2DNode29 = tex2D( _SpecularRGBSmoothnessA, uv_MainTex );
			o.Specular = ( tex2DNode29 * _SpecularPower ).rgb;
			float lerpResult269 = lerp( ( tex2DNode29.a * _SmoothnessPower ) , _WetSmoothness , temp_output_261_0);
			o.Smoothness = lerpResult269;
			float clampResult67 = clamp( tex2D( _AmbientOcclusionG, uv_MainTex ).g , ( 1.0 - _AmbientOcclusionPower ) , 1.0 );
			o.Occlusion = clampResult67;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}