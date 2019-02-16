// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'

Shader "Custom/PointToObject" {

   Properties {
      _PointTexture ("Projected Image", 2D) = "white" {}
      _PointAlpha ("PointAlpha", float) = 0.5
   }
   SubShader {

      Pass {      
         Blend One OneMinusSrcAlpha 
            // add color of _PointTexture to the color in the framebuffer 
         ZWrite Off // don't change depths
         Offset -1, -1 // avoid depth fighting

         CGPROGRAM
 
         #pragma vertex vert  
         #pragma fragment frag 
 
         // User-specified properties
         uniform sampler2D _PointTexture; 
 
         // Projector-specific uniforms
         uniform float4x4 unity_Projector; // transformation matrix 
            // from object space to projector space 
            
             uniform float _PointAlpha;
 
          struct vertexInput {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
         };
         struct vertexOutput {
            float4 pos : SV_POSITION;
            float4 posProj : TEXCOORD0;
             float3 norm : NORMAL;
               // position in projector space
         };
 
         vertexOutput vert(vertexInput input) 
         {
            vertexOutput output;
 
            output.posProj = mul(unity_Projector, input.vertex);
            output.pos = UnityObjectToClipPos(input.vertex);
            output.norm= UnityObjectToClipPos(input.normal);
            return output;
         }
 
 
         float4 frag(vertexOutput input) : COLOR
         {
         
          float4 textureColor = tex2D(_PointTexture, input.posProj.xy / input.posProj.w); 
          
   
            
          
          
            if (input.posProj.w > 0.0&& _PointAlpha>0)// in front of projector?
            {
    
            if(input.norm.y<=0)
            textureColor=float4(0.0, 0.0, 0.0, 0.0);

              return textureColor; 

            }
            else // behind projector
            {
               return float4(0.0, 0.0, 0.0, 0.0);
            }
         }
 
         ENDCG
      }
   }  
   // The definition of a fallback shader should be commented out 
   // during development:
   // Fallback "Projector/Light"
}