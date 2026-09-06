using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StoryProgressSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image checkImage;

    private Story story;
    private BriefStoryInfo info;

    public void Init(Story story, BriefStoryInfo info, bool complete)
    {
        this.story = story;
        this.info = info;
        
        checkImage.gameObject.SetActive(complete);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (story == null)
            return;
        
        info.SetInfo(story);
        info.ShowInfo((RectTransform)transform);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        info.HideInfo();
    }
}
