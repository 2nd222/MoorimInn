using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class OkPopup : PopupController
{
    [SerializeField] private TextMeshProUGUI txtMessage;
    [SerializeField] private Button btnOk;
    [SerializeField] private Button btnCancel;
    
    public void Init(string message, bool useCancel)
    {
        this.txtMessage.text = message;
        
        btnOk.gameObject.SetActive(true);
        btnCancel.gameObject.SetActive(useCancel);
    }
    public void ButtonOk(UnityAction action)
    {
        this.btnOk.onClick.AddListener(() =>
        {
            action?.Invoke();
            Destroy(this.gameObject);
        });
    }

    public void ButtonCancel(UnityAction action)
    {
        if (!btnCancel.gameObject.activeSelf)
            return;
        
        this.btnCancel.onClick.AddListener(() =>
        {
            action?.Invoke();
            Destroy(this.gameObject);
        });
    }
}
