using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GradientTexture))]
public class GradientTextureEditor : Editor
{
    public override void OnInspectorGUI()
    {
        using (var cc = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
            if (cc.changed)
                BuildTexture();
        }

    }
    void BuildTexture()
    {
        var a = target as GradientTexture;
        if (a.texture == null || a.texture.width != a.width)
        {
            if (a.texture != null) DestroyImmediate(a.texture, true);
            a.texture = new Texture2D(a.width, 4, TextureFormat.ARGB32, true);
            a.texture.name = a.name;
            AssetDatabase.AddObjectToAsset(a.texture, a);
        }
        var pixels = a.texture.GetPixels();
        for (var x = 0; x < a.texture.width; x++)
        {
            var color = a.gradient.Evaluate(1f * x / a.texture.width);
            for (var y = 0; y < 4; y++)
            {
                pixels[y * a.texture.width + x] = color;
            }
        }
        a.texture.SetPixels(pixels);
        a.texture.Apply();
        a.texture.wrapMode = TextureWrapMode.Clamp;
        EditorUtility.SetDirty(target);
        AssetDatabase.SaveAssets();
    }
}
