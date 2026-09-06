using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CookingView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image recipeIcon;
    [SerializeField] private Sprite pixelsSprite;
    [SerializeField] private TMP_Text recipeName;
    [SerializeField] private TMP_Text recipeDescription;
    [SerializeField] private CookingIngredientSlotUI[] ingredientSlots;

    [Header("Button")]
    [SerializeField] private Button cookBtn;
    [SerializeField] private Button clostBtn;
    [SerializeField] private Button autoBtn;


    public event Action OnCookClicked;
    public event Action OnCloseClicked;
    public event Action OnAutoClicked;

    private void Awake()
    {
        Debug.Log($"Awake : {recipeName.font.name}");
    }

    private void Start()
    {
        Debug.Log($"Start : {recipeName.font.name}");
    }
    
    public void Init()
    {
        cookBtn.onClick.RemoveAllListeners();  // 혹시 모를 중복 방지
        clostBtn.onClick.RemoveAllListeners();
        autoBtn.onClick.RemoveAllListeners();
        
        cookBtn.onClick.AddListener(() => OnCookClicked?.Invoke());
        clostBtn.onClick.AddListener(() => OnCloseClicked?.Invoke());
        autoBtn.onClick.AddListener(() => OnAutoClicked?.Invoke());
        autoBtn.gameObject.SetActive(false);
    }

    // 레시피가 선택됐을 때 ui에 연결
    // 매개 변수 : 플에이어가 고른 레시피 데이터
    public void UpdateRecipe(Recipe recipe)
    {
        RecipeData data = recipe.Data;

        recipeIcon.sprite = data.icon;
        recipeName.text = data.recipeName;
        recipeDescription.text = data.description;
        
        // 재료 슬롯 빈칸으로 초기화
        foreach (var slot in ingredientSlots)
            slot.ClearSlot();

        // 요리에 필요한 재료 개수만큼 for문으로 슬롯 확장
        for (int i = 0; i <data.ingredients.Count; i++)
        {
            // UI 슬롯 개수보다 재료가 많으면 터짐 방지
            if (i >= ingredientSlots.Length)
                break;

            //순서에 맞춰서 빈칸을 하나씩 켜고 데이터 넘겨줌
            // 슬롯 켜짐, 목표 재료 설정, 필요한 갯수 설정
            ingredientSlots[i].Init(data.ingredients[i].ingredient, data.ingredients[i].amount);
        }
    }

    // 인벤토리에서 재료가 들어왓을 때 숫자를 올리는 함수
    // 매개변수 : 들어오 ㄴ재료의 id, 갯수
    public void UpdateSlotUI(int ingredientId, int currentAmount)
    {
        // 켜져있는 재료 빈칸들을 확인 후 id가 일치하는 칸의 숫자를 올려줌
        foreach (var slot in ingredientSlots) // 재료슬롯전체탐색
        {
            if (!slot.gameObject.activeSelf)
                continue;

            // 현재 켜져 있는 슬롯만 & 이 슬롯이 찾는 재료인지 확인
            if (slot.GetTargetIngredient().id == ingredientId)
            {
                // 현재 보유 수량 반영
                slot.AddIngredient(currentAmount);
                break;
            }
        }
    }

    // 매소드4. 조리 버튼 끄고 켜는 함수
    public void SetCookStartInteractable(bool interactable)
    {
        cookBtn.interactable = interactable;
    }

    public void ClearView()
    {
        recipeIcon.sprite = pixelsSprite;
        recipeName.text = "";
        recipeDescription.text = "";

        foreach(var slot in ingredientSlots)
        {
            if (slot != null)
            {
                slot.gameObject.SetActive(true);
                slot.ClearSlot();
            }
        }

        SetCookStartInteractable(false);
    }

    public void SetAutoButtonInteractable()
    {
        autoBtn.gameObject.SetActive(UpgradeManager.Instance.IsUnlocked(UpgradeType.AutoIngredient));
    }
}
