using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 퀘스트 객체에 대한 클래스
/// Condition 리스트 보관
/// Condition에게 이벤트 전달
/// 전체 완료 여부 판단
/// Complete / Fail 처리
/// </summary>
public class Quest : IObserver
{
    private QuestData data;
    public QuestData Data => data;
    public QuestType QuestType => data.questType;
    
    public int QuestID { get; }
    public string QuestName { get; private set; }
    public bool IsCompleted { get; private set; }
    public bool IsAccepted  { get; private set; }
    public bool IsFailed { get; private set; }

    /// <summary>
    /// 이 퀘스트만의 보상. QuestData(에셋)의 값을 복사해서 쓴다.
    /// 에셋을 직접 수정하면 같은 템플릿으로 만든 다른 퀘스트의 보상까지 바뀌므로 반드시 사본을 사용한다.
    /// </summary>
    public QuestReward RuntimeReward { get; private set; }
    // public QuestReward RuntimeReward => data.reward; 

    public List<int> RewardRecipeIds;
    public List<int> RewardIngredientIds;
    
    public NPCType targetNPC; // 퀘스트 완료 할 수 있는 대상
    
    private List<QuestCondition> conditions;
    public List<QuestCondition> Conditions => conditions;
    
    private Inventory inventory;
    
    /// <summary>
    /// 퀘스트를 생성하는 생성자, 퀘스트 데이터를 받아서 퀘스트를 생성
    /// </summary>
    /// <param name="questData"></param>
    public Quest(QuestData questData, Inventory inventory)
    {
        data = questData;
        QuestID = data.id;
        QuestName = data.questName;
        IsCompleted = false;
        IsAccepted = false;
        
        targetNPC = questData.npcType;
        this.inventory = inventory;

        // 에셋 값을 복사 (에셋 오염 방지)
        RuntimeReward = new QuestReward
        {
            gold = questData.reward.gold,
            reputation = questData.reward.reputation
        };
        
        conditions = new List<QuestCondition>();
        foreach (var condition in data.conditions)
        {
            var instance = condition.Clone();
            instance.Init(inventory);
                
            conditions.Add(instance);
        }

        RewardRecipeIds = questData.rewardRecipeIds;
        RewardIngredientIds = questData.rewardIngredientIds;
    }
    
    public Quest(QuestData questData, Inventory inventory, QuestRuntimeData runtimeData)
    {
        data = questData;
        QuestID = data.id;
        QuestName = data.questName;

        IsCompleted = false;
        IsAccepted = false;

        targetNPC = questData.npcType;
        this.inventory = inventory;

        RewardRecipeIds = questData.rewardRecipeIds;
        RewardIngredientIds = questData.rewardIngredientIds;

        // 저장된 보상값으로 복원 (에셋은 건드리지 않음)
        RuntimeReward = new QuestReward
        {
            gold = runtimeData.rewardGold,
            reputation = runtimeData.rewardFame
        };

        conditions = new List<QuestCondition>();

        foreach (var savedCond in runtimeData.conditions)
        {
            QuestCondition condition = CreateCondition(savedCond.conditionType);

            if (condition == null)
                continue;

            condition.Init(inventory);
            condition.LoadRuntimeData(savedCond);

            conditions.Add(condition);
        }
    }

    /// <summary>
    /// 난이도 스케일링 등으로 이 퀘스트만의 보상을 설정할 때 사용
    /// </summary>
    public void SetRuntimeReward(int gold, int reputation)
    {
        RuntimeReward.gold = gold;
        RuntimeReward.reputation = reputation;
    }

    /// <summary>
    /// UI 표시용 보상 목록. 에셋이 아니라 이 퀘스트의 런타임 보상을 기준으로 만든다.
    /// </summary>
    public List<QuestRewardEntry> GetRuntimeRewardEntries()
    {
        List<QuestRewardEntry> list = new();

        if (RuntimeReward.gold > 0)
        {
            list.Add(new QuestRewardEntry
            {
                type = QuestRewardType.Gold,
                amount = RuntimeReward.gold
            });
        }

        if (RuntimeReward.reputation > 0)
        {
            list.Add(new QuestRewardEntry
            {
                type = QuestRewardType.Reputation,
                amount = RuntimeReward.reputation
            });
        }

        if (RewardRecipeIds != null)
        {
            foreach (int id in RewardRecipeIds)
            {
                list.Add(new QuestRewardEntry
                {
                    type = QuestRewardType.Recipe,
                    id = id
                });
            }
        }

        if (RewardIngredientIds != null)
        {
            foreach (int id in RewardIngredientIds)
            {
                list.Add(new QuestRewardEntry
                {
                    type = QuestRewardType.Ingredient,
                    id = id
                });
            }
        }

        return list;
    }
    
    /// <summary>
    /// 관찰자로 부터 퀘스트 진행상황에 대한 업데이트 공지(QuestEvent의 정보)가 왔을 때 본인의 퀘스트가 진행된 건지 확인하는 함수
    /// </summary>
    /// <param name="questEvent"></param>
    public void Notify(QuestEvent questEvent, Constants.GuestMood mood = Constants.GuestMood.Neutral)
    {
        if (IsCompleted || IsFailed)
            return;
        
        Debug.Log(questEvent.ToString());
        
        foreach (var condition in conditions)
        {
            condition.Notify(questEvent, mood);
            
            if (condition is FailLimitCondition failCondition && failCondition.IsFailed())
            {
                Fail();
                return;
            }
        }
    }

