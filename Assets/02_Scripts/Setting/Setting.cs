using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Setting : MonoBehaviour
{
    [SerializeField] private GameObject objSetting;
    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnExit;

    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    
    private UnityAction onShow;
    private UnityAction onHide;

    void Start()
    {
        EventInit();
    }
    private void EventInit()
    {
        this.btnClose.onClick.AddListener(() => UIManager.Instance.ActiveSetting(false));
        this.btnExit.onClick.AddListener(OnClickExit);
        
        
        saveButton.onClick.AddListener(OnClickSave);
        loadButton.onClick.AddListener(OnClickLoad);
    }
    public void Show()
    {
        this.onShow?.Invoke();
        this.objSetting.SetActive(true);

        bool openSaveLoad = !(GameManager.Instance == null ||
                            GameManager.Instance.CurrentState is GameFlowState.Story or GameFlowState.None);
        
        saveButton.gameObject.SetActive(openSaveLoad);
        loadButton.gameObject.SetActive(openSaveLoad);
    }
    public void Hide()
    {
        this.onHide?.Invoke();
        this.objSetting.SetActive(false);
    }
    
    private void OnClickExit()
    {
        UIManager.Instance.CreateOkPopup(
            "게임을 종료하시겠습니까?",
            onOk: () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            },
            onCancel: () => { },
            useCancel: true
        );
    }
    
    private void OnClickSave()
    {
        UIManager.Instance.ActiveSaveUI(true);
    }

    private void OnClickLoad()
    {
        UIManager.Instance.ActiveLoadUI(true);
    }
}
