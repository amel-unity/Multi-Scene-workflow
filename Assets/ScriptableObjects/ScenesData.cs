using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "sceneDB", menuName = "Scene Data/Database")]
public class ScenesData : ScriptableObject
{
    public List<Level> levels = new List<Level>();
    public List<Menu> menus = new List<Menu>();

    public int CurrentLevelIndex=1;

    //To save the currentLevelIndex when we leave the game, but are we going this far for the blog post?
    public bool SaveDuringGame;


    /*
     * Levels
     */

    //Load a scene with a given index
    public void LoadLevelWithIndex(int index)
    {
        //string currentLevelName = levels[index].sceneName;
        //SceneManager.LoadSceneAsync(currentLevelName);

        //Load Gameplay scene for the level
        SceneManager.LoadSceneAsync("Gameplay" + index.ToString());
        //Load first part of the level in additive mode
        SceneManager.LoadSceneAsync("Level"+index.ToString()+"Part"+ index, LoadSceneMode.Additive);

    }
    //Start next level
    public void NextLevel()
    {
        CurrentLevelIndex++;
        LoadLevelWithIndex(CurrentLevelIndex);
    }
    //Restart current level
    public void RestartLevel()
    {
        LoadLevelWithIndex(CurrentLevelIndex);
    }
    //New game, load level 0
    public void NewGame()
    {
        LoadLevelWithIndex(0);
    }

    public int GetNameFromIndex()
    {
        return 0;
    }

    /*
     * Menus
     */

    //Load main Menu
    public void LoadMainMenu()
    {
        SceneManager.LoadSceneAsync(menus[(int)Type.Main_Menu].sceneName);
    }

    //Load Pause Menu
    public void LoadPauseMenu()
    {
        SceneManager.LoadSceneAsync(menus[(int)Type.Pause_Menu].sceneName);
    }



}
