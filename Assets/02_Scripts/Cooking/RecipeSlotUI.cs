using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeSlotUI : MonoBehaviour
{
    // 필요한 UI 컴포넌트들
    [Header("UI")]
    [SerializeField] private Image recipeIcon;
    [SerializeField] private TMP_Text recipeName;

    [Header("Button")]
    [SerializeField] private Button slotBtn;

    // 기억해야 할 데이터
    private Recipe myRecipe; // 내 주머니에 넣을 레시피 데이터
    private event Action<Recipe> onSlotClicked;

    // 매개변수 : 나에게 부여된 레시피 데이터, 보고할 컨트롤러의 연락처
    public void Init(Recipe recipe, Action<Recipe> onClicked)
    {
        // 매개변수로 전달 받은 데이터를 내 주머니에 저장
        myRecipe = recipe;
        onSlotClicked = onClicked;

        RecipeData data = myRecipe.Data; // 알맹이 꺼내기
        recipeIcon.sprite = data.icon;
        recipeName.text = data.recipeName;

        slotBtn.onClick.RemoveAllListeners();
        slotBtn.onClick.AddListener(OnSlotClicked);
    }

    // 메소드2 : 버튼이 눌렸을 때 실행될 함수
    private void OnSlotClicked()
    {
        onSlotClicked?.Invoke(myRecipe);
    }
}
