using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class PopupController : MonoBehaviour
{
    // 팝업 패널의 RectTransform 참조
    [SerializeField] private RectTransform panelTransform;
    public RectTransform PanelTransform => panelTransform;

    public delegate void PanelControllerHideDelegate();

    private CanvasGroup _canvasGroup;

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    // 팝업 표시
    public void Show()
    {
        gameObject.SetActive(true);
        // TODO: Fade in
    }

    // 팝업 숨기기
    public void Hide(PanelControllerHideDelegate onComplete = null)
    {
        onComplete?.Invoke();
        gameObject.SetActive(false);    
        
        // TODO: Fade out
    }
}