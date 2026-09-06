using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCQuestSlot : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI title;
    
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;
    
    private Quest quest;
    private Action<Quest> callback;

    public void Init(Quest quest, Action<Quest, NPCQuestSlot> callback)
    {
        this.quest = quest;
        title.text = quest.QuestName;

        SetSelected(false);
        
        // 상태 표시
        if (!quest.IsAccepted)
            title.text += " [수락 가능]";
        else if (quest.CanClear())
            title.text += " [완료 가능]";
        else
            title.text += " [진행중]";

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => callback?.Invoke(quest, this));
    }
    
    public void SetSelected(bool isSelected)
    {
        background.sprite = isSelected ? selectedSprite : normalSprite;
    }
}
