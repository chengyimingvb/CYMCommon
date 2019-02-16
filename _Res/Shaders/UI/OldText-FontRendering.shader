// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Clear Text Shader for Unity 4.5 and lower " {
	Properties {
		_MainTex ("Font Texture", 2D) = "white" {}
	}

	SubShader {

		Tags {
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
		}
		Lighting Off Cull Off ZTest Always ZWrite Off Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord[3]  : TEXCOORD0;
			};

			sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float4 _MainTex_TexelSize;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				half2 texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.texcoord[0]  = texcoord;
				o.texcoord[1] = TRANSFORM_TEX(v.texcoord + float2(+_MainTex_TexelSize.x/500.0f,0),_MainTex);
				o.texcoord[2] = TRANSFORM_TEX(v.texcoord + float2(-_MainTex_TexelSize.x/500.0f,0),_MainTex);
				o.color = v.color;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				
				float c = tex2D(_MainTex, i.texcoord[0]).a ;
				float r = tex2D(_MainTex, i.texcoord[1]).a ;
				float l = tex2D(_MainTex, i.texcoord[2]).a ;

				half4 color = i.color;

				if(c > 0.5f) {
					c *= 1.0f + c * c / 1.5f;
				}

				color.a *= c;

				if(( l < 0.35f && r < 0.35f )) {
					if(r > l && c <= r ) { 
						color.b *= 1.0f;
						color.b += 0.1f;
						color.g *= 0.5f;
						color.g += 0.05f;
						color.r *= 0.15f;
						color.r += 0.015f;
					} else if (l > r && c <= l) {
						color.r *= 1.0f;
						color.r += 0.1f;
						color.g *= 0.3f;
						color.g += 0.03f;
						color.b *= 0.25f;
						color.b += 0.025f;
					}
				}

				clip (color.a - 0.01f);
				return color;
			}
			ENDCG 
		}
	}
}
