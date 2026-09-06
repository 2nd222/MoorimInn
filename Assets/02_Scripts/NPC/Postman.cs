using System;
using System.Collections;
using UnityEngine;
using static Constants;

public class Postman : MonoBehaviour, IInteractable, INPCSchedule
{ 
    private DayManager dayManager;
    public DayManager DayManager => dayManager;
    
    [SerializeField] private GameObject postmanView;
    [SerializeField] private PostManUI postmanUI;
    // ── IInteractable 구현 ──
 
    // 우편배달부는 클릭해서 상호작용
    public InteractMode Mode => InteractMode.ClickToInteract;
 
    private void OnEnable()
    {
        dayManager = DayManager.Instance;
        DayManager.Instance.RegisterNPC(this);
    }

    private void OnDisable()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.UnregisterNPC(this);
    }
    
    IEnumerator Start()
    {
        yield return null; // 1프레임 대기 (모든 Awake 끝나게)

        postmanUI.Init(this);
        postmanView.SetActive(false);
    }

    public void ViewActive()
    {
        if (IsAvailableDay())
        {
            GameManager.Instance.SetState(GameFlowState.NPC);
        }
        
        postmanView.SetActive(IsAvailableDay());
        postmanUI.gameObject.SetActive(IsAvailableDay());
    }
    
    public bool CanInteract(PlayerType playerType)
    {
        return IsAvailableDay();
    }
 
    public void Interact(PlayerController player)
    {
        if (!IsAvailableDay())
            return;
 
        // UI 창 호출
        // UI 호출 코드
        // UI 닫을 때 player.EndInteract() 호출 필요
    }
 
    public void OnInteractEnd(PlayerController player)
    {
        // ClickToInteract — 트리거 퇴장으로 호출되지 않음
    }
 
    /// <summary>
    /// 랜덤 퀘스트 이전거 삭제 후 3개 재발행 (매주 화 목요일 마다 호출)
    /// </summary>
    public void DailyQuestCreate()
    {
        if (IsAvailableDay())
        {
            QuestManager.Instance.ClearRandomQuests(QuestType.PostMan);
 
            for (int i = 0; i < 3; i++)
            {
                QuestManager.Instance.NotifyListener(new QuestEvent
                {
                    type = QuestEventType.GenerateQuest 
                }, QuestType.PostMan);
            }
        }
    }
    
    public bool IsAvailableDay()
    {
        return dayManager.DayData.dayOfWeek == DayOfWeek.Tuesday ||
               dayManager.DayData.dayOfWeek == DayOfWeek.Thursday;
    }
 
    public Vector3 GetInteractPosition()
    {
        return default;
    }
}
