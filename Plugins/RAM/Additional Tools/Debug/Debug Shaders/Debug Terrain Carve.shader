
Shader "Debug Terrain Carve"
{
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
	}

		SubShader
	{
		Pass
		{
			Tags{  "Queue" = "Geometry-10"  }
			ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"



			struct v2f
			{
				fixed4 diff : COLOR0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				half nl;

				float4 output = UnityObjectToClipPos(float4(unity_LightPosition[0].xyz, 1));
				output = ComputeScreenPos(output);
				float4 uv_ray = float4(2.0 * unity_LightPosition[0].xyz - 1.0, 1.0);
				float3 unity_LightPositionWorld = mul(unity_CameraInvProjection, uv_ray).xyz;
#if UNITY_REVERSED_Z
				output.z *= -1.0;
#endif


				nl = max(0, dot(worldNormal, output.xyz));;


				o.diff = nl * _LightColor0;


				o.diff.rgb += ShadeSH9(half4(worldNormal,1));
				return o;
			}

			fixed4 _Color;

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = _Color;
				col *= i.diff;
				return col;
			}
			ENDCG
		}
		Pass
			{
				Tags{  "Queue" = "Geometry-10" }

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				#include "UnityLightingCommon.cginc"



				struct v2f
				{
					fixed4 diff : COLOR0;
					float4 vertex : SV_POSITION;
				};

				v2f vert(appdata_base v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					half3 worldNormal = UnityObjectToWorldNormal(v.normal);
					half nl;

					float4 output = UnityObjectToClipPos(float4(unity_LightPosition[0].xyz, 1));
					output = ComputeScreenPos(output);
					float4 uv_ray = float4(2.0 * unity_LightPosition[0].xyz - 1.0, 1.0);
					float3 unity_LightPositionWorld = mul(unity_CameraInvProjection, uv_ray).xyz;
#if UNITY_REVERSED_Z
					output.z *= -1.0;
#endif


					nl = max(0, dot(worldNormal, output.xyz));

					o.diff = nl * _LightColor0;



					o.diff.rgb += ShadeSH9(half4(worldNormal,1));
					return o;
				}

				fixed4 _Color;

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = _Color;
					col *= i.diff;
					return col;
				}
				ENDCG
			}
	}
}