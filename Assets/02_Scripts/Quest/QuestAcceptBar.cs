using UnityEngine;
using UnityEngine.UI;

public class QuestAcceptBar : MonoBehaviour
{
    [SerializeField] private Button acceptButton;

    [SerializeField] private Transform conditionContent;
    [SerializeField] private GameObject conditionSlotPrefab;
    
    private Quest _quest;

    public void Init(Quest quest)
    {
        _quest = quest;

        acceptButton.onClick.RemoveAllListeners();
        acceptButton.onClick.AddListener(OnClickAccept);
        
        foreach (Transform child in conditionContent)
            Destroy(child.gameObject);
        
        foreach (var condition in quest.GetConditions())
        {
            var obj = Instantiate(conditionSlotPrefab, conditionContent);
            obj.GetComponent<QuestConditionUI>().Init(condition, true);
        }
    }

    private void OnClickAccept()
    {
        QuestManager.Instance.AcceptQuest(_quest);
        Destroy(gameObject);
    }
}
