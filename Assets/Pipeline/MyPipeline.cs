using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

public class MyPipeline : RenderPipeline
{

    CommandBuffer cb = new CommandBuffer { name = "Render Camera" };
    CullResults cull;
    
    public override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        base.Render(context, cameras);
        BeginFrameRendering(cameras);

        foreach (var camera in cameras)
        {
            context.SetupCameraProperties(camera);
            BeginCameraRendering(camera);

            // Cull
            if (!CullResults.GetCullingParameters(camera, out var cullingParameters))
                return;
            CullResults.Cull(ref cullingParameters, context, ref cull);

            // Clear

            var clearFlags = camera.clearFlags;
            cb.ClearRenderTarget(clearFlags.HasFlag(CameraClearFlags.Depth), clearFlags.HasFlag(CameraClearFlags.Color), camera.backgroundColor);

            cb.BeginSample("Render Camera");
            context.ExecuteCommandBuffer(cb);
            cb.Clear();

            // Draw
            
            var drawSettings = new DrawRendererSettings(camera, new ShaderPassName("SRPDefaultUnlit"));

            var filterSettings = new FilterRenderersSettings(true);

            // opaque pass
            drawSettings.sorting.flags = SortFlags.CommonOpaque; // sort by distance to avoid overdraw
            filterSettings.renderQueueRange = RenderQueueRange.opaque;
            context.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSettings);

            context.DrawSkybox(camera);

            // post-skybox pass for transparent renderers
 
            drawSettings.sorting.flags = SortFlags.CommonTransparent;
            filterSettings.renderQueueRange = RenderQueueRange.transparent;
            context.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSettings);

            DrawDefaultPipeline(context, camera);

            cb.EndSample("Render Camera");
            context.ExecuteCommandBuffer(cb);
            cb.Clear();
            context.Submit();
        }
    }

    Material errorMaterial;

    [Conditional("DEBUG_EDITOR"), Conditional("UNITY_EDITOR")]
    void DrawDefaultPipeline(ScriptableRenderContext context, Camera camera)
    {
        if (errorMaterial == null)
        {
            Shader errorShader = Shader.Find("Hidden/InternalErrorShader");
            errorMaterial = new Material(errorShader) { hideFlags = HideFlags.HideAndDontSave };
        }
        var drawSettings = new DrawRendererSettings(camera, new ShaderPassName("ForwardBase"));
        drawSettings.SetShaderPassName(1, new ShaderPassName("PrepassBase"));
        drawSettings.SetShaderPassName(2, new ShaderPassName("Always"));
        drawSettings.SetShaderPassName(3, new ShaderPassName("Vertex"));
        drawSettings.SetShaderPassName(4, new ShaderPassName("VertexLMRGBM"));
        drawSettings.SetShaderPassName(5, new ShaderPassName("VertexLM"));
        drawSettings.SetOverrideMaterial(errorMaterial, 0);

        var filterSettings = new FilterRenderersSettings(true);
        
        context.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSettings);
    }
}
