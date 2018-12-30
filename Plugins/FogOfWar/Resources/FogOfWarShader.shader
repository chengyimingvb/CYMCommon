Shader "Hidden/FogOfWar"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_FogTex ("Fog", 2D) = "white" {}
		_FogColorTex ("Fog Color", 2D) = "white" {}
	}

	CGINCLUDE

	#include "UnityCG.cginc"
	
	uniform sampler2D _MainTex;
	uniform sampler2D _FogTex;
	uniform sampler2D_float _CameraDepthTexture;
	uniform sampler2D _FogColorTex;
	uniform float2 _FogColorTexScale;
	
	// for fast world space reconstruction
	uniform float4x4 _InverseView;

	uniform float _FogTextureSize;
	uniform float _MapSize;
	uniform float4 _MapOffset;
	uniform float4 _FogColor;
	uniform float _OutsideFogStrength;
	uniform float _StereoSeparation;

	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float3 interpolatedRay : TEXCOORD1;
	};

	v2f vert(uint vertexID : SV_VertexID)
    {
        float far = _ProjectionParams.z;
        float2 orthoSize = unity_OrthoParams.xy;
        float isOrtho = unity_OrthoParams.w; // 0: perspective, 1: orthographic

        // Vertex ID -> clip space vertex position
        float x = vertexID != 1 ? -1 : 3;
        float y = vertexID == 2 ? -3 : 1;
        float3 viewpos = float3(x, y, 1);

        // Perspective: view space vertex position of the far plane
        float3 rayPers = mul(unity_CameraInvProjection, viewpos.xyzz * far).xyz;

        // Orthographic: view space vertex position
        float3 rayOrtho = float3(orthoSize * viewpos.xy, 0);

        v2f o;
        o.pos = float4(viewpos.x, -viewpos.y, 1, 1);
        o.uv = (viewpos.xy + 1) * 0.5;
        o.interpolatedRay = lerp(rayPers, rayOrtho, isOrtho);
		
		#if UNITY_UV_STARTS_AT_TOP
		if (_ProjectionParams.x > 0)
			o.pos.y = -o.pos.y;
		#endif
		
        return o;
    }

	ENDCG

	SubShader
	{
		ZTest Always
		Cull Off
		ZWrite Off
		Fog { Mode Off }

		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "HLSLSupport.cginc"

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile CAMERA_ORTHOGRAPHIC CAMERA_PERSPECTIVE
			#pragma multi_compile PLANE_XY PLANE_YZ PLANE_XZ
			#pragma multi_compile _ TEXTUREFOG
			#pragma multi_compile _ FOGFARPLANE
			#pragma multi_compile _ FOGOUTSIDEAREA
			#pragma multi_compile _ CLEARFOG
			
			float3 ComputeViewSpacePosition(float2 texcoord, float3 ray, out float rawdpth)
			{
				// Render settings
				float near = _ProjectionParams.y;
				float far = _ProjectionParams.z;
				float isOrtho = unity_OrthoParams.w; // 0: perspective, 1: orthographic

				// Z buffer sample
				float z = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, texcoord);

				// Far plane exclusion
				#if !defined(EXCLUDE_FAR_PLANE)
				float mask = 1;
				#elif defined(UNITY_REVERSED_Z)
				float mask = z > 0;
				#else
				float mask = z < 1;
				#endif

				// Perspective: view space position = ray * depth
				rawdpth = Linear01Depth(z);
				float3 vposPers = ray * rawdpth;

				// Orthographic: linear depth (with reverse-Z support)
				#if defined(UNITY_REVERSED_Z)
				float depthOrtho = -lerp(far, near, z);
				#else
				float depthOrtho = -lerp(near, far, z);
				#endif

				// Orthographic: view space position
				float3 vposOrtho = float3(ray.xy, depthOrtho);

				// Result: view space position
				return lerp(vposPers, vposOrtho, isOrtho) * mask;
			}

			half4 frag (v2f i) : SV_Target
			{
				// for VR
				i.uv = UnityStereoTransformScreenSpaceTex(i.uv);

				float rawdpth;
				float3 viewspacepos = ComputeViewSpacePosition(i.uv, i.interpolatedRay, rawdpth);
				float3 wsPos = mul(_InverseView, float4(viewspacepos, 1)).xyz;

				// single pass VR requires the world space separation between eyes to be manually set
				#if UNITY_SINGLE_PASS_STEREO
				wsPos.x += unity_StereoEyeIndex * _StereoSeparation;
				#endif
				
				#ifdef PLANE_XY
					float2 modepos = wsPos.xy;
				#elif PLANE_YZ
					float2 modepos = wsPos.yz;
				#elif PLANE_XZ
					float2 modepos = wsPos.xz;
				#endif

				float2 mapPos = (modepos - _MapOffset.xy) / _MapSize + float2(0.5f, 0.5f);
				
				// if it is beyond the map
				float isoutsidemap = min(1, step(mapPos.x, 0) + step(1, mapPos.x) + step(mapPos.y, 0) + step(1, mapPos.y));

				// if outside map, use the outside fog color
				float fog = lerp(tex2D(_FogTex, mapPos).a, _OutsideFogStrength, isoutsidemap);

				#ifdef TEXTUREFOG
					// raycast plane
					float3 rayorigin = _CameraWS;
					float3 raydir = normalize(i.interpolatedRay);
					float3 planeorigin = float3(0, _FogColorTexScale.y, 0);
					float3 planenormal = float3(0, 1, 0);
					float t = dot(planeorigin - rayorigin, planenormal) / dot(planenormal, raydir);
					float4 fogcolor = tex2D(_FogColorTex, (rayorigin + raydir * t).xz * _FogColorTexScale.x);
				#else
					float4 fogcolor = _FogColor;
				#endif
				
				#ifndef FOGFARPLANE
					if (rawdpth == 0) // there's some weird behaviour with the far plane in some rare cases that this should fix...
						fog = 0;
					else
						fog *= step(rawdpth, 0.999); // don't show fog on the far plane
				#endif

				half4 sceneColor = tex2D(_MainTex, i.uv);

				#ifdef CLEARFOG
					return lerp(sceneColor, fogcolor, fog);
				#else
					return lerp(sceneColor, fogcolor, fog * fogcolor.a);
				#endif
			}

			ENDCG
		}
	}

	Fallback off
}
