using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;


public enum CheckMethod
{
    Distance,
    Trigger
}

public class SceneLoader : MonoBehaviour
{
    public Transform player;


    public CheckMethod checkMethod;
    private string[] levelScenes;
    private bool[] sceneLoaded;

    //For Distance Check
    private Transform[] levelScenesPos;
    //For Trigger check
    private GameObject[] levelScenesTrig;

    void Start()
    {
        levelScenesTrig = ScenesDataBase.instance.levelScenesTriggers;
        levelScenesPos = ScenesDataBase.instance.levelScenesPositions;
        levelScenes = ScenesDataBase.instance.levelScenesNames;
        sceneLoaded = ScenesDataBase.instance.SceneState;

        //verify if any scene is already open to avoid opening a scene twice
        if (SceneManager.sceneCount > 0)
        {
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (scene.isLoaded)
                {
                    for (int j = 0; j < levelScenes.Length; ++j)
                    {
                        if(scene.name == levelScenes[j])
                        {
                            sceneLoaded[j] = true;
                        }
                    }
                }
            }
        }
    }

    void Update()
    {
        if(checkMethod == CheckMethod.Trigger)
        {
            TriggerCheck();
        }
        else if(checkMethod == CheckMethod.Distance)
        {
            DistanceCheck(); 
        }
    }


    void TriggerCheck()
    {
        for (int i = 0; i < levelScenesTrig.Length; ++i)
        {

            if (levelScenesTrig[i].GetComponent<TriggerDetection>().GetState())
            {

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

    void DistanceCheck()
    {
        for (int i = 0; i < levelScenesPos.Length; ++i)
        {
            float loadRange = levelScenesPos[i].GetComponent<DistanceDetection>().GetLoadRange();

            if (Vector3.Distance(player.position, levelScenesPos[i].position) < loadRange)
            {

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
