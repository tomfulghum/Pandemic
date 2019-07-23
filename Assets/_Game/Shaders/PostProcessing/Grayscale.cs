using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(GrayscaleRenderer), PostProcessEvent.AfterStack, "Custom/Grayscale")]
public sealed class Grayscale : PostProcessEffectSettings
{
    [Range(0f, 1f)]
    public FloatParameter blend = new FloatParameter { value = 0.5f };
    [Range(0f, 1f)]
    public FloatParameter luminanceR = new FloatParameter { value = 0.2126729f };
    [Range(0f, 1f)]
    public FloatParameter luminanceG = new FloatParameter { value = 0.7151522f };
    [Range(0f, 1f)]
    public FloatParameter luminanceB = new FloatParameter { value = 0.0721750f };
}

public sealed class GrayscaleRenderer : PostProcessEffectRenderer<Grayscale>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find(("Hidden/PostProcessing/Grayscale")));
        sheet.properties.SetFloat("_Blend", settings.blend);
        sheet.properties.SetVector("_Luminance", new Vector3(settings.luminanceR, settings.luminanceG, settings.luminanceB));
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}