    /// <summary>
    /// 클리어 가능한 상황인지 여부를 확인하는 함수
    /// </summary>
    /// <returns></returns>
    public bool CanClear()
    {
        foreach (var condition in conditions)
        {
            if (!condition.IsComplete())
                return false;
        }
        return true;
    }
    
    /// <summary>
    /// 아이템을 인벤토리에서 현재 갖고 있는 양을 확인하고 제출 가능한지 확인하는 함수(퀘스트 클리어 시, 아이템 제출을 할 때 사용)
    /// </summary>
    /// <param name="inventory"></param>
    /// <returns></returns>
    public bool CanRemoveItems(Inventory inventory)
    {
        foreach (var condition in conditions)
        {
            if (condition is ItemCondition itemCondition)
            {
                int have = inventory.GetItemCount(itemCondition.itemID);
                if (have < itemCondition.requiredCount)
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 아이템을 제출하는 퀘스트의 경우, 퀘스트 완료 시에 아이템을 인벤토리에서 지워달라고 요청하는 함수
    /// </summary>
    /// <param name="inventory"></param>
    public void CostForQuestClear(Inventory inventory)
    {
        foreach (var condition in conditions)
        {
            if (condition is ItemCondition itemCondition)
            {
                inventory.RemoveItem(itemCondition.itemID, itemCondition.requiredCount);
            }
        }
    }
    
    /// <summary>
    ///  퀘스트 완료시 호출되는 함수, 퀘스트가 성공됐다고 bool값 갱신
    /// </summary>
    public void Complete()
    {
        IsCompleted = true;
        Debug.Log($"퀘스트 완료: {QuestName} / 보상 {RuntimeReward.gold}원, 명성 {RuntimeReward.reputation}");

        DailyTestLogger.Instance?.RecordQuestCompleted(this);

        // 퀘스트 보상은 인연 효과(획득률 증가)를 적용하지 않고 표기된 값 그대로 지급
        EconomyManager.Instance.AddMoney(RuntimeReward.gold, false);
        EconomyManager.Instance.AddFame(RuntimeReward.reputation, false);
        
        QuestManager.Instance.CompletedQuests.Add(this);
        QuestManager.Instance.EraseQuestFromActive(this);
        QuestManager.Instance.RemoveObserver(this);
        
        if (QuestType == QuestType.SubStory)
        {
            SubStoryQuestController.Instance.OnQuestCompleted(this, inventory);
        }
        
        if(Data.questType == QuestType.MainStory)
            MainStoryQuestController.Instance.StartNextQuest(inventory);
    }
    
    /// <summary>
    /// 퀘스트 실패 시 호출되는 함수, 퀘스트 실패의 bool값 갱신
    /// </summary>
    public void Fail()
    {
        IsFailed = true;
        Debug.Log($"퀘스트 실패: {QuestName}");
        QuestManager.Instance.EraseQuestFromActive(this);
        QuestManager.Instance.RemoveObserver(this);
    }
    
    /// <summary>
    /// 퀘스트 조건들 호출(주로 진행도 확인 용)
    /// </summary>
    /// <returns></returns>
    public List<QuestCondition> GetConditions()
    {
        return conditions;
    }

    /// <summary>
    /// 퀘스트 수락 시 프로퍼티 체크
    /// </summary>
    public void SetAccepted()
    {
        IsAccepted = true;
    }
    
    
    /// <summary>
    /// 퀘스트 조건 삽입
    /// </summary>
    /// <param name="condition"></param>
    public void AddCondition(QuestCondition condition)
    {
        condition.Init(inventory);
        conditions.Add(condition);
    }
    
    
    /// <summary>
    /// 랜덤 아이템 조건용 조건 복제 함수
    /// </summary>
    /// <param name="targetCount"></param>
    public void AdjustRandomItemConditions(int targetCount)
    {
        List<ItemCondition> randomConditions = new();

        foreach (var condition in conditions)
        {
            if (condition is ItemCondition itemCondition && itemCondition.useRandomItem)
                randomConditions.Add(itemCondition);
        }

        // 랜덤 조건이 하나도 없으면 종료
        if (randomConditions.Count == 0)
            return;

        // ---------- 부족하면 추가 ----------
        if (randomConditions.Count < targetCount)
        {
            int need = targetCount - randomConditions.Count;

            for (int i = 0; i < need; i++)
            {
                ItemCondition source = randomConditions[i % randomConditions.Count];
                AddCondition(source.Clone() as ItemCondition);
            }
        }
        // ---------- 많으면 제거 ----------
        else if (randomConditions.Count > targetCount)
        {
            int removeCount = randomConditions.Count - targetCount;

            for (int i = 0; i < removeCount; i++)
            {
                conditions.Remove(randomConditions[randomConditions.Count - 1 - i]);
            }
        }
    }
    
    private QuestCondition CreateCondition(QuestConditionType type)
    {
        switch (type)
        {
            case QuestConditionType.Item:
                return ScriptableObject.CreateInstance<ItemCondition>();

            case QuestConditionType.Serve:
                return ScriptableObject.CreateInstance<ServeCondition>();

            case QuestConditionType.FailLimit:
                return ScriptableObject.CreateInstance<FailLimitCondition>();

            case QuestConditionType.CatchThief:
                return ScriptableObject.CreateInstance<CatchThiefCondition>();

            case QuestConditionType.ProgressCheck:
                return ScriptableObject.CreateInstance<ProgressCondition>();
            
            default:
                return null;
        }
    }
}