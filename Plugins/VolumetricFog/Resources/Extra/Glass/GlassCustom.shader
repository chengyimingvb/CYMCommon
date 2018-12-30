Shader "VolumetricFogAndMist/Glass" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 1.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Geometry+500" }
		LOD 200
        ZWrite Off
        Cull Off

        GrabPass
        {
            "_BackgroundTexture"
        }


		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

        sampler2D _BackgroundTexture;

		struct Input {
            float4 grabPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

        void vert (inout appdata_full v, out Input IN) {
          float4 pos = UnityObjectToClipPos(v.vertex);
          IN.grabPos = ComputeGrabScreenPos(pos);
        }

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
            fixed4 bg = tex2Dproj(_BackgroundTexture, IN.grabPos);
            o.Albedo = 0;
			o.Emission = lerp(bg.rgb, _Color.rgb, _Color.a);
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
}
