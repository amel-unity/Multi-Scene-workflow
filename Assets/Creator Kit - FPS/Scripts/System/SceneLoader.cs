using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public Transform player;
    public float loadRange;

    private Transform[] levelScenesPos;
    private string[] levelScenes;
    private bool[] sceneLoaded;

    void Start()
    {
        levelScenesPos = ScenesDataBase.instance.levelScenesPositions;
        levelScenes = ScenesDataBase.instance.levelScenesNames;
        sceneLoaded = ScenesDataBase.instance.SceneState;
    }

    void Update()
    {
       
        for (int i = 0; i < levelScenesPos.Length; ++i)
        {
            if (Vector3.Distance(player.position, levelScenesPos[i].position) < loadRange){
                if (!sceneLoaded[i])
                {
                    SceneManager.LoadSceneAsync(levelScenes[i], LoadSceneMode.Additive);
                    sceneLoaded[i] = true;
                }
            }
            else
            {
                if (sceneLoaded[i])
                {
                    SceneManager.UnloadSceneAsync(levelScenes[i]);
                    sceneLoaded[i] = false;
                }
            }
        }    
    }
}
