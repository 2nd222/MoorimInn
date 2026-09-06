using UnityEngine;
using UnityEngine.UI;

public class DefaultUI : MonoBehaviour
{
    [SerializeField]private Button _questButton;
    [SerializeField]private Button _orderButton;
    [SerializeField]private GameObject _orderPopupObj;

    [SerializeField] private GameObject _questPopupObj;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector3 rect = rectTransform.anchoredPosition3D;
        rect.z = 0f;
        rectTransform.anchoredPosition3D = rect;
        
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);

        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        rectTransform.offsetMin =  Vector2.zero;
        rectTransform.offsetMax =  Vector2.zero;
        
        
    }

    void Start()
    {
        _questButton.onClick.AddListener(() => UIManager.Instance.OpenQuestPopup(_questPopupObj));
        _orderButton.onClick.AddListener(() => UIManager.Instance.OpenOrdersPopup(_orderPopupObj));
    }

    public void OpenQuest()
    {
        Debug.Log("버튼클릭");
    }
}
