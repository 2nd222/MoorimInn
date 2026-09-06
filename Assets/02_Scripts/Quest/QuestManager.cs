using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Constants;


/// <summary>
/// 퀘스트 데이터를 관리하는 매니저 UI에는 관여하지 않고 액션을 통해 변경사항을 갱신하도록 한다.
/// 퀘스트 생성
/// waiting / active 리스트 관리
/// 이벤트 전달 (Notify)
/// Observer 등록/해제
/// </summary>
public class QuestManager : Singleton<QuestManager>, ISubject, IInitializable
{
    private List<IObserver> observers = new List<IObserver>();
    
    [SerializeField] private QuestGenerator generator;
    [SerializeField] private Inventory inventory;
    [SerializeField] private GuestResultTracker guestResultTracker;
    
    private List<Quest> activeQuests = new List<Quest>();
    public List<Quest> ActiveQuests => activeQuests;
    private List<Quest> waitingQuests = new List<Quest>();
    
    private List<Quest> completedQuests = new List<Quest>();
    public List<Quest> CompletedQuests => completedQuests;
    
    public event Action<Quest> OnActiveQuestAdded; // ActiveQuest 리스트에 새 퀘스트 추가 시 호출 (quest pop up controller에서 사용)
    public event Action<Quest> OnActiveQuestRemoved; // ActiveQuest 리스트에 퀘스트 제거 시 호출
    
    public event Action OnActiveQuestChanged; // 진행 중인 퀘스트 UI 갱신 시 호출
    public event Action OnWaitingQuestChanged;// 수락 대기중인 퀘스트 수 변경 시 호출
    

    private IEnumerator Start()
    {
        yield return null;
        
        inventory = InventoryManager.Instance.GetInventory(InventoryType.Chef);
    }

    public void Init()
    {
        inventory = InventoryManager.Instance.GetInventory(InventoryType.Chef);
        guestResultTracker = FindFirstObjectByType<GuestResultTracker>();
        generator  = FindFirstObjectByType<QuestGenerator>();
        
        if(guestResultTracker != null)
            guestResultTracker.OnGuestRecorded += DetectGuestMood;
    }
    
    /// <summary>
    /// 퀘스트 수락 전 제안
    /// </summary>
    /// <param name="data"></param>
    public void OfferQuest(Quest quest)
    {
        if (HasQuest(quest.QuestID))
            return;

        waitingQuests.Add(quest);
        
        OnWaitingQuestChanged?.Invoke();
    }
    
    /// <summary>
    /// 퀘스트 창에서 퀘스트를 받을 때
    /// 퀘스트 클리어 창에 퀘스트 클리어 바를 생성하는 함수
    /// </summary>
    /// <param name="index"></param>
    public void AcceptQuest(Quest quest)
    {
        if (HasQuest(quest.QuestID) && activeQuests.Contains(quest))
            return;

        quest.SetAccepted();
        
        activeQuests.Add(quest);
        waitingQuests.Remove(quest);
        
        Instance.AddObserver(quest);
     
        OnActiveQuestAdded?.Invoke(quest);
        OnWaitingQuestChanged?.Invoke();
        OnActiveQuestChanged?.Invoke();
    }

    /// <summary>
    /// 퀘스트 완료될 때 호출, 현재 진행하는 activequest 리스트에서 퀘스트 삭제 후 알림 액션 호출
    /// </summary>
    /// <param name="quest"></param>
    public void EraseQuestFromActive(Quest quest)
    {
        if (activeQuests.Exists(q => q.QuestID == quest.QuestID))
            activeQuests.Remove(quest);
        
        OnActiveQuestRemoved?.Invoke(quest);
        OnActiveQuestChanged?.Invoke();
    }
    
    /// <summary>
    /// 퀘스트를 생성할 때 퀘스트 매니저에서 퀘스트를 추적하기 위해 설정을 하는 함수
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver(IObserver observer)
    {
        observers.Add(observer);
        Debug.Log($"퀘스트 {observer.QuestName}를 등록하였습니다.");
    }

