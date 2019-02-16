
Shader "Custom/GroundTargetOverlay" {  
Properties {
    _Color ("Color", Color) = (1,1,1)
    _MainTex ("Texture", 2D) = "white"
    _Offset ("Depth Offset", Int) = -1
}
 
SubShader {
    Tags {
        "Queue"="Geometry+1"
        "RenderType"="Opaque"
    }
    Lighting On
    Blend SrcAlpha OneMinusSrcAlpha
    Material {
        Emission [_Color]
    }
   	ZTest Always
   	ZWrite Off
   	Offset [_Offset], [_Offset]
    Pass {
    	SetTexture [_MainTex] {
            Combine Texture * Primary, Texture * Primary
        }
    }
}
}  