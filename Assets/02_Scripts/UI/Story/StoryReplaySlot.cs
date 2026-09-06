using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StoryReplaySlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button slotButton; 
    
    [SerializeField] private Sprite slotSprite;
    [SerializeField] private Sprite selectedSprite;
    
    private Story story;
    private StoryDetailPanel storyDetailPanel;
    private StoryReplayTab parentTab;

    private bool isSelected;
    private void OnEnable()
    {
        slotButton.onClick.AddListener(OnClick);
        isSelected = false;
    }

    private void OnDisable()
    {
        if(slotButton)
            slotButton.onClick.RemoveListener(OnClick);
    }
    
    public void Setup(Story story, StoryDetailPanel detailPanel, StoryReplayTab parent)
    {
        this.story = story;
        this.storyDetailPanel = detailPanel;
        parentTab = parent;

        string title = $"[{story.mainSpeaker}] {story.storyName}";
        titleText.text = title;
    }

    public void OnClick()
    {
        parentTab.SelectSlot(this);
        storyDetailPanel.SetStory(story);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        slotButton.image.sprite = isSelected ? selectedSprite : slotSprite;
    }
}