    /// <summary>
    ///  퀘스트 완료, 실패 등으로 퀘스트 추적을 그만 두기 위한 함수
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver(IObserver observer)
    {
        observers.Remove(observer);
        Debug.Log($"퀘스트 {observer.QuestName}를 삭제하였습니다.");
    }
    
    /// <summary>
    /// 이벤트 타입이 퀘스트생성이면 생성기에게 퀘스트 생성 명령
    /// 퀘스트 진행 여부를 QuestEvent를 받았을 때, 지금 추적 중인 모든 퀘스트에게 공지하여 본인이 해당이 되는지 알리는 함수
    ///
    /// 아래와 같은 호출 방식을 이용하면 랜덤 퀘스트 생성
    ///QuestManager.Instance.NotifyListener(new QuestEvent
    /// {
    ///     type = QuestEventType.GenerateQuest
    /// });
    /// 
    /// </summary>
    /// <param name="questEvent"></param>
    public void NotifyListener(QuestEvent questEvent, QuestType questType = QuestType.None)
    {
        if (questEvent.type == QuestEventType.GenerateQuest)
        {
            var quest = generator.GenerateRandomQuest(questType);

            if (quest != null && !HasSameRandomQuest(quest))
                OfferQuest(quest);

            return;
        }

        GuestMood guestMood = (GuestMood)questEvent.ID;
        
        for (int i = observers.Count - 1; i >= 0; i--)
            observers[i].Notify(questEvent, guestMood);
    }

    private void DetectGuestMood(GuestMood guestMood)
    {
        int moodID = (int)guestMood; 
        
        NotifyListener(new QuestEvent{
            type = QuestEventType.Fail,
            ID = moodID,
            currentCount = 1
        });
        NotifyListener(new QuestEvent{
            type = QuestEventType.ServeCustomer,
            ID = moodID,
            currentCount = 1
        });
    }
    
    /// <summary>
    /// 하루가 종료 됐을 때 그 날 생성된 랜덤 퀘스트들은 전부 삭제
    /// </summary>
    public void ClearRandomQuests(QuestType questType = QuestType.None)
    {
        ClearWaitingQuests(questType);
        bool isChanged = false;
        for (int i = activeQuests.Count - 1; i >= 0; i--)
        {
            var quest = activeQuests[i];

            if (quest.QuestType != questType)
                continue;
                
            if (quest.QuestType == QuestType.MainStory || quest.QuestType == QuestType.SubStory)
                continue;

            Debug.Log(quest.CanClear());
            
            if(!quest.CanClear())
                quest.Fail(); // 내부에서 RemoveObserver 호출됨
            
            isChanged = true;
        }
        if (isChanged)
            OnActiveQuestChanged?.Invoke();
    }
    
    /// <summary>
    /// 랜덤 퀘스트 중에서 동일한 퀘스트가 이미 등록됐는지 확인하는 함수
    /// </summary>
    /// <param name="newQuest"></param>
    /// <returns></returns>
    public bool HasSameRandomQuest(Quest newQuest)
    {
        return activeQuests.Exists(q =>
                   q.QuestType is not (QuestType.MainStory or QuestType.SubStory) &&
                   q.QuestID == newQuest.QuestID)
               || waitingQuests.Exists(q =>
                   q.QuestType is not (QuestType.MainStory or QuestType.SubStory) &&
                   q.QuestID == newQuest.QuestID);
    }
    
    /// <summary>
    /// 현재 진행 중에 있는 퀘스트 조회 퀘스트 타입으로 검색 함수
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public List<Quest> GetQuests(QuestType type)
    {
        return activeQuests.Where(q => q.QuestType == type).ToList();
    }
    
    
    /// <summary>
    /// 퀘스트 완료 NPC가 볼 수 있는 퀘스트들 받기
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    public List<Quest> GetQuestsByNPC(NPCType npc)
    {
        return activeQuests.Where(q => q.targetNPC == npc).ToList();
    }
    
    /// <summary>
    /// 현재 퀘스트 수락 대기 창에 있는 퀘스트 조회
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public List<Quest> GetWaitingQuests(QuestType type)
    {
        return waitingQuests.Where(q => q.QuestType == type).ToList();
    }
    
