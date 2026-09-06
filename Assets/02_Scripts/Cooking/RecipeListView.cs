using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RecipeListView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform content;
    [SerializeField] private GameObject recipeSlotPrefab;

    [Header("Button")]
    [SerializeField] private Button mainDishBtn;
    [SerializeField] private Button sideDishBtn;
    [SerializeField] private Button drinkBtn;
    [SerializeField] private Button dessertBtn;

    public event Action<Recipe> OnRecipeClicked;
    public event Action<Constants.FoodType> OnCategorySelected;

    private List<RecipeSlotUI> spawnedSlots = new List<RecipeSlotUI>(); // 내가 만든 슬롯들 목록 보관함

    public void Init()
    {
        mainDishBtn.onClick.RemoveAllListeners();
        sideDishBtn.onClick.RemoveAllListeners();
        drinkBtn.onClick.RemoveAllListeners();
        dessertBtn.onClick.RemoveAllListeners();
        
        mainDishBtn.onClick.AddListener(() => OnCategorySelected?.Invoke(Constants.FoodType.MainDish));
        sideDishBtn.onClick.AddListener(() => OnCategorySelected?.Invoke(Constants.FoodType.SideDish));
        drinkBtn.onClick.AddListener(() => OnCategorySelected?.Invoke(Constants.FoodType.Drink));
        dessertBtn.onClick.AddListener(() => OnCategorySelected?.Invoke(Constants.FoodType.Dessert));
    }

    // 리스트를 깔아주는 함수
    //List<Recipe> runtimeRecipes -> 현재 사용 가능한 레시피 목록
    public void PopulateList(List<Recipe> runtimeRecipes)
    {
        ClearSpawnedSlots();
        
        // 넘겨받은 레시피 리스트를 고유 id 순서대로 정렬
        var sortedRecipes = runtimeRecipes.OrderBy(r => r.Data.id).ToList();

        // foreach문으로 레시피 하나씩 꺼냄
        // 정렬된 레시피를 하나씩 꺼내서 반복 처리
        foreach (Recipe recipeWrap in sortedRecipes)
        {
            // 레시피가 잠긴 상태라면 넘김
            if (!RecipeUnlockManager.Instance.IsUnlocked(recipeWrap.Data.id))
                continue;
            
            // 버튼 프리팹을 스크롤큐 컨텐츠의 자식으로 1개 생성
            GameObject newPrefab = Instantiate(recipeSlotPrefab, content);
            RecipeSlotUI slotUI = newPrefab.GetComponent<RecipeSlotUI>(); // 방금 만든 오브젝트에서 RecipeSlotUI 스크립트 가져옴 -> 이걸로 데이터를 넣기 위해

            // 슬롯에 레시피 데이터 전달 / 빈 버튼에 실제 레시피 정보 넣기
            slotUI.Init(recipeWrap, (recipe) => OnRecipeClicked?.Invoke(recipe));

            // 나중에 지우기 위해 list에 방금 만든 버튼 추가
            spawnedSlots.Add(slotUI);
        }
    }

    private void ClearSpawnedSlots()
    {
        // 리스트에 있는 기존 리스트 데이터 제거 (중복방지)
        // List<RecipeSlotUI> 안에 있는 슬롯 하나씩 꺼낸 후 실제 게임 오브젝트 삭제 -> 리스트도 비움
        foreach (var slot in spawnedSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        spawnedSlots.Clear();
    }
}
