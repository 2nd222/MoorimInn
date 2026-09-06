using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button StartNewGameButton;
    [SerializeField] private Button ResumeGameButton;
    [SerializeField] private Button SettingsButton;
    [SerializeField] private Button QuitButton;
    
    
    [SerializeField] private Setting setting;

    private void Start()
    {
        StartNewGameButton.onClick.AddListener(OnClickStartNewGameButton);
        ResumeGameButton.onClick.AddListener(OnClickLoad);
        SettingsButton.onClick.AddListener(OnClickSettingsButton);
        QuitButton.onClick.AddListener(OnClickQuitButton);
    }

    private void OnClickStartNewGameButton()
    {  
        SaveManager.Instance.StartNewGame();
    }

    private void OnClickSettingsButton()
    {
        UIManager.Instance.ActiveSetting(true);
    }

    private void OnClickQuitButton()
    {
        SaveManager.Instance.SaveAndQuit();
    }

    public void OnClickLoad()
    {
        UIManager.Instance.ActiveLoadUI(true);
    }
}
