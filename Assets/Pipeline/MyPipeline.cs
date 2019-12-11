using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

public class MyPipeline : RenderPipeline
{
    CullResults cull;

    CommandBuffer cameraBuffer = new CommandBuffer
    {
        name = "Render Camera"
    };

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
        if (!CullResults.GetCullingParameters(camera, out ScriptableCullingParameters cullingParameters))
            return;
        CullResults.Cull(ref cullingParameters, context, ref cull);

        context.SetupCameraProperties(camera);
        
        var clearFlags = camera.clearFlags;
        cameraBuffer.ClearRenderTarget(
            clearFlags.HasFlag(CameraClearFlags.Depth),
            clearFlags.HasFlag(CameraClearFlags.Color),
            camera.backgroundColor
        );
        context.ExecuteCommandBuffer(cameraBuffer);
        cameraBuffer.Clear();

        cameraBuffer.BeginSample("Render Sample");

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
