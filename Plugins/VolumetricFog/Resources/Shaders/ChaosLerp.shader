Shader "VolumetricFogAndMist/Chaos Lerp" {
Properties {
	_MainTex ("Noise", 2D) = "white" {}
	_Amount ("Turbulence Amount", Float) = 0
}

SubShader {

	CGINCLUDE
	#include "UnityCG.cginc"

    struct appdata {
    	float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
    };

	struct v2f {
	    float4 pos : SV_POSITION;
	    float4 uv: TEXCOORD0;
	};
	
	sampler2D_float _MainTex;
	float _Amount;
	
	v2f vert(appdata v) {
    	v2f o;
    	o.pos = UnityObjectToClipPos(v.vertex);
    	o.uv = float4(v.texcoord, 0, 0);
		return o;
	}	
	
	float4 frag(v2f i): SV_Target {
		float sint, cost;
		sincos(_Amount, sint, cost);
		float4 p0 = tex2Dlod(_MainTex, i.uv);
		float4 p1 = tex2Dlod(_MainTex, i.uv + float4(0.25,0.25,0,0));
		float  t0 = (sint + 1.0) * 0.5;
		float4 r0 = lerp(p0, p1, t0);
		float4 p2 = tex2Dlod(_MainTex, i.uv + float4(0.5,0.5,0,0));
		float4 p3 = tex2Dlod(_MainTex, i.uv + float4(0.75,0.75,0,0));
		float  t1 = (cost + 1.0) * 0.5;
		float4 r1 = lerp(p2, p3, t1);
		return max(r0, r1);
	}	
	
	
	ENDCG

	Pass { 
		ZTest Always Cull Off ZWrite Off
       	Fog { Mode Off }
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
