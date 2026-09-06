using UnityEngine;
using UnityEngine.UI;

public class DragUI : MonoBehaviour
{
    public static DragUI Instance;

    [SerializeField] private Image dragImage;

    private void Awake()
    {
        Instance = this;
        dragImage.gameObject.SetActive(false);
        dragImage.raycastTarget = false;
    }

    public void StartDrag(Sprite icon)
    {
        dragImage.sprite = icon;
        dragImage.gameObject.SetActive(true);
    }

    public void UpdatePosition(Vector2 position)
    {
        dragImage.transform.position = position;
    }

    public void EndDrag()
    {
        dragImage.sprite = null;
        dragImage.gameObject.SetActive(false);
    }
}
