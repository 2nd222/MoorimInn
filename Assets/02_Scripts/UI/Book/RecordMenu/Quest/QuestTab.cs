using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Constants;
using System.Collections.Generic;
using UnityEngine.Events;

[Serializable]
public class CategoryPair
{
    public QuestCategory category;
    public Toggle toggleTab;
}

public class QuestTab : BaseTab
{
    [SerializeField] private CategoryPair[] categoryTabs;

    private Dictionary<QuestCategory, List<Quest>> dicActiveQuests = new Dictionary<QuestCategory, List<Quest>>();
    private Dictionary<QuestCategory, List<Quest>> dicCompletedQuests = new Dictionary<QuestCategory, List<Quest>>();

    private QuestSlot currentSelectedSlot;

    [SerializeField] private QuestAccordionGroup inProgressGroup;
    [SerializeField] private QuestAccordionGroup completedGroup;

    [Header("���� ������")]
    [SerializeField] private TextMeshProUGUI txtCategoryName;

    [Header("������ ������")]
    [SerializeField] private TextMeshProUGUI txtQuestName;
    [SerializeField] private TextMeshProUGUI txtDetail;
    [SerializeField] private TextMeshProUGUI txtDescription;
    [SerializeField] private GameObject rewardPrefab;
    [SerializeField] private Transform tsRewardParent;

    private QuestCategory currentCategory = QuestCategory.Main;

    private bool isInitCategory = false;

    public override void SetupData()
    {
        if (this.currentSelectedSlot)
            this.currentSelectedSlot.SetSelect(false);

        ListInit();
        QuestCache();
        CategoryTabInit();
        SetCategory(QuestCategory.Main);
    }
    private void QuestListUpdate()
    {
        switch (this.currentCategory)
        {
            case QuestCategory.Main: 
                this.txtCategoryName.text = "메인 퀘스트";
                break;
            case QuestCategory.Sub: 
                this.txtCategoryName.text = "서브 퀘스트";
                break;
            case QuestCategory.Repeat: 
                this.txtCategoryName.text = "반복 퀘스트"; 
                break;
        }

        List<Quest> displayActiveQuests = this.dicActiveQuests[this.currentCategory];
        List<Quest> displayCompletedQuests = this.dicCompletedQuests[this.currentCategory];

        RefreshQuestGroup(this.inProgressGroup, displayActiveQuests);
        RefreshQuestGroup(this.completedGroup, displayCompletedQuests);

        ClearRightPage();
    }

    private void ShowQuest(QuestSlot clickedSlot)
    {
        if (this.currentSelectedSlot != null)
            this.currentSelectedSlot.SetSelect(false);

        this.currentSelectedSlot = clickedSlot;
        this.currentSelectedSlot.SetSelect(true);

        QuestData data = this.currentSelectedSlot.questData;

        // ������ �������� ��� ������
        this.txtQuestName.text = data.questName;
        this.txtDetail.text = data.questDetail;
        foreach(QuestCondition v in data.conditions)
            this.txtDescription.text = $"{v.GetDescription()}({v.GetCurrentCount()} / {v.GetRequiredCount()})";
        // ���� �߰��ؾ���
    }
    private void ListInit()
    {
        this.dicActiveQuests[QuestCategory.Main] = new List<Quest>();
        this.dicActiveQuests[QuestCategory.Sub] = new List<Quest>();
        this.dicActiveQuests[QuestCategory.Repeat] = new List<Quest>();

        this.dicCompletedQuests[QuestCategory.Main] = new List<Quest>();
        this.dicCompletedQuests[QuestCategory.Sub] = new List<Quest>();
        this.dicCompletedQuests[QuestCategory.Repeat] = new List<Quest>();
    }
    private void CategoryTabInit()
    {
        if (this.isInitCategory) return;

        for (int i = 0; i < this.categoryTabs.Length; i++)
        {
            int index = i;
            this.categoryTabs[i].toggleTab.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    this.currentCategory = this.categoryTabs[index].category;
                    QuestListUpdate();
                }
            });
        }
        this.isInitCategory = true;
    }

    private void SetCategory(QuestCategory targetCategory)
    {
        foreach (var pair in this.categoryTabs)
        {
            if (pair.category == targetCategory)
            {
                pair.toggleTab.SetIsOnWithoutNotify(false);
                pair.toggleTab.isOn = true;
                break;
            }
        }
    }

    // ������ ������ ����
    private void ClearRightPage()
    {
        if (this.currentSelectedSlot != null)
        {
            this.currentSelectedSlot.SetSelect(false);
            this.currentSelectedSlot = null;
        }

        this.txtQuestName.text = "선택된 퀘스트가 없습니다.";
        this.txtDetail.text = "";
        this.txtDescription.text = "";
    }

    // ����Ʈ ������ �޾ƿ���
    private void QuestCache()
    {
        foreach (List<Quest> list in this.dicActiveQuests.Values)
            list.Clear();
        foreach (List<Quest> list in this.dicCompletedQuests.Values)
            list.Clear();

        foreach (Quest q in QuestManager.Instance.ActiveQuests)
            AddQuestList(q, this.dicActiveQuests);

        foreach (Quest q in QuestManager.Instance.CompletedQuests)
            AddQuestList(q, this.dicCompletedQuests);
    }
    // ī�װ��� �°� ����Ʈ �߰�
    private void AddQuestList(Quest q, Dictionary<QuestCategory, List<Quest>> targetDic)
    {
        if (q == null)
            return;
        switch (q.QuestType)
        {
            case QuestType.MainStory:
                targetDic[QuestCategory.Main].Add(q);
                break;
            case QuestType.SubStory:
                targetDic[QuestCategory.Sub].Add(q);
                break;
            case QuestType.Merchant:
            case QuestType.PostMan:
                targetDic[QuestCategory.Repeat].Add(q);
                break;
        }
    }
    private void RefreshQuestGroup(QuestAccordionGroup targetGroup, List<Quest> questList)
    {
        if (targetGroup != null)
        {
            targetGroup.UpdateList(questList, ShowQuest);
            targetGroup.OnClickGroup(false);
        }
    }

    public override void ResetSetting()
    {
        SetCategory(QuestCategory.Main);
    }
}
