#ifndef CUSTOM_POSTFX_GAUSSIAN_BLUR
#define CUSTOM_POSTFX_GAUSSIAN_BLUR

#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
float _Radius;

float4 Blur(VaryingsDefault i, float2 direction) : SV_Target
{
	float4 color = float4(0.0, 0.0, 0.0, 0.0);

	float2 off1 = float2(1.411764705882353, 1.411764705882353) * direction * _Radius;
	float2 off2 = float2(3.294117647058823, 3.294117647058823) * direction * _Radius;
	float2 off3 = float2(5.176470588235294, 5.176470588235294) * direction * _Radius;
	color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord) * 0.1964825501511404;
	color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + (off1 / _ScreenParams.xy)) * 0.2969069646728344;
	color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord - (off1 / _ScreenParams.xy)) * 0.2969069646728344;
	color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + (off2 / _ScreenParams.xy)) * 0.09447039785044732;
	color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord - (off2 / _ScreenParams.xy)) * 0.09447039785044732;
	color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + (off3 / _ScreenParams.xy)) * 0.010381362401148057;
	color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord - (off3 / _ScreenParams.xy)) * 0.010381362401148057;

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