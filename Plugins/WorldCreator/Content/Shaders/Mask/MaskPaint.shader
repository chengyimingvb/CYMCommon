// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/WorldCreator/MaskPaint"
{
  Properties
  {
    // Texture Distribution
    _Brush("BrushTexture", 2D) = "black" {}  
    _BrushPosition("BrushPosition", Vector) = (0,0,0,0)
    _BrushSize("BrushSize", float) = 1
    _BrushRotation("BrushRotation", float) = 0
    _BrushStrength("BrushStrength", float) = 0.2

  }
    SubShader
    {
      Cull Off ZWrite Off ZTest Always
      Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }

      Pass
      {
        Blend One One
        BlendOp Add
      
        CGPROGRAM
        #pragma target 3.0
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"


        struct appdata
        {
          float4 vertex : POSITION;
          float2 uv : TEXCOORD0;
        };
      
        struct v2f
        {
          float2 uv : TEXCOORD0;
          float4 vertex : SV_POSITION;
        };
        
        sampler2D _Brush;
        float4 _BrushPosition;
        float _BrushSize;
        float _BrushRotation;
        float _BrushStrength;


        v2f vert(appdata v)
        {
          v2f o;
          o.vertex = UnityObjectToClipPos(v.vertex);
          o.uv = v.uv;
          return o;
        }

        fixed4 frag(v2f input) : SV_Target
        {
          float2 brushMin = _BrushPosition - _BrushSize.xx * 0.5f;
          float2 pixPos = (input.uv - brushMin) / _BrushSize.xx;
          
          pixPos -= 0.5f;
          float cR = cos(_BrushRotation);
          float sR = sin(_BrushRotation);
          pixPos = float2(pixPos.x * cR - pixPos.y * sR, pixPos.x * sR + pixPos.y * cR);
          pixPos += 0.5f;
           
          float brushBorder = step(pixPos.x, 1) * step(0, pixPos.x) *
                              step(pixPos.y, 1) * step(0, pixPos.y);
      
          float brushValue = tex2D(_Brush, pixPos).r * _BrushStrength * brushBorder;
          return fixed4(brushValue.xxx,1);
        }
        ENDCG
      }
      
      Pass
      {
        Blend One One
        BlendOp RevSub
      
        CGPROGRAM
        #pragma target 3.0
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"


        struct appdata
        {
          float4 vertex : POSITION;
          float2 uv : TEXCOORD0;
        };
      
        struct v2f
        {
          float2 uv : TEXCOORD0;
          float4 vertex : SV_POSITION;
        }; 
        
        sampler2D _Brush;
        float4 _BrushPosition;
        float _BrushRotation;
        float _BrushSize;
        float _BrushStrength;


        v2f vert(appdata v)
        {
          v2f o;
          o.vertex = UnityObjectToClipPos(v.vertex);
          o.uv = v.uv;
          return o;
        }

        fixed4 frag(v2f input) : SV_Target
        {
          float2 brushMin = _BrushPosition - _BrushSize.xx * 0.5f;
          float2 pixPos = (input.uv - brushMin) / _BrushSize.xx;
      
          pixPos -= 0.5f;
          float cR = cos(_BrushRotation);
          float sR = sin(_BrushRotation);
          pixPos = float2(pixPos.x * cR - pixPos.y * sR, pixPos.x * sR + pixPos.y * cR);
          pixPos += 0.5f;
      
          float brushBorder = step(pixPos.x, 1.0f) * step(0.0f, pixPos.x) *
                              step(pixPos.y, 1.0f) * step(0.0f, pixPos.y);
      
          float brushValue = tex2D(_Brush, pixPos).r * _BrushStrength * brushBorder;
          return fixed4(brushValue.xxx,1);
        }
        ENDCG
      }
    }
}
