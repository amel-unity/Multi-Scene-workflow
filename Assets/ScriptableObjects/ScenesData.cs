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
    public bool Save;

    //Load a scene with a given index
    public void LoadLevelWithIndex(int index)
    {
        string currentLevelName = levels[index].sceneName;
        SceneManager.LoadSceneAsync(currentLevelName);
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
    //Load main Menu
    public void LoadMainMenu()
    {
        SceneManager.LoadSceneAsync(menus[(int)Type.Main_Menu].sceneName);
    }

    //Load main Menu
    public void LoadPauseMenu()
    {
        SceneManager.LoadSceneAsync(menus[(int)Type.Pause_Menu].sceneName);
    }



}
