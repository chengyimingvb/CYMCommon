#include "UnityCG.cginc"

struct SpriteData 
{
	float4 vertex		: POSITION;
	float3 normal		: NORMAL;
	float4 texcoord		: TEXCOORD0;
	half4 color			: COLOR;
};
	
uniform sampler2D _MainTex;
uniform fixed _Cutoff;
uniform fixed4 _Color;

