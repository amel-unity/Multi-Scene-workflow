using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
public class MainMenu : MonoBehaviour
{
    public GameObject Menu;
    public Image LoadingScreenBackground;
    public Image loadingProgressBar;

    public void StartGame()
    {
        HideMenu();
        ShowLoadingScreen();
        StartCoroutine(LoadingScreen());
    }

    public void HideMenu()
    {
        Menu.SetActive(false);
    }

    public void ShowLoadingScreen()
    {
        LoadingScreenBackground.gameObject.SetActive(true);
        loadingProgressBar.gameObject.SetActive(true);
    }

     
    IEnumerator LoadingScreen()
    {
        AsyncOperation startLevel = SceneManager.LoadSceneAsync(ScenesDataBase.instance.scenesNames[1]);
        loadingProgressBar.fillAmount = startLevel.progress;

        yield return new WaitForEndOfFrame();
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
