using System;
using UnityEngine;
using static Constants;


/// <summary>
/// 실패 횟수를 체크하는 퀘스트 조건을 만들기 위한 클래스(ex. 기분나쁜 손님을 3명 이상 만들지 마시오)
/// </summary>
[CreateAssetMenu(menuName = "Quest/Condition/Fail")]
public class FailLimitCondition : QuestCondition
{
    public override QuestConditionType ConditionType => QuestConditionType.FailLimit;
    
    public GuestMood requiredGuestMood;
    public int maxFailCount;
    private int currentFailCount;
    
    private int acceptedDay;

    /// <summary>
    /// 실패 횟수를 0에서 시작하면서 카운팅
    /// </summary>
    /// <param name="inventory"></param>
    public override void Init(Inventory inventory = null)
    {
        currentFailCount = 0;
        acceptedDay = DayManager.Instance.DayData.day;
    }

    /// <summary>
    /// 만약 들어온 questevent의 type이 fail이면 업데이트(조건 구분하기 위한 id 추가 필요)
    /// </summary>
    /// <param name="questEvent"></param>
    public override void Notify(QuestEvent questEvent, GuestMood mood = GuestMood.Neutral)
    {
        if (questEvent.type != QuestEventType.Fail)
            return;

        if (mood > requiredGuestMood)
            return;
        
        currentFailCount += questEvent.currentCount;
        NotifyUpdated();
    }

    /// <summary>
    /// 퀘스트 완료인 지 체크 함수
    /// </summary>
    /// <returns></returns>
    public override bool IsComplete()
    {
        bool isNextDay = DayManager.Instance.DayData.day > acceptedDay;
        return isNextDay && currentFailCount <= maxFailCount;
    }

    /// <summary>
    /// 퀘스트 실패인 지 체크 함수
    /// </summary>
    /// <returns></returns>
    public bool IsFailed()
    {
        return currentFailCount > maxFailCount;
    }
    
    /// <summary>
    /// 퀘스트 파일에서 퀘스트 조건 등록을 위한 클로닝 함수
    /// </summary>
    /// <returns></returns>
    public override QuestCondition Clone()
    {
        var instance = CreateInstance<FailLimitCondition>();
        instance.requiredGuestMood = requiredGuestMood;
        instance.maxFailCount = maxFailCount;
        
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

        description = $"기분이 {mood} 이하인 손님 만들지 않기";
        return description;
    }
    
    public override string GetCurrentCount()
    {
        return currentFailCount.ToString();
    }

    public override void SetCurrentCount(int value)
    {
        currentFailCount = value;
    }

    public override string GetRequiredCount()
    {
        return maxFailCount.ToString();
    }

    public override void SaveRuntimeData(ConditionRuntimeData data)
    {
        data.maxFailCount = maxFailCount;
        data.failCount = currentFailCount;
    }

    public override void LoadRuntimeData(ConditionRuntimeData data)
    {
        maxFailCount = data.maxFailCount;
        currentFailCount = data.failCount;
    }
}