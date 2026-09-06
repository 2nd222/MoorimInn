using UnityEngine;
using UnityEngine.UI;

public class DashboardController : MonoBehaviour
{
    [Header("�޴� ��ư")]
    [SerializeField] private Button btnQuests;
    
    [Header("�˾� â")]
    [SerializeField] private RectTransform rectQuests;

    void Awake()
    {
        this.btnQuests.onClick.AddListener(OnClickQuests);
    }

    private void OnClickQuests()
    {
        if (this.rectQuests.gameObject.activeSelf)
        {
            UIAnimationManager.Instance.HidePaper(this.rectQuests, PaperDirection.TopRight,
                onComplete: () => UIManager.Instance.OpenQuestPopup(this.rectQuests.gameObject));
            return;
        }
        
        UIManager.Instance.OpenQuestPopup(this.rectQuests.gameObject);

        UIAnimationManager.Instance.ShowPaper(this.rectQuests, PaperDirection.TopRight);
    }
}
