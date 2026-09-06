using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorySettingMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button toggleButton;
    [SerializeField] private Button logButton;
    [SerializeField] private Button autoButton;
    [SerializeField] private Button sceneSkipButton;
    [SerializeField] private Button storySkipButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private ScrollMenu scrollMenu;
    
    [SerializeField] private Sprite autoButtonSprite;
    [SerializeField] private Sprite autoButtonClickedSprite;
    
    private StoryUI storyUI;
    private bool isOpen;

    public void Init(StoryUI ui)
    {
        storyUI = ui;

        toggleButton.onClick.AddListener(scrollMenu.Toggle);

        logButton.onClick.AddListener(storyUI.ToggleLogUI);
        autoButton.onClick.AddListener(ToggleAutoPlay);

        settingButton.onClick.AddListener(storyUI.StopAllAutoPlay);
        
        sceneSkipButton.onClick.AddListener(() =>
        {
            if (StoryManager.Instance.StoryUI.CanSkip())
            {
                StoryManager.Instance.SkipCurrentScene();
            }
        });

        storySkipButton.onClick.AddListener(() =>
        {
            if (StoryManager.Instance.StoryUI.CanSkip())
            {
                StoryManager.Instance.SkipCurrentStory();
            }
        });
    }

    private void ToggleAutoPlay()
    {
        bool enabled = storyUI.ToggleAutoPlay();
        if (enabled)
            autoButton.GetComponent<Image>().sprite = autoButtonClickedSprite;
        else
            autoButton.GetComponent<Image>().sprite = autoButtonSprite;
    }

    public void RefreshAutoButtonSprite()
    {
        autoButton.GetComponent<Image>().sprite = autoButtonSprite;
    }
}
