using UnityEngine;

public class StoryFadePanel : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        StoryManager.Instance.SetFadePanel(this);
    }
}
