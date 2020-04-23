using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public GameObject menu;
    public Image loadingScreenBackground;
    public Image loadingProgressBar;

    public void StartGame()
    {
        HideMenu();
        ShowLoadingScreen();
        StartCoroutine(LoadingScreen());
    }

    public void HideMenu()
    {
        menu.SetActive(false);
    }

    public void ShowLoadingScreen()
    {
        loadingScreenBackground.gameObject.SetActive(true);
        loadingProgressBar.gameObject.SetActive(true);
    }

    IEnumerator LoadingScreen()
    {
        AsyncOperation startLevel = SceneManager.LoadSceneAsync("Gameplay");
        while (startLevel.progress < 1)
        {
            loadingProgressBar.fillAmount = startLevel.progress;
            yield return new WaitForEndOfFrame();
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
