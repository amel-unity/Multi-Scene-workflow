using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
public class GameScene : ScriptableObject
{
    [Header("Information")]
    public string sceneName;
    //public int index;
    public string shortDescription;

    [Header("Sounds")]
    public AudioClip music;
    [Range(0.0f, 1.0f)]
    public float musicVolume;

    [Header("visuals")]
    public PostProcessProfile postprocss;
}
