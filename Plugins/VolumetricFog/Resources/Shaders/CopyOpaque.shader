Shader "VolumetricFogAndMist/CopyOpaque" {
Properties {
	_MainTex ("", 2D) = "black" {}
	_BlendPower("Blend Power", Float) = 0.5
}

SubShader {
	
	Pass {
	    ZTest Always Cull Off ZWrite Off
       	Fog { Mode Off }
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma target 3.0
		#include "CopyOpaque.cginc"
		ENDCG
	}

	Pass { // with depth test
	    ZTest Always Cull Off ZWrite Off
       	Fog { Mode Off }
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma target 3.0
		#define FOG_COMPUTE_DEPTH
		#include "CopyOpaque.cginc"
		ENDCG
	}

	
	
}

Fallback Off
}
