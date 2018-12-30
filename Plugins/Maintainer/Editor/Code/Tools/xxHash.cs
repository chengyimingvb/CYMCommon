#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Tools
{
	internal class xxHash
	{
		private const uint Prime32_1 = 2654435761U;
		private const uint Prime32_2 = 2246822519U;
		private const uint Prime32_3 = 3266489917U;
		private const uint Prime32_4 = 668265263U;
		private const uint Prime32_5 = 374761393U;

		public static uint CalculateHash(byte[] buf, int len, uint seed)
		{
			uint h32;
			var index = 0;

			if (len >= 16)
			{
				var limit = len - 16;
				var v1 = seed + Prime32_1 + Prime32_2;
				var v2 = seed + Prime32_2;
				var v3 = seed;
				var v4 = seed - Prime32_1;

				do
				{
					var readValue = (uint)(buf[index++] | buf[index++] << 8 | buf[index++] << 16 | buf[index++] << 24);
					v1 += readValue * Prime32_2;
					v1 = (v1 << 13) | (v1 >> 19);
					v1 *= Prime32_1;

					readValue = (uint)(buf[index++] | buf[index++] << 8 | buf[index++] << 16 | buf[index++] << 24);
					v2 += readValue * Prime32_2;
					v2 = (v2 << 13) | (v2 >> 19);
					v2 *= Prime32_1;

					readValue = (uint)(buf[index++] | buf[index++] << 8 | buf[index++] << 16 | buf[index++] << 24);
					v3 += readValue * Prime32_2;
					v3 = (v3 << 13) | (v3 >> 19);
					v3 *= Prime32_1;

					readValue = (uint)(buf[index++] | buf[index++] << 8 | buf[index++] << 16 | buf[index++] << 24);
					v4 += readValue * Prime32_2;
					v4 = (v4 << 13) | (v4 >> 19);
					v4 *= Prime32_1;

				} while (index <= limit);

				h32 = ((v1 << 1) | (v1 >> 31)) + ((v2 << 7) | (v2 >> 25)) + ((v3 << 12) | (v3 >> 20)) + ((v4 << 18) | (v4 >> 14));
			}
			else
			{
				h32 = seed + Prime32_5;
			}

			h32 += (uint)len;

			while (index <= len - 4)
			{
				h32 += (uint)(buf[index++] | buf[index++] << 8 | buf[index++] << 16 | buf[index++] << 24) * Prime32_3;
				h32 = ((h32 << 17) | (h32 >> 15)) * Prime32_4;
			}

			while (index < len)
			{
				h32 += buf[index] * Prime32_5;
				h32 = ((h32 << 11) | (h32 >> 21)) * Prime32_1;
				index++;
			}

			h32 ^= h32 >> 15;
			h32 *= Prime32_2;
			h32 ^= h32 >> 13;
			h32 *= Prime32_3;
			h32 ^= h32 >> 16;

			return h32;
		}
	}
}