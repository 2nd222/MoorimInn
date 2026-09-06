using TMPro;
using UnityEngine;

public class QuestConditionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI currentCountText;
    [SerializeField] private TextMeshProUGUI requiredCountText;
    [SerializeField] private GameObject checkIcon; 

    private QuestCondition condition;
    private bool isPreview;
    
    public void Init(QuestCondition condition, bool isPreview)
    {
        this.condition = condition;
        this.isPreview = isPreview;
        
        if (isPreview)
            checkIcon.SetActive(false);

        Refresh();
    }

    private void OnEnable()
    {
        if (condition != null)
            condition.OnConditionUpdated += Refresh;
    }

    private void OnDisable()
    {
        if (condition != null)
            condition.OnConditionUpdated -= Refresh;
    }

    public void Refresh()
    {
        if (condition is ItemCondition itemCondition)
        {
            if (DataManager.Instance.GetItemData(itemCondition.itemID) is not FoodData)
            {
                Debug.Log("Item Condition은 음식데이터인 경우만 유효합니다");
                gameObject.SetActive(false);
            }
        }
        
        descriptionText.text = condition.GetDescription();
        currentCountText.text = condition.GetCurrentCount();
        requiredCountText.text = condition.GetRequiredCount();
        
        descriptionText.color = condition.IsComplete() ? Color.green : Color.black;
        currentCountText.color = condition.IsComplete() ? Color.black : Color.red;
        
        checkIcon.SetActive(condition.IsComplete()); // ✔ 표시
    }
}
