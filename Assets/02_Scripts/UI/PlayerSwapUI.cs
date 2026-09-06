using static Constants;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSwapUI : MonoBehaviour
{
    [SerializeField] private Image sowolIcon;
    [SerializeField] private Image myungwolIcon;

    [SerializeField] private GameObject sowolInventoryObj;
    [SerializeField] private GameObject myungwolInventoryObj;

    [SerializeField] private Color activeColor = Color.white; 
    [SerializeField] private Color inactiveColor = new Color(0.3f, 0.3f, 0.3f, 1f); 

    void Start()
    {
        PlayerManager.Instance.OnPlayerSwapped += UpdateIcons;

        UpdateIcons(PlayerManager.Instance.CurrentPlayer);
    }

    void OnDestroy()
    {
        // 스크립트가 파괴될 때 구독 해제 (메모리 누수 방지)
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.OnPlayerSwapped -= UpdateIcons;
    }

    private void UpdateIcons(PlayerType type)
    {
        // 삼항 연산자로 선택된 타입에 따라 컬러 변경
        myungwolIcon.color = (type == PlayerType.Myungwol) ? activeColor : inactiveColor;
        sowolIcon.color = (type == PlayerType.Sowol) ? activeColor : inactiveColor;

        sowolInventoryObj.SetActive(type == PlayerType.Sowol);
        myungwolInventoryObj.SetActive(type == PlayerType.Myungwol);
    }
}
