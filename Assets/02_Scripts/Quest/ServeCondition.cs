using UnityEngine;
using static Constants;

[CreateAssetMenu(menuName = "Quest/Condition/ServeGuest")]
public class ServeCondition : QuestCondition
{
    public override QuestConditionType ConditionType => QuestConditionType.Serve;
    
    public GuestMood requiredGuestMood;
    public int requireCount;
    private int currentServeCount;
    
    public override void Init(Inventory inventory = null)
    {
        currentServeCount = 0;
    }

    public override void Notify(QuestEvent questEvent, GuestMood mood = GuestMood.Neutral)
    {
        if (questEvent.type != QuestEventType.ServeCustomer)
            return;

        if (mood < requiredGuestMood)
            return;
        
        currentServeCount += questEvent.currentCount;
        
        if (currentServeCount >= requireCount)
            currentServeCount = requireCount;
        
        NotifyUpdated();
    }

    public override bool IsComplete()
    {
        return currentServeCount >= requireCount;
    }

    public override QuestCondition Clone()
    {
        var instance = CreateInstance<ServeCondition>();
        instance.requiredGuestMood = requiredGuestMood;
        instance.requireCount = requireCount;
        
        return instance;
    }

    public override string GetDescription()
    {
        string description = "";
        string mood = "";
        switch (requiredGuestMood)
        {
            case GuestMood.VeryBad:
                mood = "매우 나쁨";
                break;
            case GuestMood.Bad:
                mood = "나쁨";
                break;
            case GuestMood.Neutral:
                mood = "보통";
                break;
            case GuestMood.Good:
                mood = "좋음";
                break;
            case GuestMood.VeryGood:
                mood = "매우 좋음";
                break;
        }

        description = $"기분이 {mood} 이상인 손님 접객하기";
        return description;
    }

    public override string GetCurrentCount()
    {
        return currentServeCount.ToString();
    }

    public override void SetCurrentCount(int value)
    {
        currentServeCount = value;
    }

    public override string GetRequiredCount()
    {
        return requireCount.ToString();
    }

    public override void SaveRuntimeData(ConditionRuntimeData data)
    {
        data.serveRequired = requireCount;
        data.serveCurrent = currentServeCount;
    }

    public override void LoadRuntimeData(ConditionRuntimeData data)
    {
        requireCount = data.serveRequired;
        currentServeCount = data.serveCurrent;
    }
}
