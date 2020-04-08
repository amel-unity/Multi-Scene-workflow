using UnityEngine.SceneManagement;
using UnityEngine;

public class ScenesDataBase : MonoBehaviour
{
    public static ScenesDataBase instance;
    public string[] scenesNames;
    void Awake()
    {
        instance = this;
    }

}
