using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 퀘스트 클리어 창에서, 퀘스트 완료 바에 대한 클래스
/// 퀘스트 실패 시 퀘스트 바 삭제를 하는 함수 추가 필요
/// </summary>
public class QuestClearBar : MonoBehaviour
{
    [SerializeField] private Button clearButton;
    [SerializeField] private Inventory inventory;
    
    [SerializeField] private Transform conditionContent;
    [SerializeField] private GameObject conditionSlotPrefab;
    
    private Quest quest;

    public void Init(Quest quest, Inventory inventory)
    {
        this.quest = quest;
        this.inventory = inventory;

        clearButton.onClick.RemoveAllListeners();
        clearButton.onClick.AddListener(OnClickClear);
        
        foreach (Transform child in conditionContent)
            Destroy(child.gameObject);
        
        foreach (var condition in quest.GetConditions())
        {
            var obj = Instantiate(conditionSlotPrefab, conditionContent);
            obj.GetComponent<QuestConditionUI>().Init(condition, false);
        }
    }

    /// <summary>
    /// 버튼이 클릭됐을 때 퀘스트가 완료 가능한지 확인하고 가능하면 완료 처리 하는 함수
    /// </summary>
    private void OnClickClear()
    {
        if (!quest.CanClear())
            return;
        
        if (!quest.CanRemoveItems(inventory))
        {
            Debug.Log("아이템 부족");
            return;
        }
        quest.CostForQuestClear(inventory);
        
        // 완료 처리
        quest.Complete();
    }
}
