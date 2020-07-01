using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "sceneDB", menuName = "Scene Data/Database")]
public class ScenesData : ScriptableObject
{
    public List<Level> levels = new List<Level>();
    public List<Menu> menus = new List<Menu>();
    public int CurrentLevelIndex=1;

    /*
     * Levels
     */

    //Load a scene with a given index
    public void LoadLevelWithIndex(int index)
    {
        if (index <= levels.Count)
        {
            //Load Gameplay scene for the level
            SceneManager.LoadSceneAsync("Gameplay" + index.ToString());
            //Load first part of the level in additive mode
            SceneManager.LoadSceneAsync("Level" + index.ToString() + "Part1", LoadSceneMode.Additive);
        }
        //reset the index if we have no more levels
        else CurrentLevelIndex =1;
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
    //New game, load level 1
    public void NewGame()
    {
        LoadLevelWithIndex(1);
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