    /// <summary>
    /// 동일한 퀘스트 있는 지 확인용
    /// </summary>
    /// <param name="questID"></param>
    /// <returns></returns>
    public bool HasQuest(int questID)
    {
        return activeQuests.Exists(q => q.QuestID == questID)
               || waitingQuests.Exists(q => q.QuestID == questID);
    }

    /// <summary>
    /// 선행퀘스트 완료됐는지 확인
    /// </summary>
    /// <param name="questID"></param>
    /// <returns></returns>
    public bool PrerequisiteQuestIsDone(int questID)
    {
        foreach (var completedQuest in completedQuests)
        {
            if (completedQuest.Data.id == questID)
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// 퀘스트 waiting에 있는 거는 안받으면 사라지게
    /// </summary>
    /// <param name="type"></param>
    public void ClearWaitingQuests(QuestType type)
    {
        waitingQuests.RemoveAll(q =>
        {
            //Debug.Log($"대기 퀘스트 삭제 : {q.Conditions[0].GetDescription()}");
            return q.QuestType == type;
        });
        OnWaitingQuestChanged?.Invoke();
    }
    
    /// <summary>
    /// 퀘스트 데이터 저장
    /// </summary>
    /// <returns></returns>
    public QuestSaveData GetSaveData()
    {
        var data = new QuestSaveData();

        foreach (var quest in waitingQuests)
        {
            // 에셋이 아니라 이 퀘스트의 런타임 보상을 저장한다
            var questData = new QuestRuntimeData
            {
                questID = quest.QuestID,
                rewardGold = quest.RuntimeReward.gold,
                rewardFame = quest.RuntimeReward.reputation
            };

            var conditions = quest.GetConditions();

            for (int i = 0; i < conditions.Count; i++)
            {
                var cond = conditions[i];

                var condData = new ConditionRuntimeData
                {
                    conditionIndex = i,
                    conditionType = cond.ConditionType,
                };

                cond.SaveRuntimeData(condData);

                questData.conditions.Add(condData);
            }

            data.waitingQuests.Add(questData);
        }
        
        foreach (var quest in activeQuests)
        {
            // 에셋이 아니라 이 퀘스트의 런타임 보상을 저장한다
            var questData = new QuestRuntimeData
            {
                questID = quest.QuestID,
                rewardGold = quest.RuntimeReward.gold,
                rewardFame = quest.RuntimeReward.reputation
            };

            var conditions = quest.GetConditions();

            for (int i = 0; i < conditions.Count; i++)
            {
                var cond = conditions[i];

                var condData = new ConditionRuntimeData
                {
                    conditionIndex = i,
                    conditionType = cond.ConditionType,
                };

                cond.SaveRuntimeData(condData);

                questData.conditions.Add(condData);
            }

            data.activeQuests.Add(questData);
        }

        foreach (var quest in completedQuests)
        {
            data.completedQuestIDs.Add(quest.QuestID);
        }

        return data;
    }
    
    public void LoadFromData(QuestSaveData data)
    {
        activeQuests.Clear();
        waitingQuests.Clear();
        completedQuests.Clear();

        foreach (var savedQuest in data.waitingQuests)
        {
            var quest = generator.GenerateQuestFromSave(savedQuest);

            if (quest == null)
                continue;

            waitingQuests.Add(quest);
        }
        
        foreach (var savedQuest in data.activeQuests)
        {
            var quest = generator.GenerateQuestFromSave(savedQuest);

            if (quest == null)
                continue;

            activeQuests.Add(quest);
            quest.SetAccepted();
            AddObserver(quest);
        }

        foreach (var id in data.completedQuestIDs)
        {
            var quest = generator.GenerateQuestByID(id);
            if (quest != null)
                completedQuests.Add(quest);
        }
    }
    
    public void ResetData()
    {
        activeQuests.Clear();
        waitingQuests.Clear();
        completedQuests.Clear();

        Debug.Log("Quest Reset 완료");
    }
}