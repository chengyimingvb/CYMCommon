// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

sampler2D _MainTex;
sampler2D _MaskTex;
fixed _MaskTexInverted;
sampler2D _Gloss;
half4 _GlossColor;
fixed _GlossRotation;
fixed _GlossWidth;
fixed _GlossProgressFrom;
fixed _GlossProgressTo;
fixed _GlossTime;
fixed _GlossDelay;
fixed _MainTexTransparent;

struct appdata_t {
    
    float4 vertex   : POSITION;
    float4 color    : COLOR;
    float2 texcoord : TEXCOORD0;
    float3 normal   : NORMAL;

};

struct v2f {
    
    float4 vertex   : SV_POSITION;
    fixed4 color    : COLOR;
    half2 texcoord  : TEXCOORD0;
    half2 texcoord2 : TEXCOORD1;
    half3 lightDir  : TEXCOORD2;
    half3 normal	: NORMAL;

};

half computeOffsetX() {

	fixed from = _GlossProgressFrom;
	fixed to = _GlossProgressTo;
    return lerp(from, to + (to - from) * _GlossDelay, frac(_Time.y / (_GlossTime + _GlossDelay)));

}

v2f vert(appdata_t i) {
    
    v2f o;
    o.normal = normalize(mul(unity_ObjectToWorld, float4(i.normal, 0)));
    o.lightDir = normalize(WorldSpaceLightDir(i.vertex));
    o.vertex = UnityObjectToClipPos(i.vertex);

    o.texcoord = i.texcoord;

#if (SCREENPOS == 1)
    half2 screenPos = ComputeScreenPos(o.vertex).xy;
	half2 tx = screenPos * 0.135;
#else
	half2 tx = i.texcoord;
#endif

	half r = (_GlossRotation / 360) * 6.25;

    tx.xy -= 0.5;
    float s = sin(r);
    float c = cos(r);
    float2x2 rotationMatrix = float2x2(c, -s, s, c);
    rotationMatrix *= 0.5;
    rotationMatrix += 0.5;
    rotationMatrix = rotationMatrix * 2 - 1;
    tx.xy = mul(tx.xy, rotationMatrix);
    tx.xy += 0.5;

    //o.texcoord2 = half2(tx.x * _GlossWidth - computeOffsetX(), tx.y);
    o.texcoord2 = half2(tx.x + 1 - _GlossWidth - computeOffsetX(), tx.y);
    o.texcoord2.x *= _GlossWidth;

    o.color = i.color;

    return o;

}

fixed4 fragDo(v2f i, half4 mainColor) : SV_Target {

#if (TEXT == 1)
    fixed4 c = tex2D(_MainTex, i.texcoord);
    c.rgb = mainColor.rgb;
#else
    fixed4 c = tex2D(_MainTex, i.texcoord) * mainColor;
#endif
    fixed4 gc = tex2D(_Gloss, i.texcoord2);
#if (MASK == 1)
	fixed mask = abs(tex2D(_MaskTex, i.texcoord).a - _MaskTexInverted);
	gc *= mask;
#endif
    gc *= _GlossColor;
    gc.rgb *= gc.a;

    return (c * clamp(1 - _MainTexTransparent, 0, 1) + gc) * c.a;

}