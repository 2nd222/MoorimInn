using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 퀘스트 종류
/// </summary>
public enum QuestType
{
    None, // null 대신 쓸 타입
    Merchant, // 상인이 주는 퀘스트
    PostMan, // 우체부가 주는 퀘스트
    MainStory,
    SubStory
}

public enum NPCType
{
    None,
    Merchant,
    PostMan
}

/// <summary>
/// 퀘스트 보상 클래스
/// </summary>
[Serializable]
public class QuestReward
{
    public int gold;
    public int reputation;
}

public enum QuestConditionCategory
{
    None,
    RandomFood,
    FoodType,
    FixedFood,
    Serve,
    FailLimit,
    CatchThief,
}


/// <summary>
/// 각각의 퀘스트에 대한 데이터를 SO로 생성하기 위한 클래스
/// condition은 퀘스트 완료 조건에 대한 변수
/// </summary>
[CreateAssetMenu(fileName = "QuestData", menuName = "Quest/QuestData")]
public class QuestData : ScriptableObject
{
    public int id;
    public QuestType questType;
    public QuestConditionCategory category;
    public string questName;
    
    public bool useDifficultyScaling = true;
    
    public List<QuestCondition> conditions;
    
    public QuestReward reward;
    public List<int> rewardRecipeIds;
    public List<int> rewardIngredientIds;
    
    public NPCType npcType; // 퀘스트 완료 할 수 있는 대상

    public QuestData nextQuest; // 다음 퀘스트

    // 퀘스트 내용
    [TextArea(3, 10)]
    public string questDetail;
    
    /// <summary>
    /// 퀘스트 보상 리스트화해서 리턴
    /// </summary>
    /// <returns></returns>
    public List<QuestRewardEntry> GetRewardEntries()
    {
        List<QuestRewardEntry> list = new();

        if (reward.gold > 0)
        {
            list.Add(new QuestRewardEntry
            {
                type = QuestRewardType.Gold,
                amount = reward.gold
            });
        }

        if (reward.reputation > 0)
        {
            list.Add(new QuestRewardEntry
            {
                type = QuestRewardType.Reputation,
                amount = reward.reputation
            });
        }

        foreach (int id in rewardRecipeIds)
        {
            list.Add(new QuestRewardEntry
            {
                type = QuestRewardType.Recipe,
                id = id
            });
        }

        foreach (int id in rewardIngredientIds)
        {
            list.Add(new QuestRewardEntry
            {
                type = QuestRewardType.Ingredient,
                id = id
            });
        }

        return list;
    }
}

public enum QuestRewardType
{
    Gold,
    Reputation,
    Recipe,
    Ingredient
}

public class QuestRewardEntry
{
    public QuestRewardType type;

    public int amount;     // Gold, Reputation
    public int id;         // RecipeID, IngredientID
}
