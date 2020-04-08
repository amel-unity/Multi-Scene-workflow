using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Rendering;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// Minimap System that render the level into a given render texture with the given settings. It rely on the level part
/// that need to be on the minimap (e.g. most of the time the wall) to have a MinimapElement script attached to it.
/// It is used by the Minimap UI script to render the minimap.
/// </summary>
public class MinimapSystem
{
    [System.Serializable]
    public struct MinimapSystemSetting
    {
        public float heightStep;
        public float halfSize;
        public float wallThickness;
        public Material material;
        public bool isFixed;
    }

    static int s_WallThicknessId;

    static MinimapSystem()
    {
        s_WallThicknessId = Shader.PropertyToID("_WallThickness");
    }
    

    public static void Render(RenderTexture renderTarget, Vector3 origin, Vector3 forward, MinimapSystemSetting settings)
    {
        settings.material.SetFloat(s_WallThicknessId, settings.wallThickness);
        
        float aspectRatio = renderTarget.width / (float)renderTarget.height;

        CommandBuffer buffer = new CommandBuffer();

        Matrix4x4 lookAt;

        Vector3 camPos = origin + Vector3.up * 3.0f;

        if (settings.isFixed)
        {
            lookAt = Matrix4x4.TRS(camPos, Quaternion.LookRotation(Vector3.down, Vector3.forward), new Vector3(1, 1, -1)).inverse;
        }
        else
        {
            lookAt = Matrix4x4.TRS(camPos, Quaternion.LookRotation(Vector3.down, forward), new Vector3(1, 1, -1)).inverse;
        }

        buffer.SetRenderTarget(renderTarget);
        buffer.SetProjectionMatrix(Matrix4x4.Ortho(-settings.halfSize * aspectRatio, settings.halfSize  * aspectRatio, -settings.halfSize , settings.halfSize , 0.5f, 1.5f));
        buffer.SetViewMatrix(lookAt);
        
        buffer.ClearRenderTarget(true,true, Color.black);
        foreach (var r in MinimapElement.Renderers)
        {
            buffer.DrawRenderer(r, settings.material);
        }
        
        Graphics.ExecuteCommandBuffer(buffer);
    }
}
