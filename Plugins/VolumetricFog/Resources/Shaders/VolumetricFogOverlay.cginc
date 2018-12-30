    #define OVERLAY_FOG
    #define SHADER_API_GLES 1
    #include "VolumetricFog.cginc"

    // Final fog function for Standard Shaders
    void overlayFog(Input IN, SurfaceOutputStandardSpecular o, inout fixed4 color) {

        #ifndef UNITY_PASS_FORWARDADD

            float3 worldPos = IN.worldPos.xyz;

            #if FOG_USE_XY_PLANE
                wsCameraPos = float3(_WorldSpaceCameraPos.x, _WorldSpaceCameraPos.y, _WorldSpaceCameraPos.z - _FogData.x);
                worldPos.z -= _FogData.x;
            #else
                wsCameraPos = float3(_WorldSpaceCameraPos.x, _WorldSpaceCameraPos.y - _FogData.x, _WorldSpaceCameraPos.z);
                worldPos.y -= _FogData.x;
            #endif

            float3 uv = IN.screenPos.xyz / IN.screenPos.w;
            float depth01 = Linear01Depth(uv.z);

            SetDither(uv.xy);

            half4 sum = getFogColor(worldPos, depth01);

            sum *= 1.0 + dither * _FogStepping.w;

            #if defined(FOG_DEBUG)
                color = sum;
                return;
            #endif
    
            #if FOG_BLUR_ON
                half4 blurColor = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_BlurTex, uv.xy);
                color.rgb = lerp(color.rgb, blurColor.rgb, sum.a);
            #endif

            color.rgb = color.rgb * saturate(1.0 - sum.a) + sum.rgb;

            #if _ALPHAPREMULTIPLY_ON
                color.rgb *= o.Alpha;
            #endif

        #endif
    }

  