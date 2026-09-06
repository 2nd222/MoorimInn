using TMPro;
using UnityEngine;

public class BriefStoryInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtDescription;

    [SerializeField] private RectTransform popupRect;
    
    void Start()
    {
        HideInfo();
    }

    public void SetInfo(Story story)
    {
        txtName.text = story.storyName;
        txtDescription.text = story.description;
    }

    public void ShowInfo(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        // 오른쪽 중앙
        Vector3 rightCenter = (corners[2] + corners[3]) * 0.5f;

        popupRect.position = rightCenter + Vector3.right * 160f + Vector3.down * 60;

        gameObject.SetActive(true);
    }
    
    public void HideInfo()
    {
        this.gameObject.SetActive(false);
    }
}
