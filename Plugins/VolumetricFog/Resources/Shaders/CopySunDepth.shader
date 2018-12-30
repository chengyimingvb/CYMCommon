Shader "VolumetricFogAndMist/CopySunDepth" {
Properties {
	_MainTex ("", 2D) = "black" {}
	_VF_ShadowBias ("Shadow Bias", Float) = 0.1
}

SubShader {
	Tags { "RenderType"="Opaque" }
	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma multi_compile_instancing
		#pragma instancing_options assumeuniformscaling maxcount:50
		#include "UnityCG.cginc"

		struct appdata {
			float4 vertex : POSITION;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2f {
    		float4 pos : SV_POSITION;
    		float3 wpos: TEXCOORD0;
//    		UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		float _VF_ShadowBias;
		v2f vert( appdata v ) {
    		v2f o;
    		UNITY_SETUP_INSTANCE_ID(v);
//    		UNITY_TRANSFER_INSTANCE_ID(v, o);
    		o.pos = UnityObjectToClipPos(v.vertex);
    		o.wpos = mul(unity_ObjectToWorld, v.vertex);
    		return o;
		}
		float4 frag(v2f i) : SV_Target {
//			UNITY_SETUP_INSTANCE_ID(i);
    		float dist = 1.0 / (distance(_WorldSpaceCameraPos, i.wpos) + _VF_ShadowBias);
			return EncodeFloatRGBA(dist);
		}
		ENDCG
	}
}


SubShader {
	Tags { "RenderType"="TransparentCutout" }
	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"
		struct v2f {
    		float4 pos : SV_POSITION;
    		float2 uv : TEXCOORD0;
    		float3 wpos: TEXCOORD1;
		};
		uniform sampler2D _MainTex;
		uniform fixed _Cutoff;
		uniform fixed4 _Color;
		float _VF_ShadowBias;

		v2f vert( appdata_base v ) {
    		v2f o;
    		o.pos = UnityObjectToClipPos(v.vertex);
    		o.wpos = mul(unity_ObjectToWorld, v.vertex);
    		o.uv = v.texcoord;
    		return o;
		}
		float4 frag(v2f i) : SV_Target {
			fixed4 texcol = tex2D( _MainTex, i.uv );
			clip( texcol.a*_Color.a - _Cutoff );
    		float dist = 1.0 / (distance(_WorldSpaceCameraPos, i.wpos) + _VF_ShadowBias);
			return EncodeFloatRGBA(dist);
		}
		ENDCG
	}
}

SubShader {
	Tags { "RenderType"="TreeOpaque" }
	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"
		#include "TerrainEngine.cginc"
		struct v2f {
    		float4 pos : SV_POSITION;
    		float3 wpos: TEXCOORD0;
		};
		float _VF_ShadowBias;

		v2f vert( appdata_full v ) {
    		v2f o;
    		TerrainAnimateTree(v.vertex, v.color.w);
    		o.pos = UnityObjectToClipPos(v.vertex);
    		o.wpos = mul(unity_ObjectToWorld, v.vertex);
    		return o;
		}
		float4 frag(v2f i) : SV_Target {
    		float dist = 1.0 / (distance(_WorldSpaceCameraPos, i.wpos) + _VF_ShadowBias);
			return EncodeFloatRGBA(dist);
		}
		ENDCG
	}
}

SubShader {
	Tags { "RenderType"="TreeBark" }
	Pass {
	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#include "UnityCG.cginc"
	#include "Lighting.cginc"
	#include "UnityBuiltin3xTreeLibrary.cginc"
	struct v2f {
	    float4 pos : SV_POSITION;
   		float3 wpos: TEXCOORD0;
	};
		float _VF_ShadowBias;

	v2f vert( appdata_full v ) {
	    v2f o;
	    TreeVertBark(v);
		o.pos = UnityObjectToClipPos(v.vertex);
   		o.wpos = mul(unity_ObjectToWorld, v.vertex);
    	return o;
	}
	float4 frag(v2f i) : SV_Target {
   		float dist = 1.0 / (distance(_WorldSpaceCameraPos, i.wpos) + _VF_ShadowBias);
		return EncodeFloatRGBA(dist);
	}
	ENDCG
	}
}


SubShader {
	Tags { "RenderType"="TreeLeaf" }
	Cull Off
	Pass {
	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#include "UnityCG.cginc"
	#include "Lighting.cginc"
	#include "UnityBuiltin3xTreeLibrary.cginc"
	struct v2f {
	    float4 pos : SV_POSITION;
    	float2 uv : TEXCOORD0;
   		float3 wpos: TEXCOORD1;
	};
	uniform sampler2D _MainTex;
	uniform fixed _Cutoff;
		float _VF_ShadowBias;

	v2f vert( appdata_full v ) {
	    v2f o;
	    TreeVertLeaf(v);
		o.pos = UnityObjectToClipPos(v.vertex);
   		o.wpos = mul(unity_ObjectToWorld, v.vertex);
    	o.uv = v.texcoord;
    	return o;
	}
	float4 frag(v2f i) : SV_Target {
		half alpha = tex2D(_MainTex, i.uv).a;
		clip (alpha - _Cutoff);
   		float dist = 1.0 / (distance(_WorldSpaceCameraPos, i.wpos) + _VF_ShadowBias);
		return EncodeFloatRGBA(dist);
	}
	ENDCG
	}
}

SubShader {
	Tags { "RenderType"="TreeTransparentCutout" }
	Pass {
	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#include "UnityCG.cginc"
	#include "TerrainEngine.cginc"
	struct v2f {
	    float4 pos : SV_POSITION;
    	float2 uv : TEXCOORD1;
   		float3 wpos: TEXCOORD2;
	};
	uniform sampler2D _MainTex;
	uniform fixed _Cutoff;
		float _VF_ShadowBias;

	v2f vert( appdata_full v ) {
	    v2f o;
	    TerrainAnimateTree(v.vertex, v.color.w);
		o.pos = UnityObjectToClipPos(v.vertex);
   		o.wpos = mul(unity_ObjectToWorld, v.vertex);
    	o.uv = v.texcoord;
    	return o;
	}
	float4 frag(v2f i) : SV_Target {
		half alpha = tex2D(_MainTex, i.uv).a;
		clip (alpha - _Cutoff);
   		float dist = 1.0 / (distance(_WorldSpaceCameraPos, i.wpos) + _VF_ShadowBias);
		return EncodeFloatRGBA(dist);
	}
	ENDCG
	}
}

Fallback Off
}
