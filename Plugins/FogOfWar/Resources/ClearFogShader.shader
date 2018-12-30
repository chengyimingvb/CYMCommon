// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/FogOfWarClearFog"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_SkyboxTex ("Fog", 2D) = "white" {}
	}

	CGINCLUDE

	#include "UnityCG.cginc"
	
	uniform sampler2D _MainTex;
	uniform float4 _MainTex_TexelSize;
	uniform sampler2D _SkyboxTex;

	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};
	
	v2f vert (appdata_img v)
	{
		v2f o;
		half index = v.vertex.z;
		v.vertex.z = 0.1;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		
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

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			half4 frag (v2f i) : SV_Target
			{
				// for VR
				i.uv = UnityStereoTransformScreenSpaceTex(i.uv);

				half4 forecolor = tex2D(_MainTex, i.uv);
				half4 backcolor = tex2D(_SkyboxTex, i.uv);
				return half4(lerp(backcolor.rgb, forecolor.rgb, forecolor.a), 1);
			}

			ENDCG
		}
	}

	Fallback off
}
