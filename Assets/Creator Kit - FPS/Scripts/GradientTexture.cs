using UnityEngine;

[CreateAssetMenu]
public class GradientTexture : ScriptableObject
{
    public Gradient gradient;
    public int width = 64;

    public Texture2D texture;
}
