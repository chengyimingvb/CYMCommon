Shader "VolumetricFogAndMist/Blur" {
Properties {
	_MainTex ("", 2D) = "white" {}
	_Color ("", Color) = (1,1,1,1)
}

SubShader {

	CGINCLUDE
	#include "UnityCG.cginc"

    struct appdata {
    	float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
    };

	struct v2fCross {
	    float4 pos : SV_POSITION;
	    float2 uv: TEXCOORD0;
	    float2 uv1: TEXCOORD1;
	    float2 uv2: TEXCOORD2;
	    float2 uv3: TEXCOORD3;
	    float2 uv4: TEXCOORD4;
		UNITY_VERTEX_INPUT_INSTANCE_ID
		UNITY_VERTEX_OUTPUT_STEREO
	};
	
	UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
	float4 _MainTex_ST;
	float4 _MainTex_TexelSize;
	UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
	float _BlurDepth;
	
	v2fCross vertBlurH(appdata v) {
	    v2fCross o;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_TRANSFER_INSTANCE_ID(v, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    	o.pos = UnityObjectToClipPos(v.vertex);
		#if UNITY_UV_STARTS_AT_TOP
    	if (_MainTex_TexelSize.y < 0) {
	        // Texture is inverted WRT the main texture
    	    v.texcoord.y = 1.0 - v.texcoord.y;
    	}
    	#endif   
    	o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
		float2 inc = float2(_MainTex_TexelSize.x * 1.3846153846, 0);	
		#if UNITY_SINGLE_PASS_STEREO
		inc.x *= 2.0;
		#endif
    	o.uv1 = UnityStereoScreenSpaceUVAdjust(v.texcoord - inc, _MainTex_ST);	
    	o.uv2 = UnityStereoScreenSpaceUVAdjust(v.texcoord + inc, _MainTex_ST);	
		float2 inc2 = float2(_MainTex_TexelSize.x * 3.2307692308, 0);	
		#if UNITY_SINGLE_PASS_STEREO
		inc2.x *= 2.0;
		#endif
		o.uv3 = UnityStereoScreenSpaceUVAdjust(v.texcoord - inc2, _MainTex_ST);
    	o.uv4 = UnityStereoScreenSpaceUVAdjust(v.texcoord + inc2, _MainTex_ST);	
		return o;

	}	
	
	v2fCross vertBlurV(appdata v) {
    	v2fCross o;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_TRANSFER_INSTANCE_ID(v, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    	o.pos = UnityObjectToClipPos(v.vertex);
		#if UNITY_UV_STARTS_AT_TOP
    	if (_MainTex_TexelSize.y < 0) {
	        // Texture is inverted WRT the main texture
    	    v.texcoord.y = 1.0 - v.texcoord.y;
    	}
    	#endif   
    	o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
    	float2 inc = float2(0, _MainTex_TexelSize.y * 1.3846153846);	
    	o.uv1 = UnityStereoScreenSpaceUVAdjust(v.texcoord - inc, _MainTex_ST);	
    	o.uv2 = UnityStereoScreenSpaceUVAdjust(v.texcoord + inc, _MainTex_ST);	
    	float2 inc2 = float2(0, _MainTex_TexelSize.y * 3.2307692308);	
    	o.uv3 = UnityStereoScreenSpaceUVAdjust(v.texcoord - inc2, _MainTex_ST);	
    	o.uv4 = UnityStereoScreenSpaceUVAdjust(v.texcoord + inc2, _MainTex_ST);	
    	return o;
	}
	
	#define PIX(uv) UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex,uv)
	
	float4 fragBlur (v2fCross i): SV_Target {
		UNITY_SETUP_INSTANCE_ID(i);
		float depth01 = Linear01Depth(UNITY_SAMPLE_DEPTH(UNITY_SAMPLE_SCREENSPACE_TEXTURE(_CameraDepthTexture, i.uv)));
		float4 pixel0 = PIX(i.uv);
		if (depth01>_BlurDepth) {
			pixel0 = pixel0 * 0.2270270270
					+ (PIX(i.uv1) + PIX(i.uv2)) * 0.3162162162
					+ (PIX(i.uv3) + PIX(i.uv4)) * 0.0702702703;
			pixel0 *= 1.05;
		}
        pixel0 = clamp(pixel0, 0.0.xxxx, 1.05.xxxx);
		return pixel0;
	}	
	
	
	ENDCG

	Pass { // Blur horizontally
	    ZTest Always Cull Off ZWrite Off
       	Fog { Mode Off }
		CGPROGRAM
		#pragma vertex vertBlurH
		#pragma fragment fragBlur
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma target 3.0
		ENDCG
	}
	
	Pass { // Blur vertically
	    ZTest Always Cull Off ZWrite Off
       	Fog { Mode Off }
		CGPROGRAM
		#pragma vertex vertBlurV
		#pragma fragment fragBlur
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma target 3.0
		ENDCG
	}
	
}

Fallback Off
}
