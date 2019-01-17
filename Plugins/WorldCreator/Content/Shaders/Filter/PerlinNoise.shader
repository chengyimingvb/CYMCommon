// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Perlin"
{
	Properties
	{
    _Scale("Scale", Float) = 0
    _Seed("Seed", Int) = 0
    _Octaves("Ocatves", Int) = 0
    _Depth("Depth", Float) = 0
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
      // #pragma target 4.0
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

      

      int _Seed;
      int _Octaves;

      float _Scale;
      float _Depth;

      //***************************************************************
      //***************************************************************
      uint Hash(uint x)
      {
        x  = x ^ _Seed;
        x += (x << 10);
        x  = x ^ (x >> 6);
        x += (x << 3);
        x  = x ^ (x >> 11);
        x += (x << 15);
        return x;
      }

      //***************************************************************
      //***************************************************************
      uint Random(int x, int y, int z)
      {
        uint r= Hash(x ^ Hash(y) ^ Hash(z)); 
        return r;
      }

      //***************************************************************
      //***************************************************************
      float NoiseWeight(float t)
      {
        float t3 = t * t * t;
        float t4 = t3 * t;

        return 6.0f * t4 * t - 15.0f * t4 + 10.0f * t3;
      }

      //***************************************************************
      //***************************************************************
      float Gradient(int x, int y, int z, float fX, float fY, float fZ)
      {
        uint h = Random(x, y, z) % 255;
        h = h & 15;
        float u = (h < 8 || h == 12 || h == 13) ? fX : fY;
        float v = (h < 4 || h == 12 || h == 13) ? fY : fZ;

        return (bool(h & 1) ? -u : u) + (bool(h & 2) ? -v : v);
      }


      //***************************************************************
      //***************************************************************
      float Noise(float3 pos)
      {
        // Get position floor values.
        int iX = int(pos.x);
        int iY = int(pos.y);
        int iZ = int(pos.z);

        // Get the fractional values of the position.
        float fX = frac(pos.x);
        float fY = frac(pos.y);
        float fZ = frac(pos.z);


        // Calculate gradients for every corner.
        float w000 = Gradient(iX, iY, iZ, fX, fY, fZ);
        float w100 = Gradient(iX + 1, iY, iZ, fX - 1, fY, fZ);
        float w010 = Gradient(iX, iY + 1, iZ, fX, fY - 1, fZ);
        float w110 = Gradient(iX + 1, iY + 1, iZ, fX - 1, fY - 1, fZ);
        float w001 = Gradient(iX, iY, iZ + 1, fX, fY, fZ - 1);
        float w101 = Gradient(iX + 1, iY, iZ + 1, fX - 1, fY, fZ - 1);
        float w011 = Gradient(iX, iY + 1, iZ + 1, fX, fY - 1, fZ - 1);
        float w111 = Gradient(iX + 1, iY + 1, iZ + 1, fX - 1, fY - 1, fZ - 1);

        // Trilinear interpolation of weights.
        float wX = NoiseWeight(fX);
        float wY = NoiseWeight(fY);
        float wZ = NoiseWeight(fZ);


        float x00 = lerp(w000, w100, wX);
        float x10 = lerp(w010, w110, wX);
        float x01 = lerp(w001, w101, wX);
        float x11 = lerp(w011, w111, wX);

        float y0 = lerp(x00, x10, wY);
        float y1 = lerp(x01, x11, wY);

        return lerp(y0, y1, wZ);
      }

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
		

			fixed4 frag (v2f input) : SV_Target
			{
        float noise = 0.0f;

        for (int i = 1; i < _Octaves; i++)
        {
          noise += Noise(float3(input.uv * _Scale * i + float2(i,i) * 3.67f, _Depth)) * (1.0f / i);
        }

        noise = (noise + 1.0f) * 0.5f;

        return fixed4(noise.xxx, 1.0f);
			}
			ENDCG
		}
	}
}
