using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public class LevelSelectionUI : MonoBehaviour
{
    public static LevelSelectionUI Instance { get; private set; }

    public RectTransform ButtonListPlace;
    public Button ButtonPrefab;
    public Button BackButton;
    
    List<Button> m_EpisodeButtons = new List<Button>();
    List<List<Button>> m_LevelButtons = new List<List<Button>>();
    
    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Init()
    {
        for (int i = 0; i < GameDatabase.Instance.episodes.Length; ++i)
        {
            var ep = GameDatabase.Instance.episodes[i];

            Button b = Instantiate(ButtonPrefab);
            Text t = b.GetComponentInChildren<Text>();

            t.text = $"Episode {i + 1}";

            var i1 = i;
            b.onClick.AddListener(() => {UIAudioPlayer.PlayPositive(); OpenEpisode(i1);});
            b.transform.SetParent(ButtonListPlace);
                
            m_EpisodeButtons.Add(b);
            
            m_LevelButtons.Add(new List<Button>());

            for (int j = 0; j < ep.scenes.Length; ++j)
            {
                Button levelB = Instantiate(ButtonPrefab);
                t = levelB.GetComponentInChildren<Text>();

                var j1 = j;
                levelB.onClick.AddListener(() =>
                {
#if UNITY_EDITOR
                    EditorSceneManager.LoadSceneInPlayMode(ep.scenes[j1], new LoadSceneParameters());
#else
                    SceneManager.LoadScene(ep.scenes[j1]);
#endif
                });

                t.text = $"Level {j + 1}";
                
                levelB.transform.SetParent(ButtonListPlace);
                m_LevelButtons[i].Add(levelB);
            }
        }
    }

    void BackToPause()
    {
        UIAudioPlayer.PlayNegative();
        gameObject.SetActive(false);
        PauseMenu.Instance.Display();
    }

    void BackToEpisode()
    {
        DisplayEpisode();
        UIAudioPlayer.PlayNegative();
    }

    public void DisplayEpisode()
    {
        BackButton.onClick.RemoveAllListeners();
        BackButton.onClick.AddListener(BackToPause);
        
        gameObject.SetActive(true);
        
        foreach (RectTransform t in ButtonListPlace)
        {
            t.gameObject.SetActive(false);
        }

        foreach (var b in m_EpisodeButtons)
        {
            b.gameObject.SetActive(true);
        }
    }

    void OpenEpisode(int i)
    {
        BackButton.onClick.RemoveAllListeners();
        BackButton.onClick.AddListener(BackToEpisode);
        
        foreach (RectTransform t in ButtonListPlace)
        {
            t.gameObject.SetActive(false);
        }

        foreach (var b in m_LevelButtons[i])
        {
            b.gameObject.SetActive(true);
        }
    }
}
