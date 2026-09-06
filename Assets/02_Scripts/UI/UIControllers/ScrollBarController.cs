using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollBarController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeSpeed = 5f;
    private bool isMouseOver = false;

    void Update()
    {
        float alpha = isMouseOver ? 1f : 0f;
        
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, alpha, Time.deltaTime * fadeSpeed);
        
    }
    
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
    }
}
