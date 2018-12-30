Shader "VolumetricFogAndMist/CopyDepthAndTrans" {
Properties {
	_MainTex ("", 2D) = "white" {}
	_Cutoff ("", Float) = 0.5
	_Color ("", Color) = (1,1,1,1)
}

SubShader {
	Tags { "RenderType"="TreeBillboard" }
	Pass {
	Cull Off
	ColorMask 0
	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#include "UnityCG.cginc"
	#include "TerrainEngine.cginc"

	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};

	v2f vert (appdata_tree_billboard v) {
		v2f o;
		UNITY_SETUP_INSTANCE_ID(v);
		TerrainBillboardTree(v.vertex, v.texcoord1.xy, v.texcoord.y);
		o.pos =  UnityObjectToClipPos(v.vertex);
		o.uv.x = v.texcoord.x;
		o.uv.y = v.texcoord.y > 0;
		return o;
	}
	uniform sampler2D _MainTex;
	float4 frag(v2f i) : SV_Target {
		fixed4 texcol = tex2D(_MainTex, i.uv);
		clip( texcol.a - 0.001 );
		return 0;
	}
	ENDCG
	}
}


SubShader{
	Tags { "RenderType" = "TreeTransparentCutout" }
	Cull Off
	ColorMask 0
	Pass{
	CGPROGRAM
	#pragma vertex leaves
	#pragma fragment frag
	#include "UnityBuiltin2xTreeLibrary.cginc"

	sampler2D _MainTex;
	fixed _Cutoff;

	fixed4 frag(v2f input) : SV_Target {
		fixed4 c = tex2D(_MainTex, input.uv.xy);
		clip(c.a - _Cutoff);
		return 0;
	}
	ENDCG
	}
}


SubShader {
	Tags { "RenderType"="Transparent" }
	ColorMask 0
	Pass {
	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#include "UnityCG.cginc"

	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv  : TEXCOORD0;
	};

	v2f vert (appdata_base v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv  = v.texcoord.xy;
		return o;
	}

	sampler2D _MainTex;
	fixed _VFM_CutOff;

	float4 frag(v2f i) : SV_Target {
		fixed4 c = tex2D(_MainTex, i.uv);
		clip(c.a - _VFM_CutOff);
		return 0;
	}
	ENDCG
	}
}


Fallback Off
}
