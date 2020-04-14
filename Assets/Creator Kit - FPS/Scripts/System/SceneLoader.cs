using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public Transform player;
    public float loadRange;

    private Transform[] levelScenesPos;
    private string[] levelScenes;

    void Start()
    {
        levelScenesPos = ScenesDataBase.instance.levelScenesPositions;
        levelScenes = ScenesDataBase.instance.levelScenesNames;
    }

    void Update()
    {
       
        for (int i = 0; i < levelScenesPos.Length; ++i)
        {
            if (Vector3.Distance(player.position, levelScenesPos[i].position) < loadRange){
                SceneManager.LoadSceneAsync(levelScenes[i], LoadSceneMode.Additive);
            }
            else
            {
                SceneManager.UnloadSceneAsync(levelScenes[i]);
            }
        }    
    }
}
