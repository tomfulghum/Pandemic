Shader "Hidden/PostProcessing/Grayscale"
{
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex VertDefault
				#pragma fragment Frag
				#include "Grayscale.hlsl"
			ENDHLSL
		}
	}
}