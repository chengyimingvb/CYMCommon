Shader "UI/Sprite Blink (Mask)" {
    
    Properties {
        
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _MaskTex("Mask Texture", 2D) = "white" {}
        [MaterialToggle] _MaskTexInverted("Mask Inverted", float) = 0
        _Gloss("Gloss Texture", 2D) = "white" {}
        _GlossColor("Gloss Color", Color) = (1,1,1,1)
        _GlossRotation("Gloss Rotation", Range(0, 360)) = 0
        _GlossWidth("Gloss Width", float) = 1
        _GlossProgressFrom("Gloss Progress (From)", float) = 0
        _GlossProgressTo("Gloss Progress (To)", float) = 0
        _GlossTime("Gloss Time", float) = 1
        _GlossDelay("Gloss Delay", float) = 2
        [MaterialToggle] _MainTexTransparent("Main Texture Is Transparent", float) = 1
        [MaterialToggle] PixelSnap("Pixel snap", float) = 0

        [HideInInspector] _StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255
        _ColorMask("Color Mask", Float) = 15

    }

    Category {
        
        Tags {
            
            "Queue" = "Transparent" 
            "IgnoreProjector" = "True" 
            "RenderType" = "TransparentCutout" 
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"

        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Stencil {
            
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]

        }

        SubShader {
            
            Pass {
                
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile DUMMY PIXELSNAP_ON
                #define SCREENPOS 0
                #define MASK 1
                #include "UnityCG.cginc"
                #include "BlinkCG.cginc"

                fixed4 frag(v2f IN) : SV_Target {

                	return fragDo(IN, IN.color);

                }
                ENDCG

            }

        }

    }

}
