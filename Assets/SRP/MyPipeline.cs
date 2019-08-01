using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class MyPipeline : RenderPipeline
{
    CullResults cull;
    CommandBuffer cameraBuffer = new CommandBuffer()
    {
        name = "Render Camera"
    };
    Material errorMaterial;
    public override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
    {
        base.Render(renderContext, cameras);
   
        foreach (var camera in cameras)
        {
            renderContext.SetupCameraProperties(camera);

            CameraClearFlags clearFlags = camera.clearFlags;
            cameraBuffer.ClearRenderTarget((clearFlags & CameraClearFlags.Depth)!=0, (clearFlags & CameraClearFlags.Color) != 0, camera.backgroundColor);
            renderContext.ExecuteCommandBuffer(cameraBuffer);
            cameraBuffer.Clear();

            if (!CullResults.GetCullingParameters(camera, out ScriptableCullingParameters cullingParameters))
            {
                return;
            }

#if UNITY_EDITOR
            if (camera.cameraType == CameraType.SceneView)
            {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
            }
#endif

            CullResults.Cull(ref cullingParameters, renderContext,ref cull);

            var drawSettings = new DrawRendererSettings(camera, new ShaderPassName("SRPDefaultUnlit"));
            //先渲染不透明
            //This instructs Unity to sort the renderers by distance, from front to back, plus a few other criteria.
            drawSettings.sorting.flags = SortFlags.CommonOpaque;
            //筛选不透明物体
            var filterSettings = new FilterRenderersSettings(true)
            {
                renderQueueRange = RenderQueueRange.opaque
            };

            renderContext.DrawRenderers(
                cull.visibleRenderers, ref drawSettings, filterSettings
            );
            //渲染天空
            renderContext.DrawSkybox(camera);

            //筛选透明物体
            drawSettings.sorting.flags = SortFlags.CommonTransparent;
            filterSettings.renderQueueRange = RenderQueueRange.transparent;
            //渲染透明
            renderContext.DrawRenderers(
                cull.visibleRenderers, ref drawSettings, filterSettings
            );

#if UNITY_EDITOR
            DrawDefaultPipeline(renderContext, camera);
#endif

        }
        renderContext.Submit();
    }

    //To only include the invocation when compiling for the Unity editor   invocation in development builds
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD"),System.Diagnostics.Conditional("UNITY_EDITOR")]
    void DrawDefaultPipeline(ScriptableRenderContext context, Camera camera)
    {
        if (errorMaterial == null)
        {
            Shader errorShader = Shader.Find("Hidden/InternalErrorShader");
            errorMaterial = new Material(errorShader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }
        var drawSettings = new DrawRendererSettings(
            camera, new ShaderPassName("ForwardBase")
        );

        drawSettings.SetShaderPassName(1, new ShaderPassName("PrepassBase"));
        drawSettings.SetShaderPassName(2, new ShaderPassName("Always"));
        drawSettings.SetShaderPassName(3, new ShaderPassName("Vertex"));
        drawSettings.SetShaderPassName(4, new ShaderPassName("VertexLMRGBM"));
        drawSettings.SetShaderPassName(5, new ShaderPassName("VertexLM"));


        //替换材质
        drawSettings.SetOverrideMaterial(errorMaterial, 0);

        var filterSettings = new FilterRenderersSettings(true);

        context.DrawRenderers(
            cull.visibleRenderers, ref drawSettings, filterSettings
        );
    }
}
