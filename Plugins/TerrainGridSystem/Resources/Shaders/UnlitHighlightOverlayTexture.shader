Shader "Terrain Grid System/Unlit Highlight Overlay Texture" {
Properties {
    _Color ("Tint Color", Color) = (1,1,1,0.5)
    _Intensity ("Intensity", Range(0.0, 2.0)) = 1.0
    _Offset ("Depth Offset", Int) = -1    
    _MainTex("Texture", 2D) = "black" {}
    _FadeAmount("Fade Amount", Float) = 0
}
SubShader {
    Tags {
        "Queue"="Geometry+230"
        "RenderType"="Transparent"
    }
    
		Offset [_Offset], [_Offset]
		Cull Off			
		Lighting Off		
		ZWrite Off			
		ZTest Always		
		Fog { Mode Off }
  		Blend One SrcAlpha
		
		Pass {
			CGPROGRAM						
			#pragma target 2.0				
			#pragma fragment frag			
			#pragma vertex vert				
			#pragma multi_compile _ TGS_TEX_HIGHLIGHT_ADDITIVE TGS_TEX_HIGHLIGHT_MULTIPLY TGS_TEX_HIGHLIGHT_COLOR TGS_TEX_HIGHLIGHT_SCALE
			#include "UnityCG.cginc"		

			sampler2D _MainTex;
			fixed4 _Color;								
			fixed _Intensity, _FadeAmount;

			struct AppData {
				float4 vertex : POSITION;	
				float2 texcoord: TEXCOORD0;			
			};

			struct VertexToFragment {
				float4 pos : SV_POSITION;	
				float2 uv: TEXCOORD0;			
			};

			VertexToFragment vert(AppData v) {
				VertexToFragment o;						
				UNITY_INITIALIZE_OUTPUT(AppData,o);				
				o.pos = UnityObjectToClipPos(v.vertex);	
				o.uv = v.texcoord;
				return o;								
			}

			fixed4 frag(VertexToFragment i) : SV_Target {
				fixed4 pixel = tex2D(_MainTex, i.uv);
				fixed4 color = _Color * _Intensity;
				#if TGS_TEX_HIGHLIGHT_ADDITIVE
				color = lerp(pixel, pixel + color, _FadeAmount);
				color.rgb *= pixel.a;
				color.a = 1;
				#elif TGS_TEX_HIGHLIGHT_MULTIPLY
				color = lerp(pixel, pixel * color, _FadeAmount);
				color.rgb *= pixel.a;
				color.a = 1;
				#elif TGS_TEX_HIGHLIGHT_COLOR
				color = lerp(pixel, 1.0 - (1.0 - pixel) * (1.0 - color), _FadeAmount);
				color.rgb *= pixel.a;
				color.a = 1;
				#else
				color = lerp(color * color.a, pixel * pixel.a, _FadeAmount);
				color.a = 1.0 - color.a;
				#endif
				return color;
			}

			ENDCG		

		}
	}	
}
