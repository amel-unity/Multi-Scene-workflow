using UnityEngine.SceneManagement;
using UnityEngine;

public class ScenesDataBase : MonoBehaviour
{
    [Header("Level scenes names and positions")]
    public string[] MenuScenesNames;
    [Header("Level scenes names and positions")]
    public string[] levelScenesNames;
    public Transform[] levelScenesPositions;

    public static ScenesDataBase instance;

    [HideInInspector]
    public bool[] SceneState;

    void Awake()
    {
        instance = this;
        SceneState = new bool[levelScenesNames.Length];
    }

}
