using UnityEngine;
using UnityEngine.EventSystems;

public class UIBlockPointer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        StoryUI storyUI = StoryManager.Instance.StoryUI;

        if (storyUI == null)
            return;

        storyUI.SetPointerBlock(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StoryUI storyUI = StoryManager.Instance.StoryUI;

        if (storyUI == null)
            return;

        storyUI.StartReleasePointerBlock();
    }
}
