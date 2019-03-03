Shader "NatureManufacture Shaders/Water/Water Swamp Tesseled Vertex Color Flow"
{
	Properties
	{
		_GlobalTiling("Global Tiling", Range( 0.001 , 100)) = 1
		_UVVDirection1UDirection0("UV - V Direction (1) U Direction (0)", Int) = 0
		_WaterMainSpeed("Water Main Speed", Vector) = (1,1,0,0)
		_WaterMixSpeed("Water Mix Speed", Vector) = (0.01,0.05,0,0)
		_WaterDepth("Water Depth", Range( 0 , 1)) = 0
		_ShalowFalloff("Shalow Falloff", Float) = 0
		_ShalowDepth("Shalow Depth", Range( 0 , 10)) = 0.7
		_ShalowColor("Shalow Color", Color) = (1,1,1,0)
		_WaterAOPower("Water AO Power", Range( 0 , 1)) = 1
		_DeepColor("Deep Color", Color) = (0,0,0,0)
		_WaterSpecularClose("Water Specular Close", Range( 0 , 1)) = 0
		_WaterSpecularFar("Water Specular Far", Range( 0 , 1)) = 0
		_WaterSpecularThreshold("Water Specular Threshold", Range( 0 , 10)) = 1
		_WaterSmoothness("Water Smoothness", Range( 0 , 1)) = 0
		_Distortion("Distortion", Float) = 0.5
		_WaterFalloffBorder("Water Falloff Border", Range( 0 , 30)) = 0
		_WaterNormal("Water Normal", 2D) = "bump" {}
		_NormalScale("Normal Scale", Float) = 0
		[NoScaleOffset]_WaterTesselation("Water Tesselation", 2D) = "black" {}
		_WaterTessScale("Water Tess Scale", Float) = 0
		_CascadeAngle("Cascade Angle", Range( 0.001 , 90)) = 90
		_CascadeAngleFalloff("Cascade Angle Falloff", Range( 0 , 80)) = 5
		_Noise("Noise", 2D) = "white" {}
		_DetailNoisePower("Detail Noise Power", Range( 0 , 10)) = 2.71
		_DetailAlbedo("Detail Albedo", 2D) = "black" {}
		_DetailAlbedoColor("Detail Albedo Color", Color) = (1,1,1,0)
		_DetailSpecular("Detail Specular", Range( 0 , 1)) = 0
		[NoScaleOffset]_DetailNormal("Detail Normal", 2D) = "bump" {}
		_DetailNormalScale("Detail Normal Scale", Float) = 0
		_DetailSmoothness("Detail Smoothness", Range( 0 , 1)) = 0
		_DetailAOPower("Detail AO Power", Range( 0 , 1)) = 1
		_Detail2NoisePower("Detail 2 Noise Power", Range( 0 , 10)) = 2.71
		_Detail2Albedo("Detail 2 Albedo", 2D) = "black" {}
		_Detail2AlbedoColor("Detail 2 Albedo Color", Color) = (1,1,1,0)
		_Detail2Specular("Detail 2 Specular", Range( 0 , 1)) = 0
		_Detail2NormalScale("Detail 2 Normal Scale", Float) = 0
		[NoScaleOffset]_Detail2Normal("Detail 2 Normal", 2D) = "bump" {}
		_Detail2Smoothness("Detail 2 Smoothness", Range( 0 , 1)) = 0
		_Detail2AOPower("Detail 2 AO Power", Range( 0 , 1)) = 1
		_EdgeLength ( "Edge length", Range( 2, 50 ) ) = 25
		_TessMaxDisp( "Max Displacement", Float ) = 11
		_TessPhongStrength( "Phong Tess Strength", Range( 0, 1 ) ) = 0.5
		[NoScaleOffset]_Detail1G_Sm_Detail_2_A_Sm("Detail 1 G_Sm_Detail_2_A_Sm", 2D) = "white" {}
		[HideInInspector] _texcoord4( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Geometry+999" }
		Cull Off
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
			half2 uv_texcoord;
			half2 uv4_texcoord4;
			float4 vertexColor : COLOR;
			float3 worldNormal;
			INTERNAL_DATA
			float4 screenPos;
		};

		uniform half _WaterTessScale;
		uniform int _UVVDirection1UDirection0;
		uniform half2 _WaterMixSpeed;
		uniform sampler2D _WaterNormal;
		uniform float4 _WaterNormal_ST;
		uniform half _GlobalTiling;
		uniform sampler2D _WaterTesselation;
		uniform half2 _WaterMainSpeed;
		uniform half _NormalScale;
		uniform half _DetailNormalScale;
		uniform sampler2D _DetailAlbedo;
		uniform float4 _DetailAlbedo_ST;
		uniform sampler2D _DetailNormal;
		uniform sampler2D _Noise;
		uniform float4 _Noise_ST;
		uniform half _DetailNoisePower;
		uniform half _Detail2NormalScale;
		uniform sampler2D _Detail2Albedo;
		uniform float4 _Detail2Albedo_ST;
		uniform sampler2D _Detail2Normal;
		uniform half _Detail2NoisePower;
		uniform half _CascadeAngle;
		uniform half _CascadeAngleFalloff;
		uniform sampler2D _WaterGrab;
		uniform half _Distortion;
		uniform half4 _DeepColor;
		uniform half4 _ShalowColor;
		uniform sampler2D _CameraDepthTexture;
		uniform half _ShalowDepth;
		uniform half _ShalowFalloff;
		uniform half4 _DetailAlbedoColor;
		uniform half4 _Detail2AlbedoColor;
		uniform half _WaterSpecularFar;
		uniform half _WaterSpecularClose;
		uniform half _WaterSpecularThreshold;
		uniform half _DetailSpecular;
		uniform half _Detail2Specular;
		uniform half _WaterSmoothness;
		uniform sampler2D _Detail1G_Sm_Detail_2_A_Sm;
		uniform half _DetailSmoothness;
		uniform half _Detail2Smoothness;
		uniform half _WaterAOPower;
		uniform half _DetailAOPower;
		uniform half _Detail2AOPower;
		uniform half _WaterDepth;
		uniform half _WaterFalloffBorder;
		uniform half _EdgeLength;
		uniform half _TessMaxDisp;
		uniform half _TessPhongStrength;


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
			float2 appendResult706 = (half2(_WaterMixSpeed.y , _WaterMixSpeed.x));
			float2 uv_WaterNormal = v.texcoord.xy * _WaterNormal_ST.xy + _WaterNormal_ST.zw;
			float Globaltiling1185 = ( 1.0 / _GlobalTiling );
			float2 temp_output_1186_0 = ( uv_WaterNormal * Globaltiling1185 );
			float2 panner612 = ( _Time.y * (( (float)Direction723 == 1.0 ) ? _WaterMixSpeed :  appendResult706 ) + temp_output_1186_0);
			float2 WaterSpeedValueMix516 = panner612;
			float2 appendResult705 = (half2(_WaterMainSpeed.y , _WaterMainSpeed.x));
			float2 break821 = (( (float)Direction723 == 1.0 ) ? _WaterMainSpeed :  appendResult705 );
			float2 appendResult823 = (half2(( break821.x * v.texcoord3.xy.x ) , ( break821.y * v.texcoord3.xy.y )));
			float mulTime815 = _Time.y * 0.3;
			float temp_output_816_0 = ( mulTime815 * 0.15 );
			float temp_output_818_0 = frac( ( temp_output_816_0 + 1.0 ) );
			float2 temp_output_826_0 = ( appendResult823 * temp_output_818_0 );
			float2 WaterSpeedValueMainFlowUV1830 = ( temp_output_1186_0 + temp_output_826_0 );
			float2 temp_output_825_0 = ( appendResult823 * frac( ( temp_output_816_0 + 0.5 ) ) );
			float2 WaterSpeedValueMainFlowUV2831 = ( temp_output_1186_0 + temp_output_825_0 );
			float clampResult845 = clamp( abs( ( ( temp_output_818_0 + -0.5 ) * 2.0 ) ) , 0.0 , 1.0 );
			float SlowFlowHeightBase835 = clampResult845;
			float lerpResult840 = lerp( tex2Dlod( _WaterTesselation, half4( WaterSpeedValueMainFlowUV1830, 0, 1.0) ).a , tex2Dlod( _WaterTesselation, half4( WaterSpeedValueMainFlowUV2831, 0, 1.0) ).a , SlowFlowHeightBase835);
			float3 ase_vertexNormal = v.normal.xyz;
			v.vertex.xyz += ( ( ( _WaterTessScale * tex2Dlod( _WaterTesselation, half4( WaterSpeedValueMix516, 0, 1.0) ).a ) + ( _WaterTessScale * lerpResult840 ) ) * ase_vertexNormal );
		}

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			int Direction723 = _UVVDirection1UDirection0;
			float2 appendResult706 = (half2(_WaterMixSpeed.y , _WaterMixSpeed.x));
			float2 uv_WaterNormal = i.uv_texcoord * _WaterNormal_ST.xy + _WaterNormal_ST.zw;
			float Globaltiling1185 = ( 1.0 / _GlobalTiling );
			float2 temp_output_1186_0 = ( uv_WaterNormal * Globaltiling1185 );
			float2 panner612 = ( _Time.y * (( (float)Direction723 == 1.0 ) ? _WaterMixSpeed :  appendResult706 ) + temp_output_1186_0);
			float2 WaterSpeedValueMix516 = panner612;
			float2 appendResult705 = (half2(_WaterMainSpeed.y , _WaterMainSpeed.x));
			float2 break821 = (( (float)Direction723 == 1.0 ) ? _WaterMainSpeed :  appendResult705 );
			float2 appendResult823 = (half2(( break821.x * i.uv4_texcoord4.x ) , ( break821.y * i.uv4_texcoord4.y )));
			float mulTime815 = _Time.y * 0.3;
			float temp_output_816_0 = ( mulTime815 * 0.15 );
			float temp_output_818_0 = frac( ( temp_output_816_0 + 1.0 ) );
			float2 temp_output_826_0 = ( appendResult823 * temp_output_818_0 );
			float2 WaterSpeedValueMainFlowUV1830 = ( temp_output_1186_0 + temp_output_826_0 );
			float2 temp_output_825_0 = ( appendResult823 * frac( ( temp_output_816_0 + 0.5 ) ) );
			float2 WaterSpeedValueMainFlowUV2831 = ( temp_output_1186_0 + temp_output_825_0 );
			float clampResult845 = clamp( abs( ( ( temp_output_818_0 + -0.5 ) * 2.0 ) ) , 0.0 , 1.0 );
			float SlowFlowHeightBase835 = clampResult845;
			float3 lerpResult838 = lerp( UnpackScaleNormal( tex2D( _WaterNormal, WaterSpeedValueMainFlowUV1830 ), _NormalScale ) , UnpackScaleNormal( tex2D( _WaterNormal, WaterSpeedValueMainFlowUV2831 ), _NormalScale ) , SlowFlowHeightBase835);
			float3 temp_output_24_0 = BlendNormals( UnpackScaleNormal( tex2D( _WaterNormal, WaterSpeedValueMix516 ), ( _NormalScale * 1.2 ) ) , lerpResult838 );
			float2 uv_DetailAlbedo = i.uv_texcoord * _DetailAlbedo_ST.xy + _DetailAlbedo_ST.zw;
			float2 temp_output_1189_0 = ( uv_DetailAlbedo * Globaltiling1185 );
			float2 Detail1SpeedValueMainFlowUV11018 = ( temp_output_1189_0 + temp_output_826_0 );
			float2 Detail1SpeedValueMainFlowUV21021 = ( temp_output_1189_0 + temp_output_825_0 );
			float3 lerpResult864 = lerp( UnpackScaleNormal( tex2D( _DetailNormal, Detail1SpeedValueMainFlowUV11018 ), _DetailNormalScale ) , UnpackScaleNormal( tex2D( _DetailNormal, Detail1SpeedValueMainFlowUV21021 ), _DetailNormalScale ) , SlowFlowHeightBase835);
			float4 lerpResult935 = lerp( tex2D( _DetailAlbedo, Detail1SpeedValueMainFlowUV11018 ) , tex2D( _DetailAlbedo, Detail1SpeedValueMainFlowUV21021 ) , SlowFlowHeightBase835);
			float2 uv_Noise = i.uv_texcoord * _Noise_ST.xy + _Noise_ST.zw;
			float2 temp_output_1193_0 = ( uv_Noise * Globaltiling1185 );
			float2 NoiseSpeedValueMainFlowUV11064 = ( temp_output_1193_0 + temp_output_826_0 );
			float2 NoiseSpeedValueMainFlowUV21063 = ( temp_output_1193_0 + temp_output_825_0 );
			float lerpResult1014 = lerp( tex2D( _Noise, NoiseSpeedValueMainFlowUV11064 ).a , tex2D( _Noise, NoiseSpeedValueMainFlowUV21063 ).a , SlowFlowHeightBase835);
			float smoothstepResult1037 = smoothstep( 0.0 , 0.2 , pow( lerpResult1014 , _DetailNoisePower ));
			float clampResult488 = clamp( smoothstepResult1037 , 0.0 , 1.0 );
			float lerpResult1083 = lerp( 0.0 , lerpResult935.a , clampResult488);
			float Detal_1_Alpha_Noise1158 = lerpResult1083;
			float3 lerpResult932 = lerp( temp_output_24_0 , lerpResult864 , Detal_1_Alpha_Noise1158);
			float2 uv_Detail2Albedo = i.uv_texcoord * _Detail2Albedo_ST.xy + _Detail2Albedo_ST.zw;
			float2 temp_output_1191_0 = ( uv_Detail2Albedo * Globaltiling1185 );
			float2 Detail2SpeedValueMainFlowUV11022 = ( temp_output_1191_0 + temp_output_826_0 );
			float2 Detail2SpeedValueMainFlowUV21025 = ( temp_output_1191_0 + temp_output_825_0 );
			float3 lerpResult922 = lerp( UnpackScaleNormal( tex2D( _Detail2Normal, Detail2SpeedValueMainFlowUV11022 ), _Detail2NormalScale ) , UnpackScaleNormal( tex2D( _Detail2Normal, Detail2SpeedValueMainFlowUV21025 ), _Detail2NormalScale ) , SlowFlowHeightBase835);
			float4 lerpResult947 = lerp( tex2D( _Detail2Albedo, Detail2SpeedValueMainFlowUV11022 ) , tex2D( _Detail2Albedo, Detail2SpeedValueMainFlowUV21025 ) , SlowFlowHeightBase835);
			float lerpResult1016 = lerp( tex2D( _Noise, NoiseSpeedValueMainFlowUV11064 ).a , tex2D( _Noise, NoiseSpeedValueMainFlowUV21063 ).a , SlowFlowHeightBase835);
			float smoothstepResult1056 = smoothstep( 0.0 , 0.47 , pow( max( lerpResult1016 , lerpResult1014 ) , _Detail2NoisePower ));
			float clampResult962 = clamp( smoothstepResult1056 , 0.0 , 1.0 );
			float lerpResult1125 = lerp( 0.0 , lerpResult947.a , clampResult962);
			float Detal_2_Alpha_Noise1159 = lerpResult1125;
			float3 lerpResult933 = lerp( lerpResult932 , lerpResult922 , Detal_2_Alpha_Noise1159);
			float Detal_1_Alpha1146 = lerpResult935.a;
			float3 lerpResult1134 = lerp( temp_output_24_0 , lerpResult864 , Detal_1_Alpha1146);
			float4 VertexColorRGB1172 = ( i.vertexColor / float4( 1,1,1,1 ) );
			float4 break1177 = VertexColorRGB1172;
			float3 lerpResult748 = lerp( lerpResult933 , lerpResult1134 , break1177.r);
			float Detal_2_Alpha1152 = lerpResult947.a;
			float3 lerpResult1135 = lerp( temp_output_24_0 , lerpResult922 , Detal_2_Alpha1152);
			float3 lerpResult749 = lerp( lerpResult748 , lerpResult1135 , break1177.g);
			float3 lerpResult750 = lerp( lerpResult749 , temp_output_24_0 , break1177.b);
			half3 ase_worldNormal = WorldNormalVector( i, half3( 0, 0, 1 ) );
			float clampResult259 = clamp( ase_worldNormal.y , 0.0 , 1.0 );
			float temp_output_258_0 = ( _CascadeAngle / 45.0 );
			float clampResult263 = clamp( ( clampResult259 - ( 1.0 - temp_output_258_0 ) ) , 0.0 , 2.0 );
			float clampResult584 = clamp( ( clampResult263 * ( 1.0 / temp_output_258_0 ) ) , 0.0 , 1.0 );
			float clampResult285 = clamp( pow( ( 1.0 - clampResult584 ) , _CascadeAngleFalloff ) , 0.0 , 1.0 );
			float WaterfallAngle1144 = clampResult285;
			float3 lerpResult983 = lerp( lerpResult750 , temp_output_24_0 , WaterfallAngle1144);
			o.Normal = lerpResult983;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			float2 appendResult163 = (half2(ase_grabScreenPosNorm.r , ase_grabScreenPosNorm.g));
			float4 screenColor65 = tex2D( _WaterGrab, ( half3( ( appendResult163 / ase_grabScreenPosNorm.a ) ,  0.0 ) + ( lerpResult933 * _Distortion ) ).xy );
			float eyeDepth1 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD( ase_screenPos ))));
			float temp_output_89_0 = abs( ( eyeDepth1 - ase_screenPos.w ) );
			float temp_output_87_0 = pow( ( temp_output_89_0 + _ShalowDepth ) , _ShalowFalloff );
			float temp_output_94_0 = saturate( temp_output_87_0 );
			float4 lerpResult13 = lerp( _DeepColor , _ShalowColor , temp_output_94_0);
			float4 lerpResult773 = lerp( screenColor65 , lerpResult13 , ( 1.0 - temp_output_94_0 ));
			float4 temp_output_1106_0 = ( lerpResult935 * _DetailAlbedoColor );
			float4 lerpResult964 = lerp( lerpResult773 , temp_output_1106_0 , Detal_1_Alpha_Noise1158);
			float4 temp_output_1108_0 = ( lerpResult947 * _Detail2AlbedoColor );
			float4 lerpResult987 = lerp( lerpResult964 , temp_output_1108_0 , Detal_2_Alpha_Noise1159);
			float4 lerpResult1132 = lerp( lerpResult773 , temp_output_1106_0 , Detal_1_Alpha1146);
			float4 break1181 = VertexColorRGB1172;
			float4 lerpResult986 = lerp( lerpResult987 , lerpResult1132 , break1181.r);
			float4 lerpResult1133 = lerp( lerpResult773 , temp_output_1108_0 , Detal_2_Alpha1152);
			float4 lerpResult984 = lerp( lerpResult986 , lerpResult1133 , break1181.g);
			float4 lerpResult1113 = lerp( lerpResult984 , lerpResult773 , break1181.b);
			float4 lerpResult1058 = lerp( lerpResult1113 , lerpResult773 , WaterfallAngle1144);
			o.Albedo = lerpResult1058.rgb;
			float lerpResult1124 = lerp( _WaterSpecularFar , _WaterSpecularClose , pow( temp_output_94_0 , _WaterSpecularThreshold ));
			half4 temp_cast_5 = (lerpResult1124).xxxx;
			float4 clampResult1050 = clamp( ( temp_output_1106_0 * float4( 0.3,0.3019608,0.3019608,0 ) ) , float4( 0,0,0,0 ) , float4( 0.5,0.5019608,0.5019608,0 ) );
			float4 temp_output_1051_0 = ( _DetailSpecular * clampResult1050 );
			float4 lerpResult969 = lerp( temp_cast_5 , temp_output_1051_0 , Detal_1_Alpha_Noise1158);
			float4 clampResult1053 = clamp( ( lerpResult947 * float4( 0.3,0.3019608,0.3019608,0 ) ) , float4( 0,0,0,0 ) , float4( 0.5,0.5019608,0.5019608,0 ) );
			float4 temp_output_1052_0 = ( _Detail2Specular * clampResult1053 );
			float4 lerpResult970 = lerp( lerpResult969 , temp_output_1052_0 , Detal_2_Alpha_Noise1159);
			half4 temp_cast_6 = (lerpResult1124).xxxx;
			float4 lerpResult1136 = lerp( temp_cast_6 , temp_output_1051_0 , Detal_1_Alpha1146);
			float4 break1179 = VertexColorRGB1172;
			float4 lerpResult130 = lerp( lerpResult970 , lerpResult1136 , break1179.r);
			half4 temp_cast_7 = (lerpResult1124).xxxx;
			float4 lerpResult1137 = lerp( temp_cast_7 , temp_output_1052_0 , Detal_2_Alpha1152);
			float4 lerpResult785 = lerp( lerpResult130 , lerpResult1137 , break1179.g);
			half4 temp_cast_8 = (lerpResult1124).xxxx;
			float4 lerpResult786 = lerp( lerpResult785 , temp_cast_8 , break1179.b);
			half4 temp_cast_9 = (lerpResult1124).xxxx;
			float4 lerpResult982 = lerp( lerpResult786 , temp_cast_9 , WaterfallAngle1144);
			o.Specular = lerpResult982.rgb;
			float4 lerpResult1089 = lerp( tex2D( _Detail1G_Sm_Detail_2_A_Sm, Detail1SpeedValueMainFlowUV11018 ) , tex2D( _Detail1G_Sm_Detail_2_A_Sm, Detail1SpeedValueMainFlowUV21021 ) , SlowFlowHeightBase835);
			float temp_output_1093_0 = ( lerpResult1089.g * _DetailSmoothness );
			float lerpResult973 = lerp( _WaterSmoothness , temp_output_1093_0 , Detal_1_Alpha_Noise1158);
			float4 lerpResult1102 = lerp( tex2D( _Detail1G_Sm_Detail_2_A_Sm, Detail2SpeedValueMainFlowUV11022 ) , tex2D( _Detail1G_Sm_Detail_2_A_Sm, Detail2SpeedValueMainFlowUV21025 ) , SlowFlowHeightBase835);
			float temp_output_1094_0 = ( lerpResult1102.a * _Detail2Smoothness );
			float lerpResult974 = lerp( lerpResult973 , temp_output_1094_0 , Detal_2_Alpha_Noise1159);
			float lerpResult1129 = lerp( _WaterSmoothness , temp_output_1093_0 , Detal_1_Alpha1146);
			float4 break1174 = VertexColorRGB1172;
			float lerpResult975 = lerp( lerpResult974 , lerpResult1129 , break1174.r);
			float lerpResult1127 = lerp( _WaterSmoothness , temp_output_1094_0 , Detal_2_Alpha1152);
			float lerpResult972 = lerp( lerpResult975 , lerpResult1127 , break1174.g);
			float lerpResult971 = lerp( lerpResult972 , _WaterSmoothness , break1174.b);
			float lerpResult981 = lerp( lerpResult971 , _WaterSmoothness , WaterfallAngle1144);
			o.Smoothness = lerpResult981;
			float lerpResult1038 = lerp( _WaterAOPower , _DetailAOPower , Detal_1_Alpha_Noise1158);
			float lerpResult1039 = lerp( lerpResult1038 , _Detail2AOPower , Detal_2_Alpha_Noise1159);
			float lerpResult1130 = lerp( _WaterAOPower , _DetailAOPower , Detal_1_Alpha1146);
			float4 break1175 = VertexColorRGB1172;
			float lerpResult1040 = lerp( lerpResult1039 , lerpResult1130 , break1175.r);
			float lerpResult1128 = lerp( _WaterAOPower , _Detail2AOPower , Detal_2_Alpha1152);
			float lerpResult1041 = lerp( lerpResult1040 , lerpResult1128 , break1175.g);
			float lerpResult1042 = lerp( lerpResult1041 , _WaterAOPower , break1175.b);
			float lerpResult1057 = lerp( lerpResult1042 , _WaterAOPower , WaterfallAngle1144);
			o.Occlusion = lerpResult1057;
			float lerpResult208 = lerp( 0.0 , 1.0 , pow( saturate( pow( temp_output_89_0 , _WaterDepth ) ) , _WaterFalloffBorder ));
			o.Alpha = ( lerpResult208 * i.vertexColor.a );
		}

		ENDCG
	}
}