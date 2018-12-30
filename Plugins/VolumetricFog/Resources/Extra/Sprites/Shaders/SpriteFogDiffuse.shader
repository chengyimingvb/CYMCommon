Shader "VolumetricFogAndMist/Sprites/SpriteFog Diffuse"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_Cutoff ("Alpha Cutoff", Range(0,1)) = 0.333
	}

	SubShader
	{
		Tags
		{
			"Queue"="Geometry"
			"IgnoreProjector"="True"
			"RenderType"="Opaque"
			"DisableBatching"="LODFading"
		}
		Cull Off
		
		CGPROGRAM
			#pragma surface surf Lambert vertex:vert nolightmap exclude_path:deferred exclude_path:prepass
			#include "SpriteFogCommon.cginc"

			struct Input {
				fixed4 color;
				half2 mainTexUV;
			};

			void vert(inout SpriteData IN, out Input OUT) {
				UNITY_INITIALIZE_OUTPUT(Input, OUT);
				OUT.mainTexUV = IN.texcoord.xy;
				OUT.color = IN.color * _Color; 
			}

			void surf(Input IN, inout SurfaceOutput OUT)
			{
				half4 diffuseColor = tex2D(_MainTex, IN.mainTexUV);
				half alpha = diffuseColor.a;
				clip(alpha - _Cutoff);
				OUT.Alpha = alpha * IN.color.a;
				OUT.Gloss = diffuseColor.a;
				OUT.Albedo = diffuseColor.rgb * IN.color.rgb;
			}
		ENDCG

		Pass
		{
			Tags { "LightMode" = "ShadowCaster" }
					
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_shadowcaster
				#include "SpriteFogCommon.cginc"

				struct v2f 
				{
					V2F_SHADOW_CASTER;
					half2 uv : TEXCOORD1;
				};

				v2f vert(SpriteData v)
				{
					v2f o;
					o.uv = v.texcoord.xy;
					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
					return o;
				}

				float4 frag(v2f i) : SV_Target
				{
					clip(tex2D(_MainTex, i.uv).a - _Cutoff);
					SHADOW_CASTER_FRAGMENT(i)
				}
			ENDCG
		}

		
	}

	FallBack "Transparent/Cutout/VertexLit"
}
