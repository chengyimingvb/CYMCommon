Shader "VolumetricFogAndMist/Sprites/SpriteFog Diffuse Instancing"
{
	Properties
	{
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
		_Cutoff ("Alpha Cutoff", Range(0,1)) = 0.333
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags
		{
			"Queue"="AlphaTest"
			"IgnoreProjector"="True"
			"RenderType"="TransparentCutout"
			"DisableBatching"="LODFading"
			"CanUseSpriteAtlas"="True"
		}
		Cull Off
		Pass {
		Tags { "LightMode"="ForwardBase" }
		CGPROGRAM
			#pragma vertex SpriteVert
			#pragma fragment SpriteFragClip
			#pragma multi_compile_instancing
        	#pragma multi_compile _ PIXELSNAP_ON

        	#include "UnityCG.cginc"
        	#include "Lighting.cginc"

#ifdef UNITY_INSTANCING_ENABLED

    UNITY_INSTANCING_BUFFER_START(PerDrawSprite)
        // SpriteRenderer.Color while Non-Batched/Instanced.
        UNITY_DEFINE_INSTANCED_PROP(fixed4, unity_SpriteRendererColorArray)
    UNITY_INSTANCING_BUFFER_END(PerDrawSprite)

    #define _RendererColor  UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteRendererColorArray)

#endif // instancing

CBUFFER_START(UnityPerDrawSprite)
#ifndef UNITY_INSTANCING_ENABLED
    fixed4 _RendererColor;
#endif
CBUFFER_END

// Material Color.
fixed4 _Color;

struct appdata_t
{
    float4 vertex   : POSITION;
    float4 color    : COLOR;
    float2 texcoord : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 vertex   : SV_POSITION;
    fixed4 color    : COLOR;
    float2 texcoord : TEXCOORD0;
    float3 worldNormal  : TEXCOORD1;
    UNITY_VERTEX_OUTPUT_STEREO
};

v2f SpriteVert(appdata_t IN)
{
    v2f OUT;

    UNITY_SETUP_INSTANCE_ID (IN);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

    OUT.vertex = UnityObjectToClipPos(IN.vertex);
    OUT.texcoord = IN.texcoord;
    OUT.color = IN.color * _Color * _RendererColor;
    OUT.worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, float3(0,0,-1)));

    #ifdef PIXELSNAP_ON
    OUT.vertex = UnityPixelSnap (OUT.vertex);
    #endif

    return OUT;
}

sampler2D _MainTex;
sampler2D _AlphaTex;
fixed _Cutoff;

fixed4 SpriteFragClip(v2f i) : SV_Target
{
    fixed4 c = tex2D (_MainTex, i.texcoord) * i.color;
    c.rgb *= c.a;
    clip(c.a - _Cutoff);
    fixed ndotl = saturate(dot(i.worldNormal, _WorldSpaceLightPos0.xyz));
	fixed3 lighting = ndotl * _LightColor0;
    lighting += ShadeSH9(half4(i.worldNormal, 1.0));
    c.rgb *= lighting;
    return c;
}

		ENDCG
}

		Pass
		{
			Tags { "LightMode" = "ShadowCaster" }
			Cull Off

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_shadowcaster
            	#pragma multi_compile_instancing

            	#include "UnityCG.cginc"

				sampler2D _MainTex;
				fixed _Cutoff;

				struct v2f 
				{
					V2F_SHADOW_CASTER;
					half2 uv : TEXCOORD1;
					UNITY_VERTEX_OUTPUT_STEREO
				};

				v2f vert(appdata_base v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					o.uv = v.texcoord.xy;
					TRANSFER_SHADOW_CASTER(o);
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

