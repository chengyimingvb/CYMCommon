Shader "NatureManufacture Shaders/Water/Water River Tesseled"
{
	Properties
	{
		_GlobalTiling("Global Tiling", Range( 0.001 , 100)) = 1
		_UVVDirection1UDirection0("UV - V Direction (1) U Direction (0)", Int) = 0
		_WaterMainSpeed("Water Main Speed", Vector) = (0.01,0,0,0)
		_WaterMixSpeed("Water Mix Speed", Vector) = (0.01,0.05,0,0)
		_SmallCascadeMainSpeed("Small Cascade Main Speed", Vector) = (0,0.08,0,0)
		_SmallCascadeMixSpeed("Small Cascade Mix Speed", Vector) = (0.04,0.08,0,0)
		_BigCascadeMainSpeed("Big Cascade Main Speed", Vector) = (0,0.24,0,0)
		_BigCascadeMixSpeed("Big Cascade Mix Speed", Vector) = (0.02,0.28,0,0)
		_WaterDepth("Water Depth", Range( 0 , 1)) = 0
		_ShalowFalloff("Shalow Falloff", Float) = 0
		_ShalowDepth("Shalow Depth", Range( 0 , 10)) = 0.7
		_ShalowColor("Shalow Color", Color) = (1,1,1,0)
		_DeepColor("Deep Color", Color) = (0,0,0,0)
		_WaterDeepTranslucencyPower("Water Deep Translucency Power", Range( 0 , 10)) = 1
		_WaterShalowTranslucencyPower("Water Shalow Translucency Power", Range( 0 , 10)) = 1
		_WaterSpecularClose("Water Specular Close", Range( 0 , 1)) = 0
		_WaterSpecularFar("Water Specular Far", Range( 0 , 1)) = 0
		_WaterSpecularThreshold("Water Specular Threshold", Range( 0 , 10)) = 1
		_WaterSmoothness("Water Smoothness", Float) = 0
		_Distortion("Distortion", Float) = 0.5
		_WaterFalloffBorder("Water Falloff Border", Range( 0 , 10)) = 0
		_WaterNormal("Water Normal", 2D) = "bump" {}
		_NormalScale("Normal Scale", Float) = 0
		[NoScaleOffset]_WaterTesselation("Water Tesselation", 2D) = "black" {}
		_WaterTessScale("Water Tess Scale", Float) = 0
		_SmallCascadeAngle("Small Cascade Angle", Range( 0.001 , 90)) = 90
		_SmallCascadeAngleFalloff("Small Cascade Angle Falloff", Range( 0 , 80)) = 5
		_SmallCascadeNormal("Small Cascade Normal", 2D) = "bump" {}
		_SmallCascadeNormalScale("Small Cascade Normal Scale", Float) = 0
		[NoScaleOffset]_SmallCascadeWaterTess("Small Cascade Water Tess", 2D) = "white" {}
		_SmallCascadeWaterTessScale("Small Cascade Water Tess Scale", Float) = 0
		[NoScaleOffset]_SmallCascade("Small Cascade", 2D) = "white" {}
		_SmallCascadeColor("Small Cascade Color", Vector) = (1,1,1,0)
		_SmallCascadeFoamFalloff("Small Cascade Foam Falloff", Range( 0 , 10)) = 0
		_SmallCascadeSmoothness("Small Cascade Smoothness", Float) = 0
		_SmallCascadeSpecular("Small Cascade Specular", Range( 0 , 1)) = 0
		_BigCascadeAngle("Big Cascade Angle", Range( 0.001 , 90)) = 90
		_BigCascadeAngleFalloff("Big Cascade Angle Falloff", Range( 0 , 80)) = 15
		_BigCascadeNormal("Big Cascade Normal", 2D) = "bump" {}
		_BigCascadeNormalScale("Big Cascade Normal Scale", Float) = 0
		[NoScaleOffset]_BigCascadeWaterTess("Big Cascade Water Tess", 2D) = "black" {}
		_BigCascadeWaterTessScale("Big Cascade Water Tess Scale", Float) = 0
		[NoScaleOffset]_BigCascade("Big Cascade", 2D) = "white" {}
		_BigCascadeColor("Big Cascade Color", Vector) = (1,1,1,0)
		_BigCascadeFoamFalloff("Big Cascade Foam Falloff", Range( 0 , 10)) = 0
		_BigCascadeTransparency("Big Cascade Transparency", Range( 0 , 1)) = 0
		_BigCascadeSmoothness("Big Cascade Smoothness", Float) = 0
		_BigCascadeSpecular("Big Cascade Specular", Range( 0 , 1)) = 0
		_Noise("Noise", 2D) = "white" {}
		_SmallCascadeNoisePower("Small Cascade Noise Power", Range( 0 , 10)) = 2.71
		_BigCascadeNoisePower("Big Cascade Noise Power", Range( 0 , 10)) = 2.71
		_NoiseSpeed("Noise Speed", Vector) = (-0.2,-0.5,0,0)
		_Foam("Foam", 2D) = "white" {}
		_FoamSpeed("Foam Speed", Vector) = (-0.001,0.018,0,0)
		_FoamColor("Foam Color", Vector) = (1,1,1,0)
		_FoamDepth("Foam Depth", Range( 0 , 10)) = 1
		_FoamFalloff("Foam Falloff", Range( -100 , 0)) = -10.9
		_FoamSpecular("Foam Specular", Range( 0 , 1)) = 0
		_FoamSmoothness("Foam Smoothness", Float) = 0
		_AOPower("AO Power", Range( 0 , 1)) = 1
		_EdgeLength ( "Edge length", Range( 2, 50 ) ) = 25
		_TessMaxDisp( "Max Displacement", Float ) = 20
		_TessPhongStrength( "Phong Tess Strength", Range( 0, 1 ) ) = 0.5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Geometry+999" "IsEmissive" = "true"  }
		Cull Back
		ZWrite On
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha , SrcAlpha OneMinusSrcAlpha
		BlendOp Add , Add
		GrabPass{ "_WaterGrab" }
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityCG.cginc"
		#include "Tessellation.cginc"
		#pragma target 4.6
		#pragma surface surf StandardSpecular keepalpha noshadow noinstancing vertex:vertexDataFunc tessellate:tessFunction tessphong:_TessPhongStrength 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldNormal;
			INTERNAL_DATA
			float4 screenPos;
		};

		uniform float _WaterTessScale;
		uniform int _UVVDirection1UDirection0;
		uniform float2 _WaterMixSpeed;
		uniform sampler2D _WaterNormal;
		uniform float4 _WaterNormal_ST;
		uniform float _GlobalTiling;
		uniform sampler2D _WaterTesselation;
		uniform float2 _WaterMainSpeed;
		uniform float2 _SmallCascadeMixSpeed;
		uniform sampler2D _SmallCascadeNormal;
		uniform float4 _SmallCascadeNormal_ST;
		uniform float _SmallCascadeWaterTessScale;
		uniform sampler2D _SmallCascadeWaterTess;
		uniform float2 _SmallCascadeMainSpeed;
		uniform half _SmallCascadeAngle;
		uniform float _SmallCascadeAngleFalloff;
		uniform half _BigCascadeAngle;
		uniform float _BigCascadeAngleFalloff;
		uniform float _BigCascadeWaterTessScale;
		uniform float2 _BigCascadeMixSpeed;
		uniform sampler2D _BigCascadeNormal;
		uniform float4 _BigCascadeNormal_ST;
		uniform sampler2D _BigCascadeWaterTess;
		uniform float2 _BigCascadeMainSpeed;
		uniform float _NormalScale;
		uniform float _SmallCascadeNormalScale;
		uniform float _BigCascadeNormalScale;
		uniform sampler2D _WaterGrab;
		uniform float _Distortion;
		uniform float4 _DeepColor;
		uniform float4 _ShalowColor;
		uniform sampler2D _CameraDepthTexture;
		uniform float _ShalowDepth;
		uniform float _ShalowFalloff;
		uniform float _BigCascadeTransparency;
		uniform float3 _FoamColor;
		uniform float _FoamDepth;
		uniform float _FoamFalloff;
		uniform sampler2D _Foam;
		uniform float2 _FoamSpeed;
		uniform float4 _Foam_ST;
		uniform sampler2D _SmallCascade;
		uniform float2 _NoiseSpeed;
		uniform sampler2D _Noise;
		uniform float4 _Noise_ST;
		uniform float _SmallCascadeNoisePower;
		uniform float3 _SmallCascadeColor;
		uniform float _SmallCascadeFoamFalloff;
		uniform sampler2D _BigCascade;
		uniform float _BigCascadeNoisePower;
		uniform float3 _BigCascadeColor;
		uniform float _BigCascadeFoamFalloff;
		uniform float _WaterDeepTranslucencyPower;
		uniform float _WaterShalowTranslucencyPower;
		uniform float _WaterSpecularFar;
		uniform float _WaterSpecularClose;
		uniform float _WaterSpecularThreshold;
		uniform float _FoamSpecular;
		uniform float _SmallCascadeSpecular;
		uniform float _BigCascadeSpecular;
		uniform float _WaterSmoothness;
		uniform float _FoamSmoothness;
		uniform float _SmallCascadeSmoothness;
		uniform float _BigCascadeSmoothness;
		uniform half _AOPower;
		uniform float _WaterDepth;
		uniform float _WaterFalloffBorder;
		uniform float _EdgeLength;
		uniform float _TessMaxDisp;
		uniform float _TessPhongStrength;


		inline float4 ASE_ComputeGrabScreenPos( float4 pos )
		{
			#if UNITY_UV_STARTS_AT_TOP
			float scale = -1.0;
			#else
			float scale = 1.0;
			#endif
			float4 o = pos;
			o.y = pos.w * 0.5f;
			o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
			return o;
		}


		float4 tessFunction( appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityEdgeLengthBasedTessCull (v0.vertex, v1.vertex, v2.vertex, _EdgeLength , _TessMaxDisp );
		}

		void vertexDataFunc( inout appdata_full v )
		{
			int Direction723 = _UVVDirection1UDirection0;
			float2 appendResult706 = (float2(_WaterMixSpeed.y , _WaterMixSpeed.x));
			float2 uv_WaterNormal = v.texcoord.xy * _WaterNormal_ST.xy + _WaterNormal_ST.zw;
			float Globaltiling821 = ( 1.0 / _GlobalTiling );
			float2 temp_output_824_0 = ( uv_WaterNormal * Globaltiling821 );
			float2 panner612 = ( _Time.y * (( (float)Direction723 == 1.0 ) ? _WaterMixSpeed :  appendResult706 ) + temp_output_824_0);
			float2 WaterSpeedValueMix516 = panner612;
			float2 appendResult705 = (float2(_WaterMainSpeed.y , _WaterMainSpeed.x));
			float2 panner611 = ( _Time.y * (( (float)Direction723 == 1.0 ) ? _WaterMainSpeed :  appendResult705 ) + temp_output_824_0);
			float2 WaterSpeedValueMain614 = panner611;
			float2 appendResult709 = (float2(_SmallCascadeMixSpeed.y , _SmallCascadeMixSpeed.x));
			float2 uv_SmallCascadeNormal = v.texcoord.xy * _SmallCascadeNormal_ST.xy + _SmallCascadeNormal_ST.zw;
			float2 temp_output_828_0 = ( uv_SmallCascadeNormal * Globaltiling821 );
			float2 panner597 = ( _Time.y * (( (float)Direction723 == 1.0 ) ? _SmallCascadeMixSpeed :  appendResult709 ) + temp_output_828_0);
			float2 SmallCascadeSpeedValueMix433 = panner597;
			float2 appendResult710 = (float2(_SmallCascadeMainSpeed.y , _SmallCascadeMainSpeed.x));
			float2 panner598 = ( _Time.y * (( (float)Direction723 == 1.0 ) ? _SmallCascadeMainSpeed :  appendResult710 ) + temp_output_828_0);
			float2 SmallCascadeSpeedValueMain600 = panner598;
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			float clampResult259 = clamp( ase_worldNormal.y , 0.0 , 1.0 );
			float temp_output_258_0 = ( _SmallCascadeAngle / 45.0 );
			float clampResult263 = clamp( ( clampResult259 - ( 1.0 - temp_output_258_0 ) ) , 0.0 , 2.0 );
			float clampResult584 = clamp( ( clampResult263 * ( 1.0 / temp_output_258_0 ) ) , 0.0 , 1.0 );
			float clampResult507 = clamp( ase_worldNormal.y , 0.0 , 1.0 );
			float temp_output_504_0 = ( _BigCascadeAngle / 45.0 );
			float clampResult509 = clamp( ( clampResult507 - ( 1.0 - temp_output_504_0 ) ) , 0.0 , 2.0 );
			float clampResult583 = clamp( ( clampResult509 * ( 1.0 / temp_output_504_0 ) ) , 0.0 , 1.0 );
			float clampResult514 = clamp( pow( ( 1.0 - clampResult583 ) , _BigCascadeAngleFalloff ) , 0.0 , 1.0 );
			float clampResult285 = clamp( ( pow( ( 1.0 - clampResult584 ) , _SmallCascadeAngleFalloff ) - clampResult514 ) , 0.0 , 1.0 );
			float lerpResult407 = lerp( ( ( _WaterTessScale * tex2Dlod( _WaterTesselation, float4( WaterSpeedValueMix516, 0, 1.0) ).a ) + ( _WaterTessScale * tex2Dlod( _WaterTesselation, float4( WaterSpeedValueMain614, 0, 1.0) ).a ) ) , ( ( ( tex2Dlod( _SmallCascadeWaterTess, float4( SmallCascadeSpeedValueMix433, 0, 1.0) ).a * _SmallCascadeWaterTessScale ) + ( tex2Dlod( _SmallCascadeWaterTess, float4( SmallCascadeSpeedValueMain600, 0, 0.0) ).a * _SmallCascadeWaterTessScale ) ) * clampResult285 ) , clampResult285);
			float2 appendResult712 = (float2(_BigCascadeMixSpeed.y , _BigCascadeMixSpeed.x));
			float2 uv_BigCascadeNormal = v.texcoord.xy * _BigCascadeNormal_ST.xy + _BigCascadeNormal_ST.zw;
			float2 temp_output_830_0 = ( uv_BigCascadeNormal * Globaltiling821 );
			float2 panner606 = ( _Time.y * (( (float)Direction723 == 1.0 ) ? _BigCascadeMixSpeed :  appendResult712 ) + temp_output_830_0);
			float2 BigCascadeSpeedValueMix608 = panner606;
			float2 appendResult714 = (float2(_BigCascadeMainSpeed.y , _BigCascadeMainSpeed.x));
			float2 panner607 = ( _Time.y * (( (float)Direction723 == 1.0 ) ? _BigCascadeMainSpeed :  appendResult714 ) + temp_output_830_0);
			float2 BigCascadeSpeedValueMain432 = panner607;
			float lerpResult568 = lerp( lerpResult407 , ( ( ( _BigCascadeWaterTessScale * tex2Dlod( _BigCascadeWaterTess, float4( BigCascadeSpeedValueMix608, 0, 1.0) ).a ) + ( _BigCascadeWaterTessScale * tex2Dlod( _BigCascadeWaterTess, float4( BigCascadeSpeedValueMain432, 0, 1.0) ).a ) ) * clampResult514 ) , clampResult514);
			float3 ase_vertexNormal = v.normal.xyz;
			v.vertex.xyz += ( lerpResult568 * ase_vertexNormal );
		}

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			int Direction723 = _UVVDirection1UDirection0;
			float2 appendResult706 = (float2(_WaterMixSpeed.y , _WaterMixSpeed.x));
			float2 uv_WaterNormal = i.uv_texcoord * _WaterNormal_ST.xy + _WaterNormal_ST.zw;
			float Globaltiling821 = ( 1.0 / _GlobalTiling );
			float2 temp_output_824_0 = ( uv_WaterNormal * Globaltiling821 );
			float2 panner612 = ( _Time.y * (( (float)Direction723 == 1.0 ) ? _WaterMixSpeed :  appendResult706 ) + temp_output_824_0);
			float2 WaterSpeedValueMix516 = panner612;
			float2 appendResult705 = (float2(_WaterMainSpeed.y , _WaterMainSpeed.x));
			float2 panner611 = ( _Time.y * (( (float)Direction723 == 1.0 ) ? _WaterMainSpeed :  appendResult705 ) + temp_output_824_0);
			float2 WaterSpeedValueMain614 = panner611;
			float2 appendResult709 = (float2(_SmallCascadeMixSpeed.y , _SmallCascadeMixSpeed.x));
			float2 uv_SmallCascadeNormal = i.uv_texcoord * _SmallCascadeNormal_ST.xy + _SmallCascadeNormal_ST.zw;
			float2 temp_output_828_0 = ( uv_SmallCascadeNormal * Globaltiling821 );
			float2 panner597 = ( _Time.y * (( (float)Direction723 == 1.0 ) ? _SmallCascadeMixSpeed :  appendResult709 ) + temp_output_828_0);
			float2 SmallCascadeSpeedValueMix433 = panner597;
			float2 appendResult710 = (float2(_SmallCascadeMainSpeed.y , _SmallCascadeMainSpeed.x));
			float2 panner598 = ( _Time.y * (( (float)Direction723 == 1.0 ) ? _SmallCascadeMainSpeed :  appendResult710 ) + temp_output_828_0);
			float2 SmallCascadeSpeedValueMain600 = panner598;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float clampResult259 = clamp( ase_worldNormal.y , 0.0 , 1.0 );
			float temp_output_258_0 = ( _SmallCascadeAngle / 45.0 );
			float clampResult263 = clamp( ( clampResult259 - ( 1.0 - temp_output_258_0 ) ) , 0.0 , 2.0 );
			float clampResult584 = clamp( ( clampResult263 * ( 1.0 / temp_output_258_0 ) ) , 0.0 , 1.0 );
			float clampResult507 = clamp( ase_worldNormal.y , 0.0 , 1.0 );
			float temp_output_504_0 = ( _BigCascadeAngle / 45.0 );
			float clampResult509 = clamp( ( clampResult507 - ( 1.0 - temp_output_504_0 ) ) , 0.0 , 2.0 );
			float clampResult583 = clamp( ( clampResult509 * ( 1.0 / temp_output_504_0 ) ) , 0.0 , 1.0 );
			float clampResult514 = clamp( pow( ( 1.0 - clampResult583 ) , _BigCascadeAngleFalloff ) , 0.0 , 1.0 );
			float clampResult285 = clamp( ( pow( ( 1.0 - clampResult584 ) , _SmallCascadeAngleFalloff ) - clampResult514 ) , 0.0 , 1.0 );
			float3 lerpResult330 = lerp( BlendNormals( UnpackScaleNormal( tex2D( _WaterNormal, WaterSpeedValueMix516 ), ( _NormalScale * 1.2 ) ) , UnpackScaleNormal( tex2D( _WaterNormal, WaterSpeedValueMain614 ), _NormalScale ) ) , BlendNormals( UnpackScaleNormal( tex2D( _SmallCascadeNormal, SmallCascadeSpeedValueMix433 ), _SmallCascadeNormalScale ) , UnpackScaleNormal( tex2D( _SmallCascadeNormal, SmallCascadeSpeedValueMain600 ), _SmallCascadeNormalScale ) ) , clampResult285);
			float2 appendResult712 = (float2(_BigCascadeMixSpeed.y , _BigCascadeMixSpeed.x));
			float2 uv_BigCascadeNormal = i.uv_texcoord * _BigCascadeNormal_ST.xy + _BigCascadeNormal_ST.zw;
			float2 temp_output_830_0 = ( uv_BigCascadeNormal * Globaltiling821 );
			float2 panner606 = ( _Time.y * (( (float)Direction723 == 1.0 ) ? _BigCascadeMixSpeed :  appendResult712 ) + temp_output_830_0);
			float2 BigCascadeSpeedValueMix608 = panner606;
			float2 appendResult714 = (float2(_BigCascadeMainSpeed.y , _BigCascadeMainSpeed.x));
			float2 panner607 = ( _Time.y * (( (float)Direction723 == 1.0 ) ? _BigCascadeMainSpeed :  appendResult714 ) + temp_output_830_0);
			float2 BigCascadeSpeedValueMain432 = panner607;
			float3 lerpResult529 = lerp( lerpResult330 , BlendNormals( UnpackScaleNormal( tex2D( _BigCascadeNormal, BigCascadeSpeedValueMix608 ), _BigCascadeNormalScale ) , UnpackScaleNormal( tex2D( _BigCascadeNormal, BigCascadeSpeedValueMain432 ), _BigCascadeNormalScale ) ) , clampResult514);
			o.Normal = lerpResult529;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			float2 appendResult163 = (float2(ase_grabScreenPosNorm.r , ase_grabScreenPosNorm.g));
			float4 screenColor65 = tex2D( _WaterGrab, ( float3( ( appendResult163 / ase_grabScreenPosNorm.a ) ,  0.0 ) + ( lerpResult529 * _Distortion ) ).xy );
			float eyeDepth1 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD( ase_screenPos ))));
			float temp_output_89_0 = abs( ( eyeDepth1 - ase_screenPos.w ) );
			float lerpResult786 = lerp( pow( ( temp_output_89_0 + _ShalowDepth ) , _ShalowFalloff ) , 100.0 , ( _BigCascadeTransparency * clampResult514 ));
			float temp_output_94_0 = saturate( lerpResult786 );
			float4 lerpResult13 = lerp( _DeepColor , _ShalowColor , temp_output_94_0);
			float temp_output_113_0 = saturate( pow( ( temp_output_89_0 + _FoamDepth ) , _FoamFalloff ) );
			float2 appendResult716 = (float2(_FoamSpeed.y , _FoamSpeed.x));
			float2 uv_Foam = i.uv_texcoord * _Foam_ST.xy + _Foam_ST.zw;
			float2 panner116 = ( _Time.y * (( (float)Direction723 == 1.0 ) ? _FoamSpeed :  appendResult716 ) + ( uv_Foam * Globaltiling821 ));
			float temp_output_114_0 = ( temp_output_113_0 * tex2D( _Foam, panner116 ).a );
			float4 lerpResult117 = lerp( lerpResult13 , float4( _FoamColor , 0.0 ) , temp_output_114_0);
			float4 lerpResult93 = lerp( screenColor65 , lerpResult117 , temp_output_113_0);
			float temp_output_458_0 = ( 1.0 - temp_output_94_0 );
			float4 lerpResult390 = lerp( lerpResult93 , lerpResult13 , temp_output_458_0);
			float4 tex2DNode319 = tex2D( _SmallCascade, SmallCascadeSpeedValueMain600 );
			float2 appendResult718 = (float2(_NoiseSpeed.y , _NoiseSpeed.x));
			float2 temp_output_743_0 = (( (float)Direction723 == 1.0 ) ? _NoiseSpeed :  appendResult718 );
			float2 uv_Noise = i.uv_texcoord * _Noise_ST.xy + _Noise_ST.zw;
			float2 temp_output_834_0 = ( uv_Noise * Globaltiling821 );
			float2 panner646 = ( _SinTime.x * ( temp_output_743_0 * float2( -1.2,-0.9 ) ) + temp_output_834_0);
			float4 tex2DNode647 = tex2D( _Noise, panner646 );
			float2 panner321 = ( _SinTime.x * temp_output_743_0 + temp_output_834_0);
			float clampResult488 = clamp( ( pow( min( tex2DNode647.a , tex2D( _Noise, panner321 ).a ) , _SmallCascadeNoisePower ) * 20.0 ) , 0.0 , 1.0 );
			float lerpResult480 = lerp( 0.0 , tex2DNode319.a , clampResult488);
			float clampResult322 = clamp( pow( tex2DNode319.a , _SmallCascadeFoamFalloff ) , 0.0 , 1.0 );
			float lerpResult580 = lerp( 0.0 , clampResult322 , clampResult285);
			float4 lerpResult324 = lerp( lerpResult390 , float4( ( lerpResult480 * _SmallCascadeColor ) , 0.0 ) , lerpResult580);
			float4 tex2DNode213 = tex2D( _BigCascade, BigCascadeSpeedValueMain432 );
			float clampResult783 = clamp( ( pow( min( tex2DNode647.a , tex2D( _Noise, ( panner321 + float2( -0.47,0.37 ) ) ).a ) , _BigCascadeNoisePower ) * 20.0 ) , 0.0 , 1.0 );
			float lerpResult626 = lerp( ( tex2DNode213.a * 0.5 ) , tex2DNode213.a , clampResult783);
			float clampResult299 = clamp( pow( tex2DNode213.a , _BigCascadeFoamFalloff ) , 0.0 , 1.0 );
			float lerpResult579 = lerp( 0.0 , clampResult299 , clampResult514);
			float4 lerpResult239 = lerp( lerpResult324 , float4( ( lerpResult626 * _BigCascadeColor ) , 0.0 ) , lerpResult579);
			o.Albedo = lerpResult239.rgb;
			float3 break453 = lerpResult529;
			float clampResult552 = clamp( max( break453.x , break453.y ) , 0.0 , 1.0 );
			float4 lerpResult451 = lerp( float4( 0,0,0,0 ) , _ShalowColor , clampResult552);
			float lerpResult549 = lerp( _WaterDeepTranslucencyPower , _WaterShalowTranslucencyPower , temp_output_94_0);
			float4 lerpResult459 = lerp( float4( 0,0,0,0 ) , ( lerpResult451 * lerpResult549 ) , temp_output_458_0);
			o.Emission = lerpResult459.rgb;
			float lerpResult819 = lerp( _WaterSpecularFar , _WaterSpecularClose , pow( temp_output_94_0 , _WaterSpecularThreshold ));
			float lerpResult130 = lerp( lerpResult819 , _FoamSpecular , temp_output_114_0);
			float lerpResult585 = lerp( lerpResult130 , _SmallCascadeSpecular , ( lerpResult580 * clampResult285 ));
			float lerpResult587 = lerp( lerpResult585 , _BigCascadeSpecular , ( lerpResult579 * clampResult514 ));
			float3 temp_cast_16 = (lerpResult587).xxx;
			o.Specular = temp_cast_16;
			float lerpResult591 = lerp( _WaterSmoothness , _FoamSmoothness , temp_output_114_0);
			float lerpResult593 = lerp( lerpResult591 , _SmallCascadeSmoothness , ( lerpResult580 * clampResult285 ));
			float lerpResult592 = lerp( lerpResult593 , _BigCascadeSmoothness , ( lerpResult579 * clampResult514 ));
			o.Smoothness = lerpResult592;
			o.Occlusion = _AOPower;
			float lerpResult208 = lerp( 0.0 , 1.0 , pow( saturate( pow( temp_output_89_0 , _WaterDepth ) ) , _WaterFalloffBorder ));
			o.Alpha = lerpResult208;
		}

		ENDCG
	}
}