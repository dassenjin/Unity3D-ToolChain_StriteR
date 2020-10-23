﻿Shader "Hidden/CameraEffect_DepthVerticalFog"
{
	Properties
	{
		[PreRenderData]_MainTex ("Texture", 2D) = "white" {}
		_FogDensity("Fog Density",Float) = 1
		_FogColor("Fog Color",Color) = (1,1,1,1)
		_FogVerticalStart("Fog Start",Float) = 0
		_FogVerticalOffset("Fog Offset",Float) = 1
		[NoScaleOffset]_NoiseTex("Noise Tex",2D) = "white"{}
		_NoiseScale("Noise Scale",float)=.5
		_NoiseSpeedX("Fog Speed Horizontal",Range(-.5,.5)) = .5
		_NoiseSpeedY("Fog Speed Vertical",Range(-.5,.5)) = .5
	}
		SubShader
		{
			Cull Off ZWrite Off ZTest Always

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				#include "CameraEffectInclude.cginc"
				half _FogDensity;
				float _FogPow;
				fixed4 _FogColor;
				float _FogVerticalStart;
				float _FogVerticalOffset;
				sampler2D _NoiseTex;
				float _NoiseScale;
				float _NoiseSpeedX;
				float _NoiseSpeedY;

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
					float2 uv_depth:TEXCOORD1;
					float4 interpolatedRay:TEXCOORD2;
				};

				v2f vert (appdata_img v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.texcoord;
					o.uv_depth = GetDepthUV(v.texcoord); 
					o.interpolatedRay = GetInterpolatedRay(o.uv);
					return o;
				}

				fixed4 frag (v2f i) : SV_Target
				{
					float linearDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv_depth));
					float3 worldPos = _WorldSpaceCameraPos+ i.interpolatedRay.xyz*linearDepth;
					float2 worldUV = (worldPos.xz + worldPos.y);
					float2 noiseUV = worldUV / _NoiseScale + _Time.y*float2(_NoiseSpeedX,_NoiseSpeedY);
					float noise = tex2D(_NoiseTex, noiseUV).r;
					float fog =  (( _FogVerticalStart+_FogVerticalOffset)-worldPos.y)  /_FogVerticalOffset*_FogDensity*noise;
					return lerp(tex2D(_MainTex, i.uv) , _FogColor, saturate(fog));
				}
				ENDCG
		}
	}
}