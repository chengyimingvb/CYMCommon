Shader "VolumetricFogAndMist/MaskWrite" {

SubShader {

	CGINCLUDE
	#include "UnityCG.cginc"

    struct appdata {
    	float4 vertex : POSITION;
    };

	struct v2f {
	    float4 pos : SV_POSITION;
	};
	
	v2f vert(appdata v) {
    	v2f o;
    	o.pos = UnityObjectToClipPos(v.vertex);
		return o;
	}	
	
	fixed4 frag(v2f i): SV_Target {
		return fixed4(0,0,0,0);
	}	
	
	
	ENDCG

	Pass { 
       	Fog { Mode Off }
       	ColorMask 0
		CGPROGRAM
		#pragma target 3.0
   		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma vertex vert
		#pragma fragment frag
		ENDCG
	}
	
}

Fallback Off
}
