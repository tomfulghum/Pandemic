#ifndef CUSTOM_POSTFX_GAUSSIAN_BLUR
#define CUSTOM_POSTFX_GAUSSIAN_BLUR

#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
float _Radius;

float4 Blur(VaryingsDefault i, float2 direction) : SV_Target
{
	float4 color = float4(0.0, 0.0, 0.0, 0.0);

	float2 off1 = float2(1.5, 1.5) * direction * _Radius;
	float2 off2 = float2(3.5, 3.5) * direction * _Radius;
	float2 off3 = float2(5.5, 5.5) * direction * _Radius;
	color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord) * 0.3498846590518951416015625;
	color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + (off1 / _ScreenParams.xy)) * 0.3133078515529632568359375;
	color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord - (off1 / _ScreenParams.xy)) * 0.3133078515529632568359375;
	color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + (off2 / _ScreenParams.xy)) * 0.011726136319339275360107421875;
	color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord - (off2 / _ScreenParams.xy)) * 0.011726136319339275360107421875;
	color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + (off3 / _ScreenParams.xy)) * 0.00002368900823057629168033599853515625;
	color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord - (off3 / _ScreenParams.xy)) * 0.00002368900823057629168033599853515625;

	return color;
}

float4 BlurHorizontal(VaryingsDefault i) : SV_Target
{
	return Blur(i, float2(1, 0));
}

float4 BlurVertical(VaryingsDefault i) : SV_Target
{
	return Blur(i, float2(0, 1));
}

float4 SimpleBlit(VaryingsDefault i) : SV_Target
{
	return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
}

#endif // CUSTOM_POSTFX_GAUSSIAN_BLUR