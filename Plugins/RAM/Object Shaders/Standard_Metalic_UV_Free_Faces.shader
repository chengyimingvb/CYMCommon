Shader "NatureManufacture Shaders/Standard Shaders/Standard Metalic UV Free Faces"
{
	Properties
	{
		[NoScaleOffset]_ShapeBumpMap("Shape BumpMap", 2D) = "bump" {}
		_ShapeBumpMapScale("Shape BumpMap Scale", Range( 0 , 5)) = 1
		_Color("Color", Color) = (1,1,1,1)
		_Tiling("Tiling", Range( 0.1 , 100)) = 6
		_TriplanarFallOff("Triplanar FallOff", Range( 0.0001 , 100)) = 15
		[NoScaleOffset]_TopAlbedo("Top Albedo", 2D) = "white" {}
		[NoScaleOffset]_TopNormal("Top Normal", 2D) = "bump" {}
		_TopNormalScale("Top Normal Scale", Range( 0 , 5)) = 1
		[NoScaleOffset]_TopMetalicRAmbientOcclusionGSmoothnessA("Top Metalic (R) Ambient Occlusion (G) Smoothness (A)", 2D) = "white" {}
		[NoScaleOffset]_ShapeAmbientOcclusionG("Shape Ambient Occlusion (G)", 2D) = "white" {}
		_ShapeAmbientOcclusionPower("Shape Ambient Occlusion Power", Range( 0 , 1)) = 1
		[NoScaleOffset]_BottomAlbedo("Bottom Albedo", 2D) = "white" {}
		[NoScaleOffset]_BottomNormal("Bottom Normal", 2D) = "bump" {}
		_BottomNormalScale("Bottom Normal Scale", Range( 0 , 5)) = 1
		[NoScaleOffset]_BottomMetalicRAmbientOcclusionGSmoothnessA("Bottom Metalic (R) Ambient Occlusion (G) Smoothness (A)", 2D) = "white" {}
		_MetallicPower("Metallic Power", Range( 0 , 2)) = 0
		_AmbientOcclusionPower("Ambient Occlusion Power", Range( 0 , 1)) = 0
		_SmoothnessPower("Smoothness Power", Range( 0 , 2)) = 2
		_DetailMask("DetailMask", 2D) = "white" {}
		_DetailAlbedoMap("DetailAlbedoMap", 2D) = "black" {}
		_DetailAlbedoPower("Detail Albedo Power", Range( 0 , 2)) = 0
		[NoScaleOffset]_DetailNormalMap("DetailNormalMap", 2D) = "bump" {}
		_DetailNormalMapScale("DetailNormalMapScale", Range( 0 , 5)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		ZTest LEqual
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float2 uv_texcoord;
		};

		uniform sampler2D _TopNormal;
		uniform sampler2D _BottomNormal;
		uniform float _Tiling;
		uniform float _TriplanarFallOff;
		uniform float _TopNormalScale;
		uniform float _BottomNormalScale;
		uniform float _ShapeBumpMapScale;
		uniform sampler2D _ShapeBumpMap;
		uniform float4 _ShapeBumpMap_ST;
		uniform float _DetailNormalMapScale;
		uniform sampler2D _DetailNormalMap;
		uniform sampler2D _DetailAlbedoMap;
		uniform float4 _DetailAlbedoMap_ST;
		uniform sampler2D _DetailMask;
		uniform float4 _DetailMask_ST;
		uniform sampler2D _TopAlbedo;
		uniform sampler2D _BottomAlbedo;
		uniform float4 _Color;
		uniform float _DetailAlbedoPower;
		uniform sampler2D _TopMetalicRAmbientOcclusionGSmoothnessA;
		uniform sampler2D _BottomMetalicRAmbientOcclusionGSmoothnessA;
		uniform float _MetallicPower;
		uniform float _SmoothnessPower;
		uniform float _AmbientOcclusionPower;
		uniform sampler2D _ShapeAmbientOcclusionG;
		uniform float _ShapeAmbientOcclusionPower;


		inline float3 TriplanarSamplingCNF( sampler2D topTexMap, sampler2D midTexMap, sampler2D botTexMap, float3 worldPos, float3 worldNormal, float falloff, float tilling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= projNormal.x + projNormal.y + projNormal.z;
			float3 nsign = sign( worldNormal );
			float negProjNormalY = max( 0, projNormal.y * -nsign.y );
			projNormal.y = max( 0, projNormal.y * nsign.y );
			half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
			xNorm = ( tex2D( midTexMap, tilling * worldPos.zy * float2( nsign.x, 1.0 ) ) );
			yNorm = ( tex2D( topTexMap, tilling * worldPos.xz * float2( nsign.y, 1.0 ) ) );
			yNormN = ( tex2D( botTexMap, tilling * worldPos.xz * float2( nsign.y, 1.0 ) ) );
			zNorm = ( tex2D( midTexMap, tilling * worldPos.xy * float2( -nsign.z, 1.0 ) ) );
			xNorm.xyz = half3( UnpackScaleNormal( xNorm, normalScale.y ).xy * float2( nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
			yNorm.xyz = half3( UnpackScaleNormal( yNorm, normalScale.x ).xy * float2( nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
			zNorm.xyz = half3( UnpackScaleNormal( zNorm, normalScale.y ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
			yNormN.xyz = half3( UnpackScaleNormal( yNormN, normalScale.z ).xy * float2( nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
			return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + yNormN.xyz * negProjNormalY + zNorm.xyz * projNormal.z );
		}


		inline float4 TriplanarSamplingCF( sampler2D topTexMap, sampler2D midTexMap, sampler2D botTexMap, float3 worldPos, float3 worldNormal, float falloff, float tilling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= projNormal.x + projNormal.y + projNormal.z;
			float3 nsign = sign( worldNormal );
			float negProjNormalY = max( 0, projNormal.y * -nsign.y );
			projNormal.y = max( 0, projNormal.y * nsign.y );
			half4 xNorm; half4 yNorm; half4 yNormN; half4 zNorm;
			xNorm = ( tex2D( midTexMap, tilling * worldPos.zy * float2( nsign.x, 1.0 ) ) );
			yNorm = ( tex2D( topTexMap, tilling * worldPos.xz * float2( nsign.y, 1.0 ) ) );
			yNormN = ( tex2D( botTexMap, tilling * worldPos.xz * float2( nsign.y, 1.0 ) ) );
			zNorm = ( tex2D( midTexMap, tilling * worldPos.xy * float2( -nsign.z, 1.0 ) ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + yNormN * negProjNormalY + zNorm * projNormal.z;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float temp_output_422_0 = ( 1.0 / _Tiling );
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float3 appendResult461 = (float3(_TopNormalScale , _BottomNormalScale , _BottomNormalScale));
			float3 triplanar409 = TriplanarSamplingCNF( _TopNormal, _BottomNormal, _BottomNormal, ase_worldPos, ase_worldNormal, _TriplanarFallOff, temp_output_422_0, appendResult461, float3(0,0,0) );
			float3 tanTriplanarNormal409 = mul( ase_worldToTangent, triplanar409 );
			float2 uv_ShapeBumpMap = i.uv_texcoord * _ShapeBumpMap_ST.xy + _ShapeBumpMap_ST.zw;
			float3 temp_output_455_0 = BlendNormals( tanTriplanarNormal409 , UnpackScaleNormal( tex2D( _ShapeBumpMap, uv_ShapeBumpMap ) ,_ShapeBumpMapScale ) );
			float2 uv_DetailAlbedoMap = i.uv_texcoord * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
			float2 uv_DetailMask = i.uv_texcoord * _DetailMask_ST.xy + _DetailMask_ST.zw;
			float4 tex2DNode447 = tex2D( _DetailMask, uv_DetailMask );
			float3 lerpResult441 = lerp( temp_output_455_0 , BlendNormals( temp_output_455_0 , UnpackScaleNormal( tex2D( _DetailNormalMap, uv_DetailAlbedoMap ) ,_DetailNormalMapScale ) ) , tex2DNode447.a);
			o.Normal = lerpResult441;
			float4 triplanar406 = TriplanarSamplingCF( _TopAlbedo, _BottomAlbedo, _BottomAlbedo, ase_worldPos, ase_worldNormal, _TriplanarFallOff, temp_output_422_0, float3( 1,1,1 ), float3(0,0,0) );
			float4 temp_output_459_0 = ( triplanar406 * _Color );
			float4 blendOpSrc433 = temp_output_459_0;
			float4 blendOpDest433 = ( _DetailAlbedoPower * tex2D( _DetailAlbedoMap, uv_DetailAlbedoMap ) );
			float4 lerpResult434 = lerp( temp_output_459_0 , (( blendOpDest433 > 0.5 ) ? ( 1.0 - ( 1.0 - 2.0 * ( blendOpDest433 - 0.5 ) ) * ( 1.0 - blendOpSrc433 ) ) : ( 2.0 * blendOpDest433 * blendOpSrc433 ) ) , ( _DetailAlbedoPower * tex2DNode447.a ));
			o.Albedo = lerpResult434.rgb;
			float4 triplanar414 = TriplanarSamplingCF( _TopMetalicRAmbientOcclusionGSmoothnessA, _BottomMetalicRAmbientOcclusionGSmoothnessA, _BottomMetalicRAmbientOcclusionGSmoothnessA, ase_worldPos, ase_worldNormal, _TriplanarFallOff, temp_output_422_0, float3( 1,1,1 ), float3(0,0,0) );
			o.Metallic = ( triplanar414.x * _MetallicPower );
			o.Smoothness = ( triplanar414.w * _SmoothnessPower );
			float clampResult96 = clamp( triplanar414.y , ( 1.0 - _AmbientOcclusionPower ) , 1.0 );
			float clampResult450 = clamp( tex2D( _ShapeAmbientOcclusionG, uv_ShapeBumpMap ).g , ( 1.0 - _ShapeAmbientOcclusionPower ) , 1.0 );
			o.Occlusion = ( clampResult96 + clampResult450 );
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}