using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

//**********************//
//    Settings class    //
//**********************//

[Serializable]
[PostProcess(typeof(GaussianBlurRenderer), PostProcessEvent.AfterStack, "Custom/GaussianBlur")]
public sealed class GaussianBlur : PostProcessEffectSettings
{
    [Range(0, 10)]
    public IntParameter iterations = new IntParameter { value = 1 };
    public FloatParameter radius = new FloatParameter { value = 1.0f };
}

//**********************//
//    Renderer class    //
//**********************//

public sealed class GaussianBlurRenderer : PostProcessEffectRenderer<GaussianBlur>
{
    //**********************//
    //    Internal Types    //
    //**********************//

    private enum Pass
    {
        Horizontal,
        Vertical,
        SimpleBlit
    }

    private struct Level
    {
        internal int horizontal;
        internal int vertical;
    }

    //**********************//
    //    Private Fields    //
    //**********************//

    private Shader m_shader;
    private Level[] m_levels;
    private const int k_maxIterations = 10;

    //**************************//
    //    Renderer Functions    //
    //**************************//

    public override void Init()
    {
        m_shader = Shader.Find(("Hidden/PostProcessing/GaussianBlur"));

        m_levels = new Level[k_maxIterations];
        for (int i = 0; i < k_maxIterations; i++) {
            m_levels[i] = new Level {
                horizontal = i + 1,
                vertical = k_maxIterations + i + 1
            };
        }
    }

    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(m_shader);
        var cmd = context.command;
        int iterations = settings.iterations;

        float radiusStep = settings.radius / iterations;
        var lastSource = context.source;
        for (int i = 0; i < iterations; i++) {
            sheet.properties.SetFloat("_Radius", radiusStep * (i + 1));

            int horizontal = m_levels[i].horizontal;
            int vertical = m_levels[i].vertical;

            context.GetScreenSpaceTemporaryRT(cmd, horizontal, 0, context.sourceFormat, RenderTextureReadWrite.Default, FilterMode.Bilinear);
            context.GetScreenSpaceTemporaryRT(cmd, vertical, 0, context.sourceFormat, RenderTextureReadWrite.Default, FilterMode.Bilinear);
            cmd.BlitFullscreenTriangle(lastSource, horizontal, sheet, (int)Pass.Horizontal);
            cmd.BlitFullscreenTriangle(horizontal, vertical, sheet, (int)Pass.Vertical);

            lastSource = vertical;
        }

        cmd.BlitFullscreenTriangle(lastSource, context.destination, sheet, (int)Pass.SimpleBlit);

        for (int i = 0; i < iterations; i++) {
            cmd.ReleaseTemporaryRT(m_levels[i].horizontal);
            cmd.ReleaseTemporaryRT(m_levels[i].vertical);
        }
    }
}