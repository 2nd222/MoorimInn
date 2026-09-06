using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderIconController : PopupController
{
    private Button _button;
    public void Showt()
    {
        base.Show();
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnOrderIconClicked);
    }

    private void OnOrderIconClicked()
    {
        // TODO: 이 캔버스가 속한 게임오브젝트의 GuestBase컴포넌트 찾아서 그 안의 게스트 정보 가져오기
        GameObject guestObj = transform.parent.gameObject;
        // GuestBase guestBase = guestObj.GetComponent<GuestBase>();
        // string characterName = 
        UIManager.Instance.OpenDialogPopup("examplename", "exampleText");
    }
}
