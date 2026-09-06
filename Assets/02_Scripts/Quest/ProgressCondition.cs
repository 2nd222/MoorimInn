using UnityEngine;
using static Constants;

public enum ProgressType
{
    Money,
    Fame,
    Day,
    Prerequisite
}

/// <summary>
/// 게임 진행도에 따라 완료되는 퀘스트
/// </summary>
[CreateAssetMenu(menuName = "Quest/Condition/Progress")]
public class ProgressCondition : QuestCondition
{
    public override QuestConditionType ConditionType => QuestConditionType.ProgressCheck;
    
    public ProgressType type;
    
    private int currentMoney;
    private int currentFame;
    private int dayCount;

    public int requiredMoney;
    public int requiredFame;
    public int requiredDayCount;
    public QuestData Prerequisite_Quest; // 선행퀘스트는 퀘스트 매니저에서 완료된 퀘스트에서 id기반으로 찾아서 있으면 완료가능
    
    private DayManager dayManager;

    
    public override void Init(Inventory inventory = null)
    {
        dayManager = FindFirstObjectByType<DayManager>();
        
        currentMoney = EconomyManager.Instance.RestaurantEconomy.Money;
        currentFame = EconomyManager.Instance.RestaurantEconomy.Fame;
        dayCount = dayManager.DayData.day;
    }

    public override void Notify(QuestEvent questEvent, GuestMood mood = GuestMood.Neutral)
    {
        if(questEvent.type != QuestEventType.ProgressCheck)
            return;
        
        currentMoney = EconomyManager.Instance.RestaurantEconomy.Money;
        currentFame = EconomyManager.Instance.RestaurantEconomy.Fame;
        dayCount = dayManager.DayData.day;
        
        NotifyUpdated();
    }

    public override bool IsComplete()
    {
        switch (type)
        {
            case ProgressType.Money:
                return currentMoney >= requiredMoney;

            case ProgressType.Fame:
                return currentFame >= requiredFame;

            case ProgressType.Day:
                return dayCount >= requiredDayCount;

            case ProgressType.Prerequisite:
                if (Prerequisite_Quest == null)
                {
                    Debug.LogError($"[ProgressCondition] 선행퀘스트 없음: {name}");
                    return false;
                }
                return QuestManager.Instance.PrerequisiteQuestIsDone(Prerequisite_Quest.id);
        }

        return false;
    }

    public override QuestCondition Clone()
    {
        var instance = CreateInstance<ProgressCondition>();
        instance.type = type;
        instance.requiredMoney = requiredMoney;
        instance.requiredFame = requiredFame;
        instance.requiredDayCount = requiredDayCount;
        instance.Prerequisite_Quest = Prerequisite_Quest;
        
        return instance;
    }

    public override string GetDescription()
    {
        switch (type)
        {
            case ProgressType.Money:
                return $"재력 {requiredMoney} 필요";

            case ProgressType.Fame:
                return $"명성 {requiredFame} 필요";

            case ProgressType.Day:
                return $"{requiredDayCount}일 이상 진행";

            case ProgressType.Prerequisite:
                return Prerequisite_Quest != null
                    ? $"선행퀘스트: {Prerequisite_Quest.questName}"
                    : "선행퀘스트 없음";
        }

        return "";
    }

    public override string GetCurrentCount()
    {
        switch (type)
        {
            case ProgressType.Money:
                return currentMoney.ToString();

            case ProgressType.Fame:
                return currentFame.ToString();

            case ProgressType.Day:
                return dayCount.ToString();

            case ProgressType.Prerequisite:
                return QuestManager.Instance.PrerequisiteQuestIsDone(Prerequisite_Quest.id) ? "완료" : "미완료";

        }

        return "";
    }

    public override void SetCurrentCount(int value)
    {
        // 불러오기를 하지 않아도 상관없는 조건 타입
    }
    
    public override string GetRequiredCount()
    {
        switch (type)
        {
            case ProgressType.Money:
                return requiredMoney.ToString();

            case ProgressType.Fame:
                return requiredFame.ToString();

            case ProgressType.Day:
                return requiredDayCount.ToString();

            case ProgressType.Prerequisite:
                return "완료 필요";
        }

        return "";
    }
    
    public override void SaveRuntimeData(ConditionRuntimeData data)
    {
        data.progressType = type;

        data.requiredMoney = requiredMoney;
        data.requiredFame = requiredFame;
        data.requiredDay = requiredDayCount;

        data.prerequisiteQuestID = Prerequisite_Quest != null ? Prerequisite_Quest.id : -1;
    }
    
    public override void LoadRuntimeData(ConditionRuntimeData data)
    {
        type = data.progressType;

        requiredMoney = data.requiredMoney;
        requiredFame = data.requiredFame;
        requiredDayCount = data.requiredDay;

        if (data.prerequisiteQuestID != -1)
        {
            Prerequisite_Quest = DataManager.Instance.GetQuest(data.prerequisiteQuestID);
        }

        Init();
    }
}
