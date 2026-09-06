using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestPopupController : PopupController
{
    [SerializeField] private GameObject _questPopupPrefab;
    [SerializeField] private Transform _contentTransform;
    
    [SerializeField] private GameObject _questNotificationObj;
    
    Dictionary<QuestPopup, Quest>  _questPopups = new Dictionary<QuestPopup, Quest>();

    void Awake()
    {
        QuestManager.Instance.OnActiveQuestAdded += AddQuestPopup;
        QuestManager.Instance.OnActiveQuestRemoved += DeleteQuestPopup;
    }
    
    void OnEnable()
    {
        // 기존 제거
        foreach (var popup in _questPopups)
        {
            Destroy(popup.Key.gameObject);
        }
        _questPopups.Clear();

        // 다시 생성
        var quests = QuestManager.Instance.ActiveQuests;
        MakeAllQuestPopups(quests);
    }
    
    public void Showt()
    {
        base.Show();
        UpdateNotification(false);
    }

    public void OnClickConfirmButton()
    {
        Hide();
    }

    public void OnClickCancelButton()
    {
        Hide();
    }

    // QuestManager의 activeQuests 리스트 받아오기
    public void MakeAllQuestPopups(List<Quest> activeQuests)
    {
        foreach (var quest in activeQuests)
        {
            AddQuestPopup(quest);
        }
    }
    
    // 해당 퀘스트를 퀘스트 목록 UI에 추가 
    public void AddQuestPopup(Quest quest)
    {
        Debug.Log("<color=red>AddPopup</color>");
        var newQuestPopup = Instantiate(_questPopupPrefab, _contentTransform);
        Transform pos =  newQuestPopup.transform;
        newQuestPopup.transform.localPosition = new Vector3(pos.position.x, pos.position.y, 0);
        var questPopup =  newQuestPopup.GetComponent<QuestPopup>();
        questPopup.Init(quest);
        _questPopups.Add(questPopup, quest);
        
        UpdateNotification(!gameObject.activeSelf);
    }

    // 해당 퀘스트를 퀘스트 목록 UI에서 제거 
    public void DeleteQuestPopup(Quest quest)
    {
        foreach (var popup in _questPopups)
        {
            if (popup.Value == quest)
            {
                _questPopups.Remove(popup.Key);
                Destroy(popup.Key.gameObject);
                return;
            }
        }
        Debug.Log("<color=red>QuestPopupController: DestroyQuestPopup 실행되었지만 해당 questPopup을 찾을 수 없음</color>");
    }

    void UpdateNotification(bool isActive)
    {
        _questNotificationObj.SetActive(isActive);
    }

    void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnActiveQuestAdded -= AddQuestPopup;
            QuestManager.Instance.OnActiveQuestRemoved -= DeleteQuestPopup;
        }
    }
}
