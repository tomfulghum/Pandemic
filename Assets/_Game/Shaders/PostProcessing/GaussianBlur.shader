Shader "Hidden/PostProcessing/GaussianBlur"
{
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass // 0
		{
			Name "Horizontal blur"

			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex VertDefault
				#pragma fragment BlurHorizontal
				#include "GaussianBlur.hlsl"
			ENDHLSL
		}

		Pass // 1
		{
			Name "Vertical blur"

			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex VertDefault
				#pragma fragment BlurVertical
				#include "GaussianBlur.hlsl"
			ENDHLSL
		}

		Pass // 2
		{
			Name "Simple Blit"

			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex VertDefault
				#pragma fragment SimpleBlit
				#include "GaussianBlur.hlsl"
			ENDHLSL
		}
	}
}