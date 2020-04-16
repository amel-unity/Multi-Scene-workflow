using UnityEngine.SceneManagement;
using UnityEngine;

public class ScenesDataBase : MonoBehaviour
{
    public static ScenesDataBase instance;

    [Header("Level scenes names and positions")]
    public string[] levelScenesNames;
    public Transform[] levelScenesPositions;
    public bool[] SceneState;

    void Awake()
    {
        instance = this;
        SceneState = new bool[levelScenesNames.Length];
    }

}
