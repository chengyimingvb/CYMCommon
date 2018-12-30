	#include "UnityCG.cginc"
	#include "VolumetricFogOptions.cginc"

    struct appdata {
    	float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
    };

	struct v2f {
	    float4 pos : SV_POSITION;
	    float2 uv: TEXCOORD0;
		float2 depthUV: TEXCOORD1;
		float2 depthUVNonStereo : TEXCOORD2;
		UNITY_VERTEX_INPUT_INSTANCE_ID
		UNITY_VERTEX_OUTPUT_STEREO
	};

	sampler2D_float _CameraDepthTexture;
	sampler2D_float _VolumetricFogDepthTexture;
	UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
	float4 _MainTex_ST;
	float4 _MainTex_TexelSize;
	UNITY_DECLARE_SCREENSPACE_TEXTURE(_VolumetricFog_OpaqueFrame);
	float _BlendPower;

	v2f vert(appdata v) {
    	v2f o;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_TRANSFER_INSTANCE_ID(v, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    	o.pos = UnityObjectToClipPos(v.vertex);
    	o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
   		o.depthUV = o.uv;
		o.depthUVNonStereo = v.texcoord;

    	#if UNITY_UV_STARTS_AT_TOP
    	if (_MainTex_TexelSize.y < 0) {
	        // Depth texture is inverted WRT the main texture
    	    o.depthUV.y = 1.0 - o.depthUV.y;
			o.depthUVNonStereo.y = 1.0 - o.depthUVNonStereo.y;
    	}
    	#endif

		return o;
	}	

	inline float getDepth(v2f i) {
		#if defined(FOG_ORTHO)
			float depth01 = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.depthUV));
			#if UNITY_REVERSED_Z
				depth01 = 1.0 - depth01;
			#endif
		#else
			float depth01 = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.depthUV)));
		#endif
		return depth01;
	}

	
	float4 frag (v2f i): SV_Target {

		float4 opaqueFrame = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_VolumetricFog_OpaqueFrame, i.uv);
		float4 transpFrame = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv);

		#if defined(FOG_COMPUTE_DEPTH)
			float depthOpaque = getDepth(i);
			float depthTex = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_VolumetricFogDepthTexture, i.depthUVNonStereo)));
			if (depthTex < depthOpaque) {
				return lerp(transpFrame, opaqueFrame, opaqueFrame.a);
			}
		#endif

		float  t = saturate(_BlendPower + (1.0 - opaqueFrame.a));
		return lerp(opaqueFrame, transpFrame, t);
	}	

