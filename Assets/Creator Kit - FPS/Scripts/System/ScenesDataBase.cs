using UnityEngine.SceneManagement;
using UnityEngine;

public class ScenesDataBase : MonoBehaviour
{
    [Header("Menu scenes names")]
    public string[] MenuScenesNames;
    [Header("Level scenes names and positions/Triggers")]
    public string[] levelScenesNames;
    //For Trigger Check
    public GameObject[] levelScenesTriggers;
    //For Distance Check
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
