using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCQuestDetailPanel : MonoBehaviour
{
    [Header("기본 UI")]
    [SerializeField] private TextMeshProUGUI questName;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private Transform content;

    [Header("조건 UI")]
    [SerializeField] private Transform conditionContent;
    [SerializeField] private GameObject conditionPrefab;

    [Header("보상 UI")]
    [SerializeField] private Transform rewardContent;
    [SerializeField] private QuestRewardUI rewardPrefab;
    
    [Header("버튼")]
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    private NPCQuestUI parent;
    private Quest currentQuest;

    private void OnEnable()
    {
        if (currentQuest != null)
            SubscribeConditions(true);
    }

    private void OnDisable()
    {
        if (currentQuest != null)
            SubscribeConditions(false);
    }

    private void SubscribeConditions(bool subscribe)
    {
        foreach (var condition in currentQuest.Conditions)
        {
            if (subscribe)
                condition.OnConditionUpdated += RefreshAll;
            else
                condition.OnConditionUpdated -= RefreshAll;
        }
    }
    
    private void RefreshAll()
    {
        if (currentQuest == null)
            return;

        RefreshConditions();
        SetupButton();
    }
    
    public void Show(Quest quest, NPCQuestUI parent)
    {
        if (currentQuest != null)
            SubscribeConditions(false);

        currentQuest = quest;

        SubscribeConditions(true);
        
        this.parent = parent;

        questName.text = quest.QuestName;
        description.text = quest.Data.questDetail;

        RefreshConditions();   
        RefreshRewards();
        
        SetupButton();        

        gameObject.SetActive(true);
        
        StartCoroutine(RebuildLayout());
    }

    private IEnumerator RebuildLayout()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)content);
    }
    
    private void RefreshConditions()
    {
        // 기존 제거
        foreach (Transform child in conditionContent)
            Destroy(child.gameObject);

        // 조건 생성
        foreach (var condition in currentQuest.Conditions)
        {
            var obj = Instantiate(conditionPrefab, conditionContent);
            var ui = obj.GetComponent<QuestConditionUI>();

            ui.Init(condition, !currentQuest.IsAccepted);
        }
    }
    
    private void RefreshRewards()
    {
        foreach (Transform child in rewardContent)
            Destroy(child.gameObject);

        // 에셋이 아니라 이 퀘스트의 런타임 보상을 표시한다
        foreach (var reward in currentQuest.GetRuntimeRewardEntries())
        {
            var ui = Instantiate(rewardPrefab, rewardContent);
            ui.Init(reward);
        }
    }

    private void SetupButton()
    {
        actionButton.onClick.RemoveAllListeners();
        actionButton.interactable = true;

        if (!currentQuest.IsAccepted)
        {
            buttonText.text = "수락";
            actionButton.onClick.AddListener(parent.AcceptQuest);
        }
        else if (currentQuest.CanClear())
        {
            buttonText.text = "완료";
            actionButton.onClick.AddListener(parent.ClearQuest);
        }
        else
        {
            buttonText.text = "조건 미달";
            actionButton.interactable = false;
        }
    }

    public void Clear()
    {
        // 구독 해제를 먼저 해야 한다. currentQuest를 null로 만든 뒤에는 OnDisable에서 해제할 수 없다
        if (currentQuest != null)
            SubscribeConditions(false);

        currentQuest = null;

        foreach (Transform child in conditionContent)
            Destroy(child.gameObject);
        
        foreach (Transform child in rewardContent)
            Destroy(child.gameObject);

        gameObject.SetActive(false);
    }
}