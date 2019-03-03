Shader "NatureManufacture Shaders/Debug/Flowmap Direction"
{
	Properties
	{
		_Direction("Direction", 2D) = "white" {}
		_RotateUV("Rotate UV", Range( 0 , 1)) = 0
		_TextureSize("Texture Size", Vector) = (246,246,0,0)
		_TileSize("TileSize", Vector) = (20,30,0,0)
		[HideInInspector] _texcoord4( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv4_texcoord4;
			float2 uv_texcoord;
		};

		uniform float _RotateUV;
		uniform sampler2D _Direction;
		uniform float2 _TileSize;
		uniform float2 _TextureSize;
		uniform sampler2D sampler048;
		uniform float4 _Direction_TexelSize;


		inline float2 MyCustomExpression63( float2 A , float2 B )
		{
			return frac(A/B)*B;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 break84 = i.uv4_texcoord4;
			float ifLocalVar100 = 0;
			if( _RotateUV <= 0.0 )
				ifLocalVar100 = break84.y;
			else
				ifLocalVar100 = break84.x;
			float3 _Vector1 = float3(0,0,0);
			float3 _Vector0 = float3(1,1,1);
			float clampResult78 = clamp( ( distance( i.uv4_texcoord4 , float2( 0,0 ) ) * 1.6 ) , 0.0 , 1.0 );
			float3 appendResult189 = (float3(_Vector0.x , ( _Vector0.y - clampResult78 ) , 0.0));
			float clampResult197 = clamp( pow( clampResult78 , 0.15 ) , 0.0 , 1.0 );
			float3 lerpResult193 = lerp( _Vector1 , appendResult189 , clampResult197);
			float3 appendResult191 = (float3(0.0 , ( _Vector0.y - clampResult78 ) , _Vector0.z));
			float3 lerpResult194 = lerp( _Vector1 , appendResult191 , clampResult197);
			float3 ifLocalVar39 = 0;
			if( ifLocalVar100 > 0.0 )
				ifLocalVar39 = lerpResult193;
			else if( ifLocalVar100 == 0.0 )
				ifLocalVar39 = appendResult189;
			else if( ifLocalVar100 < 0.0 )
				ifLocalVar39 = lerpResult194;
			float2 uv_TexCoord60 = i.uv_texcoord * float2( 2,2 );
			float2 A63 = uv_TexCoord60;
			float2 invTileSize61 = ( float2( 1,1 ) / _TileSize );
			float2 B63 = invTileSize61;
			float2 localMyCustomExpression63 = MyCustomExpression63( A63 , B63 );
			float2 appendResult50 = (float2(_Direction_TexelSize.x , _Direction_TexelSize.y));
			float2 ScaledMax56 = ( _TextureSize * appendResult50 );
			float2 ScaledMin57 = ( appendResult50 * float2( 10,10 ) );
			float2 Size62 = ( ScaledMax56 - ScaledMin57 );
			float2 TiledVar65 = ( localMyCustomExpression63 * Size62 );
			float2 finalUV69 = ( 0 + ( TiledVar65 * _TileSize ) );
			float temp_output_174_0 = ( i.uv4_texcoord4.y * -1.0 );
			float temp_output_168_0 = atan( ( ( i.uv4_texcoord4.x * 1.0 ) / temp_output_174_0 ) );
			float ifLocalVar166 = 0;
			if( temp_output_174_0 >= 0.0 )
				ifLocalVar166 = temp_output_168_0;
			else
				ifLocalVar166 = ( temp_output_168_0 + 3.14 );
			float cos3 = cos( ifLocalVar166 );
			float sin3 = sin( ifLocalVar166 );
			float2 rotator3 = mul( finalUV69 - float2( 0.5,0.5 ) , float2x2( cos3 , -sin3 , sin3 , cos3 )) + float2( 0.5,0.5 );
			float3 clampResult36 = clamp( ( ifLocalVar39 * tex2Dlod( _Direction, float4( rotator3, 0, 0.0) ).a ) , float3( 0,0,0 ) , float3( 1,1,1 ) );
			o.Albedo = clampResult36;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}