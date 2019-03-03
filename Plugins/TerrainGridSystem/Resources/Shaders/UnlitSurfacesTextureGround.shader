Shader "Terrain Grid System/Unlit Surface Texture Ground" {
 
Properties {
    _Color ("Color", Color) = (1,1,1)
    _MainTex ("Texture", 2D) = "black"
    _Offset ("Depth Offset", Int) = -1
}
 
SubShader {
    Tags {
        "Queue"="Geometry+1"
        "RenderType"="Opaque"
    }
    Blend SrcAlpha OneMinusSrcAlpha
	Color [_Color]
   	ZWrite Off
   	Offset [_Offset], [_Offset]
    Pass {
    	SetTexture [_MainTex] {
            Combine Texture * Primary, Texture * Primary
        }
    }
}
}