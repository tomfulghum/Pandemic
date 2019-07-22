#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
float _Blend;
float3 _Luminance;

float4 Frag(VaryingsDefault i) : SV_Target
{
	float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
	float luminance = dot(color.rgb, _Luminance);
	color.rgb = lerp(color.rgb, luminance.xxx, _Blend.xxx);
	return color;
}