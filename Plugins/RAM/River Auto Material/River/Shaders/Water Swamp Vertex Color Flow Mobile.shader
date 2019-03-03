Shader "NatureManufacture Shaders/Water/Water Swamp Vertex Color Flow Mobile"
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
		_CascadeAngle("Cascade Angle", Range( 0.001 , 90)) = 90
		_CascadeAngleFalloff("Cascade Angle Falloff", Range( 0 , 80)) = 5
		_Noise("Noise", 2D) = "white" {}
		_DetailNoisePower("Detail Noise Power", Range( 0 , 10)) = 2.71
		_DetailAlbedo("Detail Albedo", 2D) = "black" {}
		_DetailAlbedoColor("Detail Albedo Color", Color) = (1,1,1,0)
		_DetailSpecular("Detail Specular", Range( 0 , 1)) = 0
		_DetailNormal("Detail Normal", 2D) = "bump" {}
		_DetailNormalScale("Detail Normal Scale", Float) = 0
		_DetailSmoothness("Detail Smoothness", Range( 0 , 1)) = 0
		_DetailAOPower("Detail AO Power", Range( 0 , 1)) = 1
		_Detail1A_Sm("Detail 1 A_Sm", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord4( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
		[Header(Forward Rendering Options)]
		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Reflections", Float) = 1.0
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Geometry+999" }
		Cull Off
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha , SrcAlpha OneMinusSrcAlpha
		BlendOp Add , Add
		GrabPass{ "_WaterGrab" }
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
		#pragma shader_feature _GLOSSYREFLECTIONS_OFF
		#pragma surface surf StandardSpecular keepalpha noshadow 
		struct Input
		{
			half2 uv_texcoord;
			half2 uv4_texcoord4;
			float4 vertexColor : COLOR;
			float3 worldNormal;
			INTERNAL_DATA
			float4 screenPos;
		};

		uniform half _NormalScale;
		uniform int _UVVDirection1UDirection0;
		uniform half2 _WaterMixSpeed;
		uniform sampler2D _WaterNormal;
		uniform float4 _WaterNormal_ST;
		uniform half _GlobalTiling;
		uniform half2 _WaterMainSpeed;
		uniform half _DetailNormalScale;
		uniform sampler2D _DetailAlbedo;
		uniform float4 _DetailAlbedo_ST;
		uniform sampler2D _DetailNormal;
		uniform sampler2D _Noise;
		uniform float4 _Noise_ST;
		uniform half _DetailNoisePower;
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
		uniform half _WaterSpecularFar;
		uniform half _WaterSpecularClose;
		uniform half _WaterSpecularThreshold;
		uniform half _DetailSpecular;
		uniform half _WaterSmoothness;
		uniform sampler2D _Detail1A_Sm;
		uniform half _DetailSmoothness;
		uniform half _WaterAOPower;
		uniform half _DetailAOPower;
		uniform half _WaterDepth;
		uniform half _WaterFalloffBorder;


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


		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			int Direction723 = _UVVDirection1UDirection0;
			float2 appendResult706 = (half2(_WaterMixSpeed.y , _WaterMixSpeed.x));
			float2 uv_WaterNormal = i.uv_texcoord * _WaterNormal_ST.xy + _WaterNormal_ST.zw;
			float Globaltiling1154 = ( 1.0 / _GlobalTiling );
			float2 temp_output_1157_0 = ( uv_WaterNormal * Globaltiling1154 );
			float2 panner612 = ( _Time.y * (( (float)Direction723 == 1.0 ) ? _WaterMixSpeed :  appendResult706 ) + temp_output_1157_0);
			float2 WaterSpeedValueMix516 = panner612;
			float2 appendResult705 = (half2(_WaterMainSpeed.y , _WaterMainSpeed.x));
			float2 break821 = (( (float)Direction723 == 1.0 ) ? _WaterMainSpeed :  appendResult705 );
			float2 appendResult823 = (half2(( break821.x * i.uv4_texcoord4.x ) , ( break821.y * i.uv4_texcoord4.y )));
			float mulTime815 = _Time.y * 0.3;
			float temp_output_816_0 = ( mulTime815 * 0.15 );
			float temp_output_818_0 = frac( ( temp_output_816_0 + 1.0 ) );
			float2 temp_output_826_0 = ( appendResult823 * temp_output_818_0 );
			float2 WaterSpeedValueMainFlowUV1830 = ( temp_output_1157_0 + temp_output_826_0 );
			float2 temp_output_825_0 = ( appendResult823 * frac( ( temp_output_816_0 + 0.5 ) ) );
			float2 WaterSpeedValueMainFlowUV2831 = ( temp_output_1157_0 + temp_output_825_0 );
			float clampResult845 = clamp( abs( ( ( temp_output_818_0 + -0.5 ) * 2.0 ) ) , 0.0 , 1.0 );
			float SlowFlowHeightBase835 = clampResult845;
			float3 lerpResult838 = lerp( UnpackScaleNormal( tex2D( _WaterNormal, WaterSpeedValueMainFlowUV1830 ), _NormalScale ) , UnpackScaleNormal( tex2D( _WaterNormal, WaterSpeedValueMainFlowUV2831 ), _NormalScale ) , SlowFlowHeightBase835);
			float3 temp_output_24_0 = BlendNormals( UnpackScaleNormal( tex2D( _WaterNormal, WaterSpeedValueMix516 ), ( _NormalScale * 1.2 ) ) , lerpResult838 );
			float2 uv_DetailAlbedo = i.uv_texcoord * _DetailAlbedo_ST.xy + _DetailAlbedo_ST.zw;
			float2 temp_output_1158_0 = ( uv_DetailAlbedo * Globaltiling1154 );
			float2 Detail1SpeedValueMainFlowUV11018 = ( temp_output_1158_0 + temp_output_826_0 );
			float2 Detail1SpeedValueMainFlowUV21021 = ( temp_output_1158_0 + temp_output_825_0 );
			float3 lerpResult864 = lerp( UnpackScaleNormal( tex2D( _DetailNormal, Detail1SpeedValueMainFlowUV11018 ), _DetailNormalScale ) , UnpackScaleNormal( tex2D( _DetailNormal, Detail1SpeedValueMainFlowUV21021 ), _DetailNormalScale ) , SlowFlowHeightBase835);
			float4 lerpResult935 = lerp( tex2D( _DetailAlbedo, Detail1SpeedValueMainFlowUV11018 ) , tex2D( _DetailAlbedo, Detail1SpeedValueMainFlowUV21021 ) , SlowFlowHeightBase835);
			float2 uv_Noise = i.uv_texcoord * _Noise_ST.xy + _Noise_ST.zw;
			float2 temp_output_1160_0 = ( uv_Noise * Globaltiling1154 );
			float2 NoiseSpeedValueMainFlowUV11064 = ( temp_output_1160_0 + temp_output_826_0 );
			float2 NoiseSpeedValueMainFlowUV21063 = ( temp_output_1160_0 + temp_output_825_0 );
			float lerpResult1014 = lerp( tex2D( _Noise, NoiseSpeedValueMainFlowUV11064 ).a , tex2D( _Noise, NoiseSpeedValueMainFlowUV21063 ).a , SlowFlowHeightBase835);
			float smoothstepResult1037 = smoothstep( 0.0 , 0.2 , pow( lerpResult1014 , _DetailNoisePower ));
			float clampResult488 = clamp( smoothstepResult1037 , 0.0 , 1.0 );
			float lerpResult1083 = lerp( 0.0 , lerpResult935.a , clampResult488);
			float Detal_1_Alpha_Noi1124 = ( lerpResult1083 * 1.0 );
			float3 lerpResult932 = lerp( temp_output_24_0 , lerpResult864 , Detal_1_Alpha_Noi1124);
			float Detal_1_Alp1122 = lerpResult935.a;
			float3 lerpResult1148 = lerp( lerpResult932 , lerpResult864 , Detal_1_Alp1122);
			float4 VertexColorRB1126 = ( i.vertexColor / float4( 1,1,1,1 ) );
			float4 break1134 = VertexColorRB1126;
			float3 lerpResult748 = lerp( lerpResult932 , lerpResult1148 , break1134.r);
			float3 lerpResult750 = lerp( lerpResult748 , temp_output_24_0 , break1134.b);
			half3 ase_worldNormal = WorldNormalVector( i, half3( 0, 0, 1 ) );
			float clampResult259 = clamp( ase_worldNormal.y , 0.0 , 1.0 );
			float temp_output_258_0 = ( _CascadeAngle / 45.0 );
			float clampResult263 = clamp( ( clampResult259 - ( 1.0 - temp_output_258_0 ) ) , 0.0 , 2.0 );
			float clampResult584 = clamp( ( clampResult263 * ( 1.0 / temp_output_258_0 ) ) , 0.0 , 1.0 );
			float clampResult285 = clamp( pow( ( 1.0 - clampResult584 ) , _CascadeAngleFalloff ) , 0.0 , 1.0 );
			float WaterfallAng1120 = clampResult285;
			float3 lerpResult983 = lerp( lerpResult750 , temp_output_24_0 , WaterfallAng1120);
			o.Normal = lerpResult983;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			float2 appendResult163 = (half2(ase_grabScreenPosNorm.r , ase_grabScreenPosNorm.g));
			float4 screenColor65 = tex2D( _WaterGrab, ( half3( ( appendResult163 / ase_grabScreenPosNorm.a ) ,  0.0 ) + ( lerpResult932 * _Distortion ) ).xy );
			float eyeDepth1 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD( ase_screenPos ))));
			float temp_output_89_0 = abs( ( eyeDepth1 - ase_screenPos.w ) );
			float temp_output_87_0 = pow( ( temp_output_89_0 + _ShalowDepth ) , _ShalowFalloff );
			float temp_output_94_0 = saturate( temp_output_87_0 );
			float4 lerpResult13 = lerp( _DeepColor , _ShalowColor , temp_output_94_0);
			float4 lerpResult773 = lerp( screenColor65 , lerpResult13 , ( 1.0 - temp_output_94_0 ));
			float4 temp_output_1106_0 = ( lerpResult935 * _DetailAlbedoColor );
			float4 lerpResult964 = lerp( lerpResult773 , temp_output_1106_0 , Detal_1_Alpha_Noi1124);
			float4 lerpResult1151 = lerp( lerpResult773 , temp_output_1106_0 , Detal_1_Alp1122);
			float4 break1136 = VertexColorRB1126;
			float4 lerpResult986 = lerp( lerpResult964 , lerpResult1151 , break1136.r);
			float4 lerpResult1113 = lerp( lerpResult986 , lerpResult773 , break1136.b);
			float4 lerpResult1058 = lerp( lerpResult1113 , lerpResult773 , WaterfallAng1120);
			o.Albedo = lerpResult1058.rgb;
			float lerpResult1119 = lerp( _WaterSpecularFar , _WaterSpecularClose , pow( temp_output_94_0 , _WaterSpecularThreshold ));
			half4 temp_cast_5 = (lerpResult1119).xxxx;
			float4 clampResult1050 = clamp( ( temp_output_1106_0 * float4( 0.3,0.3019608,0.3019608,0 ) ) , float4( 0,0,0,0 ) , float4( 0.5,0.5019608,0.5019608,0 ) );
			float4 temp_output_1051_0 = ( _DetailSpecular * clampResult1050 );
			float4 lerpResult969 = lerp( temp_cast_5 , temp_output_1051_0 , Detal_1_Alpha_Noi1124);
			half4 temp_cast_6 = (lerpResult1119).xxxx;
			float4 lerpResult1145 = lerp( temp_cast_6 , temp_output_1051_0 , Detal_1_Alp1122);
			float4 break1132 = VertexColorRB1126;
			float4 lerpResult130 = lerp( lerpResult969 , lerpResult1145 , break1132.r);
			half4 temp_cast_7 = (lerpResult1119).xxxx;
			float4 lerpResult786 = lerp( lerpResult130 , temp_cast_7 , break1132.b);
			half4 temp_cast_8 = (lerpResult1119).xxxx;
			float4 lerpResult982 = lerp( lerpResult786 , temp_cast_8 , WaterfallAng1120);
			o.Specular = lerpResult982.rgb;
			float lerpResult1089 = lerp( tex2D( _Detail1A_Sm, Detail1SpeedValueMainFlowUV11018 ).a , tex2D( _Detail1A_Sm, Detail1SpeedValueMainFlowUV21021 ).a , SlowFlowHeightBase835);
			float temp_output_1093_0 = ( lerpResult1089 * _DetailSmoothness );
			float lerpResult973 = lerp( _WaterSmoothness , temp_output_1093_0 , Detal_1_Alpha_Noi1124);
			float lerpResult1140 = lerp( _WaterSmoothness , temp_output_1093_0 , Detal_1_Alp1122);
			float4 break1130 = VertexColorRB1126;
			float lerpResult975 = lerp( lerpResult973 , lerpResult1140 , break1130.r);
			float lerpResult971 = lerp( lerpResult975 , _WaterSmoothness , break1130.b);
			float lerpResult981 = lerp( lerpResult971 , _WaterSmoothness , WaterfallAng1120);
			o.Smoothness = lerpResult981;
			float lerpResult1038 = lerp( _WaterAOPower , _DetailAOPower , Detal_1_Alpha_Noi1124);
			float lerpResult1142 = lerp( _WaterAOPower , _DetailAOPower , Detal_1_Alp1122);
			float4 break1129 = VertexColorRB1126;
			float lerpResult1040 = lerp( lerpResult1038 , lerpResult1142 , break1129.r);
			float lerpResult1042 = lerp( lerpResult1040 , _WaterAOPower , break1129.b);
			float lerpResult1057 = lerp( lerpResult1042 , _WaterAOPower , WaterfallAng1120);
			o.Occlusion = lerpResult1057;
			float lerpResult208 = lerp( 0.0 , 1.0 , pow( saturate( pow( temp_output_89_0 , _WaterDepth ) ) , _WaterFalloffBorder ));
			o.Alpha = ( lerpResult208 * i.vertexColor.a );
		}

		ENDCG
	}
}