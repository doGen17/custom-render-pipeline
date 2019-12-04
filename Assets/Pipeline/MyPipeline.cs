using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

public class MyPipeline : RenderPipeline
{
    public override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
    {
        base.Render(renderContext, cameras);

        foreach (var camera in cameras)
        {
            Render(renderContext, camera);
        }
    }

    void Render(ScriptableRenderContext context, Camera camera)
    {
        ScriptableCullingParameters cullingParameters;
        if (!CullResults.GetCullingParameters(camera, out cullingParameters))
            return;
        CullResults cull = CullResults.Cull(ref cullingParameters, context);

        context.SetupCameraProperties(camera);

        var buffer = new CommandBuffer { name = camera.name };
        CameraClearFlags clearFlags = camera.clearFlags;
        buffer.ClearRenderTarget(
            (clearFlags & CameraClearFlags.Depth) != 0,
            (clearFlags & CameraClearFlags.Color) != 0,
            camera.backgroundColor
        );
        context.ExecuteCommandBuffer(buffer);
        buffer.Release();

        // draw settings for the unlit pass
        var drawSettings = new DrawRendererSettings(camera, new ShaderPassName("SRPDefaultUnlit"));
        drawSettings.sorting.flags = SortFlags.CommonOpaque; // sort by distance to avoid overdraw

        var filterSettings = new FilterRenderersSettings(true);

        // opaque pass
        filterSettings.renderQueueRange = RenderQueueRange.opaque;
        context.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSettings);

        context.DrawSkybox(camera);

        // post-skybox pass for transparent renderers
        filterSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSettings);

        context.Submit();
    }
}
