// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "ME/Object Blink (Transparent)" {
    
    Properties {
        
        _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Main Color", Color) = (1,1,1,1)
        
        _Gloss("Gloss Texture", 2D) = "white" {}
        _GlossColor("Gloss Color", Color) = (1,1,1,1)
        _GlossRotation("Gloss Rotation", Range(0, 360)) = 0
        _GlossWidth("Gloss Width", float) = 1
        _GlossProgressFrom("Gloss Progress (From)", float) = 0
        _GlossProgressTo("Gloss Progress (To)", float) = 0
        _GlossTime("Gloss Time", float) = 1
        _GlossDelay("Gloss Delay", float) = 2
        [MaterialToggle] _MainTexTransparent("Main Texture Is Transparent", float) = 1

    }

    Category {
        
        Tags {
            
            "Queue" = "Transparent"
            "RenderType" = "Transparent"

        }

        Cull Off
        //Lighting Off
        //ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        SubShader {
            
            Pass {
                
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag Lambert noforwardadd
                #define SCREENPOS 0
				#include "UnityCG.cginc"
                #include "BlinkCG.cginc"

                half4 _Color;
                half4 frag(v2f IN) : SV_Target {

                	half3 lambert = saturate(dot(IN.lightDir, IN.normal));
                	half4 c = fragDo(IN, IN.color * _Color);
                	c.rgb *= lambert.rgb;

                	return c;

                }
                ENDCG

            }

        }

    }

}
