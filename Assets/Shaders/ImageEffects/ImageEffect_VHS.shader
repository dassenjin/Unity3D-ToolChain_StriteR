﻿Shader "Hidden/ImageEffect_VHS"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma multi_compile _ _SCREENCUT_HARD _SCREENCUT_SCALED
            #pragma shader_feature _COLORBLEED
            #pragma shader_feature _COLORBLEED_R
            #pragma shader_feature _COLORBLEED_G
            #pragma shader_feature _COLORBLEED_B
            #pragma shader_feature _GRAIN
            #pragma shader_feature _LINEDISTORT
            #pragma shader_feature _PIXELDISTORT
            #include "CameraEffectInclude.cginc"
            #include "UnityCG.cginc"
            float2 _ScreenCutTarget;

            #if _COLORBLEED
            float _ColorBleedIteration;
            float _ColorBleedSize;
            float2 _ColorBleedR;
            float2 _ColorBleedG;
            float2 _ColorBleedB;
            #endif

            #if _LINEDISTORT
            float _LineDistortSpeed;
            float _LineDistortStrength;
            float _LineDistortClip;
            float _LineDistortFrequency;
            #endif

            #if _PIXELDISTORT
            float2 _PixelDistortScale;
            float _PixelDistortFrequency;
            float _PixelDistortClip;
            float _PixelDistortStrength;
            #endif

            #if _GRAIN
            float2 _GrainScale;
            float4 _GrainColor;
            float _GrainClip;
            float _GrainFrequency;
            #endif

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };


            float2 screenCut(float2 uv) {
                return uv;
            }
            fixed4 frag (v2f_img i) : SV_Target
            {
                float2 uv=screenCut(i.uv);
                
                uv -= 0.5;
                #if _SCREENCUT_HARD
                uv.x=sign(uv.x)*clamp(abs(uv.x),0,_ScreenCutTarget.x);
                uv.y=sign(uv.y)*clamp(abs(uv.y),0,_ScreenCutTarget.y);
                #elif _SCREENCUT_SCALED
                uv*=2;
                uv.x=sign(uv.x)*lerp(0,_ScreenCutTarget.x,abs(uv.x));
                uv.y=sign(uv.y)*lerp(0,_ScreenCutTarget.y,abs(uv.y));
                #endif
                
                #if _LINEDISTORT
                float lineDistort= lerp(-1,1,frac(uv.y*_LineDistortFrequency+_Time.y*_LineDistortSpeed));
                lineDistort=abs(lineDistort);
                lineDistort=smoothstep(_LineDistortClip,1,lineDistort);
                uv.x+=lineDistort*_LineDistortStrength;
                #endif
                uv += .5;

                #if _PIXELDISTORT
                float2 pixelDistort=floor(uv*_PixelDistortScale*_MainTex_TexelSize.zw)*(_PixelDistortScale*_MainTex_TexelSize.xy)+random(floor(_Time.y*_PixelDistortFrequency)/_PixelDistortFrequency);
                float pixelDistortRandom=random(pixelDistort);
                uv += step(_PixelDistortClip,pixelDistortRandom)*lerp(-1,1,pixelDistort)*_PixelDistortStrength;
                #endif

                float4 col = tex2D(_MainTex, uv);
                #if _COLORBLEED
                float colorBleedOffset=0;
                float colR,colG,colB;
                for(int k=0;k<_ColorBleedIteration;k++)
                {
                    colorBleedOffset+=_ColorBleedSize;
                    #if _COLORBLEED_R
                    colR+=tex2D(_MainTex,uv+colorBleedOffset*_MainTex_TexelSize.xy*_ColorBleedR).r;
                    #endif
                    #if _COLORBLEED_G
                    colG+=tex2D(_MainTex,uv+colorBleedOffset*_MainTex_TexelSize.xy*_ColorBleedG).g;
                    #endif
                    #if _COLORBLEED_B
                    colB+=tex2D(_MainTex,uv+colorBleedOffset*_MainTex_TexelSize.xy*_ColorBleedB).b;
                    #endif
                }
                #if _COLORBLEED_R
                col.r=colR/_ColorBleedIteration;
                #endif
                #if _COLORBLEED_G
                col.g=colG/_ColorBleedIteration;
                #endif
                #if _COLORBLEED_B
                col.b=colB/_ColorBleedIteration;
                #endif
                #endif
                
                #if _GRAIN
                float rand= random(floor(uv*_GrainScale*_MainTex_TexelSize.zw)*(_MainTex_TexelSize.xy*_GrainScale)+random(floor(_Time.y*_GrainFrequency)/_GrainFrequency));
                col.rgb=lerp(col.rgb,_GrainColor.rgb,step(_GrainClip,rand)*rand*_GrainColor.a);
                #endif

                return col;
            }
            ENDCG
        }
    }
}
