Shader "Terrain Grid System/Unlit Single Color Territory Thick Line" {
 
Properties {
    _Color ("Color", Color) = (1,1,1,1)
    _Offset ("Depth Offset", float) = -0.01  
    _NearClip ("Near Clip", Range(0, 1000.0)) = 25.0
    _FallOff ("FallOff", Range(1, 1000.0)) = 50.0
    _Width ("Width Offset", Range(0.0001, 0.005)) = 0.0005
}
 
SubShader {
    Tags {
       "Queue"="Geometry+3"
       "RenderType"="Opaque"
  	}
    Blend SrcAlpha OneMinusSrcAlpha
  	ZWrite Off
    Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag				
		#pragma multi_compile __ NEAR_CLIP_FADE
		#include "UnityCG.cginc"			

		float _Offset;
		fixed4 _Color;
		float _NearClip;
		float _FallOff;

		struct AppData {
			float4 vertex : POSITION;
		};

		struct VertexToFragment {
			float4 pos : SV_POSITION;	
			fixed4 color: COLOR;
		};
		
		VertexToFragment vert(AppData v) {
			VertexToFragment o;							
			o.pos = UnityObjectToClipPos(v.vertex);
			#if UNITY_REVERSED_Z
			o.pos.z -= _Offset;
			#else
			o.pos.z += _Offset;
			#endif
			#if NEAR_CLIP_FADE
			if (UNITY_MATRIX_P[3][3]==1.0) {	// Orthographic camera
				o.color = _Color;
			} else {
				#if UNITY_REVERSED_Z
				o.color = fixed4(_Color.rgb, _Color.a * saturate((o.pos.z + _NearClip)/_FallOff));
				#else
				o.color = fixed4(_Color.rgb, _Color.a * saturate((o.pos.z - _NearClip)/_FallOff));
				#endif			
			}
			#else
				o.color = _Color;
			#endif
			return o;									
		}
		
		fixed4 frag(VertexToFragment i) : SV_Target {
			return i.color;
		}
			
		ENDCG
    }
    
   // SECOND STROKE ***********
 
    Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag				
		#include "UnityCG.cginc"			

		float _Offset;
		fixed4 _Color;
		float _NearClip;
		float _FallOff;
		float _Width;

		//Data structure communication from Unity to the vertex shader
		//Defines what inputs the vertex shader accepts
		struct AppData {
			float4 vertex : POSITION;
		};

		//Data structure for communication from vertex shader to fragment shader
		//Defines what inputs the fragment shader accepts
		struct VertexToFragment {
			float4 pos : SV_POSITION;	
			fixed4 color: COLOR;
		};
		
		//Vertex shader
		VertexToFragment vert(AppData v) {
			VertexToFragment o;							
			o.pos = UnityObjectToClipPos(v.vertex);
			o.pos.x += 2 * (o.pos.w/_ScreenParams.x);
#if UNITY_REVERSED_Z
			o.pos.z -= _Offset;
#else
			o.pos.z += _Offset;
#endif
			if (UNITY_MATRIX_P[3][3]==1.0) {	// Orthographic camera
				o.color = _Color;
			} else {
				o.color = fixed4(_Color.rgb, _Color.a * saturate((o.pos.z - _NearClip)/_FallOff));
			}	
			return o;									
		}
		
		fixed4 frag(VertexToFragment i) : SV_Target {
			return i.color;
		}
			
		ENDCG
    }
    
      // THIRD STROKE ***********
 
	Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag				
		#include "UnityCG.cginc"			

		float _Offset;
		fixed4 _Color;
		float _NearClip;
		float _FallOff;
		float _Width;

		//Data structure communication from Unity to the vertex shader
		//Defines what inputs the vertex shader accepts
		struct AppData {
			float4 vertex : POSITION;
		};

		//Data structure for communication from vertex shader to fragment shader
		//Defines what inputs the fragment shader accepts
		struct VertexToFragment {
			float4 pos : SV_POSITION;	
			fixed4 color: COLOR;
		};
		
		//Vertex shader
		VertexToFragment vert(AppData v) {
			VertexToFragment o;							
			o.pos = UnityObjectToClipPos(v.vertex);
			o.pos.y += 2 * (o.pos.w/_ScreenParams.y);
#if UNITY_REVERSED_Z
			o.pos.z -= _Offset;
#else
			o.pos.z += _Offset;
#endif
			if (UNITY_MATRIX_P[3][3]==1.0) {	// Orthographic camera
				o.color = _Color;
			} else {
				o.color = fixed4(_Color.rgb, _Color.a * saturate((o.pos.z - _NearClip)/_FallOff));
			}			
			return o;									
		}
		
		fixed4 frag(VertexToFragment i) : SV_Target {
			return i.color;
		}
			
		ENDCG
    }
    
       
      // FOURTH STROKE ***********
 
  Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag				
		#include "UnityCG.cginc"			

		float _Offset;
		fixed4 _Color;
		float _NearClip;
		float _FallOff;
		float _Width;

		//Data structure communication from Unity to the vertex shader
		//Defines what inputs the vertex shader accepts
		struct AppData {
			float4 vertex : POSITION;
		};

		//Data structure for communication from vertex shader to fragment shader
		//Defines what inputs the fragment shader accepts
		struct VertexToFragment {
			float4 pos : SV_POSITION;	
			fixed4 color: COLOR;
		};
		
		//Vertex shader
		VertexToFragment vert(AppData v) {
			VertexToFragment o;							
			o.pos = UnityObjectToClipPos(v.vertex);
			o.pos.x -= 2 * (o.pos.w/_ScreenParams.x);
#if UNITY_REVERSED_Z
			o.pos.z -= _Offset;
#else
			o.pos.z += _Offset;
#endif
			if (UNITY_MATRIX_P[3][3]==1.0) {	// Orthographic camera
				o.color = _Color;
			} else {
				o.color = fixed4(_Color.rgb, _Color.a * saturate((o.pos.z - _NearClip)/_FallOff));
			}
			return o;									
		}
		
		fixed4 frag(VertexToFragment i) : SV_Target {
			return i.color;
		}
			
		ENDCG
    }
    
    // FIFTH STROKE ***********
 
	Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag				
		#include "UnityCG.cginc"			

		float _Offset;
		fixed4 _Color;
		float _NearClip;
		float _FallOff;
		float _Width;

		//Data structure communication from Unity to the vertex shader
		//Defines what inputs the vertex shader accepts
		struct AppData {
			float4 vertex : POSITION;
		};

		//Data structure for communication from vertex shader to fragment shader
		//Defines what inputs the fragment shader accepts
		struct VertexToFragment {
			float4 pos : SV_POSITION;	
			fixed4 color: COLOR;
		};
		
		//Vertex shader
		VertexToFragment vert(AppData v) {
			VertexToFragment o;							
			o.pos = UnityObjectToClipPos(v.vertex);
			o.pos.y -= 2 * (o.pos.w/_ScreenParams.y);
#if UNITY_REVERSED_Z
			o.pos.z -= _Offset;
#else
			o.pos.z += _Offset;
#endif
			if (UNITY_MATRIX_P[3][3]==1.0) {	// Orthographic camera
				o.color = _Color;
			} else {
				o.color = fixed4(_Color.rgb, _Color.a * saturate((o.pos.z - _NearClip)/_FallOff));
			}
			return o;									
		}
		
		fixed4 frag(VertexToFragment i) : SV_Target {
			return i.color;
		}
			
		ENDCG
    }
    

}
}
