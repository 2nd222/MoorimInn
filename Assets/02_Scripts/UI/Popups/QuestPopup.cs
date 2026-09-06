using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestPopup : MonoBehaviour
{
    [SerializeField] private Button _closeButton;
    [SerializeField] private GameObject _contentObj;
    [SerializeField] private TextMeshProUGUI _questNameText;
    [SerializeField] private TextMeshProUGUI _questDescriptionText;

    // 처음 보여지는 것이면 "New" 표시 위해
    private bool isShown = false;
    public bool IsShown => isShown;
    
    private Quest quest;
    
    void Awake()
    {
        _closeButton.onClick.AddListener(() =>
        {
            if (_contentObj.activeSelf)
                Close();
            else
                Open();
        });
    }

    void OnEnable()
    {
        isShown = true;
    }

    void OnDisable()
    {
        isShown = false;
        
        if (quest == null) return;

        foreach (var condition in quest.Conditions)
        {
            condition.OnConditionUpdated -= Refresh;
        }
    }

    public void Init(Quest quest)
    {
        this.quest = quest;

        foreach (var condition in quest.Conditions)
        {
            condition.OnConditionUpdated += Refresh;
        }
        Refresh();
        gameObject.SetActive(true);
    }

    private void Close()
    {
        RectTransform pos = _contentObj.GetComponent<RectTransform>();
        UIAnimationManager.Instance.HideVerticalUnfold(pos,null,() =>
        {
            _contentObj.SetActive(false);
        });
    }

    private void Open()
    {
        RectTransform pos = _contentObj.GetComponent<RectTransform>();
        UIAnimationManager.Instance.ShowVerticalUnfold(pos,() =>
        {
            _contentObj.SetActive(true);
        });
    }

    private void Refresh()
    {
        _questNameText.text = quest.QuestName;
        _questDescriptionText.text = "";
        
        foreach (var condition in quest.Conditions)
        {
            _questDescriptionText.text += condition.GetDescription();
            _questDescriptionText.text += $"\n{condition.GetCurrentCount()} / {condition.GetRequiredCount()}\n";
        }
    }
}
