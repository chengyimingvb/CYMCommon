Shader "Terrain Grid System/Unlit Single Color Cell Geo" {
 
Properties {
    _Color ("Color", Color) = (1,1,1,1)
    _Offset ("Depth Offset", float) = -0.01  
    _NearClip ("Near Clip", Range(0, 1000.0)) = 25.0
    _FallOff ("FallOff", Range(1, 1000.0)) = 50.0
    _Thickness ("Thickness", Float) = 0.05
}

SubShader {
    Tags {
      "Queue"="Geometry+2"
      "RenderType"="Opaque"
    }
    Blend SrcAlpha OneMinusSrcAlpha
  	ZWrite Off
  	Cull Off
    Pass {
	   	CGPROGRAM
		#pragma vertex vert	
		#pragma geometry geom
		#pragma fragment frag				
		#pragma target 4.0
		#pragma multi_compile __ NEAR_CLIP_FADE
		#include "UnityCG.cginc"
		
		fixed4 _Color;
		float _Thickness;
		float _Offset;
		float _NearClip;
		float _FallOff;


		struct v2g {
			float4 vertex : POSITION;
			fixed4 color  : COLOR;
		};

		struct g2f {
			float4 pos    : SV_POSITION;
			fixed4 color  : COLOR;
		};
		
		void vert(inout v2g v) {
			v.vertex = UnityObjectToClipPos(v.vertex);
			#if UNITY_REVERSED_Z
				v.vertex.z -= _Offset;
			#else
				v.vertex.z += _Offset;
			#endif
			#if NEAR_CLIP_FADE
			if (UNITY_MATRIX_P[3][3]==1.0) {	// Orthographic camera
				v.color = _Color;
			} else {
				#if UNITY_REVERSED_Z
				v.color = fixed4(_Color.rgb, _Color.a * saturate((v.vertex.z + _NearClip)/_FallOff));
				#else
				v.color = fixed4(_Color.rgb, _Color.a * saturate((v.vertex.z - _NearClip)/_FallOff));
				#endif
			}
			#else
				v.color = _Color;
			#endif
		}

		[maxvertexcount(6)]
        void geom(line v2g p[2], inout TriangleStream<g2f> outputStream) {
           float4 p0 = p[0].vertex;
           float4 p1 = p[1].vertex;

           float4 ab = p1 - p0;
           float4 normal = float4(-ab.y, ab.x, 0, 0);
           normal.xy = normalize(normal.xy) * _Thickness;

           float4 tl = p0 - normal;
           float4 bl = p0 + normal;
           float4 tr = p1 - normal;
           float4 br = p1 + normal;
  		   float4 dd = float4(normalize(p1.xy-p0.xy), 0, 0) * _Thickness;

           g2f pIn;
           pIn.color = p[0].color;
           pIn.pos = p0 - dd;
           outputStream.Append(pIn);
           pIn.pos = bl;
           outputStream.Append(pIn);
           pIn.pos = tl;
           outputStream.Append(pIn);
           pIn.color = p[1].color;
           pIn.pos = br;
           outputStream.Append(pIn);
           pIn.pos = tr;
           outputStream.Append(pIn);
           pIn.pos = p1 + dd;
           outputStream.Append(pIn);
 		}
		
		fixed4 frag(g2f i) : SV_Target {
			return i.color;
		}
		ENDCG
    }
            
 }
 Fallback Off
}
