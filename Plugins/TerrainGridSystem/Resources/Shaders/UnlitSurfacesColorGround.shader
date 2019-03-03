Shader "Terrain Grid System/Unlit Surface Color Ground" {
 
Properties {
    _Color ("Color", Color) = (1,1,1,1)
    _Offset ("Depth Offset", Int) = -1
}
 
SubShader {
  	Color [_Color]
    Tags {
      "Queue"="Geometry+1"
      "RenderType"="Transparent"
  	}
  	Offset [_Offset], [_Offset]
  	Blend SrcAlpha OneMinusSrcAlpha
  	ZWrite Off
    Pass {}
    }
}
