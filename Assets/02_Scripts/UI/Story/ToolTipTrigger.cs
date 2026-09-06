using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ToolTipUI toolTipUI;
    [TextArea] public string description;

    public void OnPointerEnter(PointerEventData eventData)
    {
        toolTipUI.Show(description, transform as RectTransform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        toolTipUI.Hide();
    }
}
