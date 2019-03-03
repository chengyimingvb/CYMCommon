Shader "Terrain Grid System/Unlit Line" {
 
Properties {
    _Color ("Color", Color) = (1,1,1)
}
 
SubShader {
    Color [_Color]
    Tags {
      "Queue"="Geometry+3"
      "RenderType"="Opaque"
  	}
  	Offset -3, -3
  	ZWrite Off
    Pass {
    }
}
 
}
