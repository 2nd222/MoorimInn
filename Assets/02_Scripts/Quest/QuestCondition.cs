using System;
using UnityEngine;
using static Constants;

/// <summary>
/// 퀘스트 종류 enum
/// </summary>
public enum QuestEventType
{
    ItemChanged,
    Fail,
    ServeCustomer,
    CatchThief,
    ProgressCheck,
    GenerateQuest
}

public enum QuestConditionType
{
    Item,
    Serve,
    FailLimit,
    CatchThief,
    ProgressCheck,
}

/// <summary>
/// 퀘스트에 대한 이벤트가 발생했을 때 어떤 이벤트인지 정의하기 위한 데이터 struct
/// </summary>
public struct QuestEvent
{
    public QuestEventType type;
    public int ID; // 아이템이 필요한 퀘스트의 경우, itemID를 담는 값, 혹은 실패의 경우 퀘스트 ID
    public int currentCount; // 퀘스트 진행 상황 카운트 값
}

/// <summary>
/// 퀘스트 완료 조건을 생성하기 위한 SO(퀘스트의 정보를 담고 있는 questdata에 변수 값으로 들어갈 SO를 생성하기 위한 클래스)
/// 진행도 직접 관리
/// 이벤트 처리
/// 완료 여부 판단
/// UI용 데이터 제공
/// </summary>
public abstract class QuestCondition : ScriptableObject
{
    public abstract QuestConditionType ConditionType { get; }
    
    public abstract void Init(Inventory inventory = null);
    public abstract void Notify(QuestEvent questEvent, GuestMood mood = GuestMood.Neutral);
    public abstract bool IsComplete();
    public abstract QuestCondition Clone();
    
    public abstract string GetDescription();
    public abstract string GetCurrentCount();
    public abstract void SetCurrentCount(int value);
    public abstract string GetRequiredCount();
    
    public event Action OnConditionUpdated;
    protected void NotifyUpdated()
    {
        OnConditionUpdated?.Invoke();
    }

    public abstract void SaveRuntimeData(ConditionRuntimeData data);
    public abstract void LoadRuntimeData(ConditionRuntimeData data);
}