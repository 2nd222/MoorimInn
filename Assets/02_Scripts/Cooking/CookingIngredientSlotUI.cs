using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CookingIngredientSlotUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private GameObject emptyBackground;   // 갈색 원 (비활성 상태)
    [SerializeField] private GameObject activeFrame;       // SetActive_True (금색 프레임 전체)
    [SerializeField] private Image ingredientIcon;         // 재료 아이콘
    [SerializeField] private TMP_Text ingredientCount;     // 수량 텍스트

    // 기억해야 할 데이터
    private IngredientData targetIngredient; // 내가 요구하는 재료 데이터
    private int requiredAmount; // 목표 필요 개수
    private int currentAmount; // 현재 들어온 개수

    // 매개변수: 내가 담당할 재료 데이터, 필요 개수
    public void Init(IngredientData ingredient, int amount)
    {
        targetIngredient = ingredient;
        requiredAmount = amount;
        currentAmount = 0;
 
        // 금색 프레임 활성화, 갈색 원 숨기기
        activeFrame.SetActive(true);
        emptyBackground.SetActive(false);
 
        ingredientIcon.sprite = ingredient.icon;
        ingredientIcon.color = new Color(1, 1, 1, 0.5f);
        ingredientCount.text = $"{currentAmount} / {requiredAmount}";
    }

    // 재료가 들어왔을 때 불리는 함수
    // 매개변수: 들어온 개수
    public void AddIngredient(int amount)
    {
        currentAmount += amount;
        ingredientCount.text = $"{currentAmount} / {requiredAmount}";
 
        if (currentAmount >= requiredAmount)
            ingredientIcon.color = Color.white;
    }

    // 슬롯이 가지고 있는 목표 재료를 외부에서 꺼내는 용도
    public IngredientData GetTargetIngredient() => targetIngredient;

    public void ClearSlot()
    {
        targetIngredient = null;
 
        // 갈색 원 보이기, 금색 프레임 숨기기
        emptyBackground.SetActive(true);
        activeFrame.SetActive(false);
 
        ingredientIcon.sprite = null;
        ingredientIcon.color = new Color(1, 1, 1, 0);
        ingredientCount.text = "";
    }
}
