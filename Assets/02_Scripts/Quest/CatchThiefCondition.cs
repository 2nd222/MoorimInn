using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Condition/CatchThief")]
public class CatchThiefCondition : QuestCondition
{
    public int requiredCatchCount;
    private int currentCatchCount;
    
    public override QuestConditionType ConditionType => QuestConditionType.CatchThief;
    
    public override void Init(Inventory inventory = null)
    {
        currentCatchCount = 0;
    }

    public override void Notify(QuestEvent questEvent, Constants.GuestMood mood = Constants.GuestMood.Neutral)
    {
        if (questEvent.type != QuestEventType.CatchThief)
            return;
        
        currentCatchCount++;
    }

    public override bool IsComplete()
    {
        return currentCatchCount >= requiredCatchCount;
    }

    public override QuestCondition Clone()
    {
        var instance = CreateInstance<CatchThiefCondition>();
        instance.requiredCatchCount  = requiredCatchCount;
        
        return instance;
    }

    public override string GetDescription()
    {
        return "음식을 먹고 돈을 내지 않은 채 도망가는 손놈잡기";
    }

    public override string GetCurrentCount()
    {
        return currentCatchCount.ToString();
    }

    public override void SetCurrentCount(int value)
    {
        currentCatchCount = value;
    }

    public override string GetRequiredCount()
    {
        return requiredCatchCount.ToString();
    }

    public override void SaveRuntimeData(ConditionRuntimeData data)
    {
        data.catchRequired = requiredCatchCount;
        data.catchCurrent = currentCatchCount;
    }

    public override void LoadRuntimeData(ConditionRuntimeData data)
    {
        requiredCatchCount = data.catchRequired;
        currentCatchCount = data.catchCurrent;
    }
}
