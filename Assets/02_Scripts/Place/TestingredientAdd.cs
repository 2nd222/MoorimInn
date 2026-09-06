using UnityEngine;
using UnityEngine.UI;

public class TestingredientAdd : MonoBehaviour
{
    public Button[] buttons;
    [SerializeField] private RecipeData[] targetRecipe;

    [SerializeField] private int amount = 10;

    private void Start()
    {
        int count = Mathf.Min(buttons.Length, targetRecipe.Length);

        if (buttons.Length != targetRecipe.Length)
            Debug.LogWarning($"버튼({buttons.Length})과 레시피({targetRecipe.Length}) 개수가 다름. {count}개만 연결됨.");

        for (int i = 0; i < count; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => OnClickAddProficiency(index));

            // 자식 이미지에 레시피 아이콘 적용
            SetButtonIcon(buttons[i], targetRecipe[i]);
        }
    }

    private void SetButtonIcon(Button button, RecipeData recipe)
    {
        if (recipe == null || recipe.icon == null)
            return;

        Image childImage = null;
        foreach (Transform child in button.transform)
        {
            childImage = child.GetComponent<Image>();
            if (childImage != null)
                break;
        }

        if (childImage != null)
            childImage.sprite = recipe.icon;
        else
            Debug.LogWarning($"{button.name}의 자식에서 Image를 못 찾음");
    }

    private void OnClickAddProficiency(int index)
    {
        RecipeData recipe = targetRecipe[index];

        if (recipe == null)
        {
            Debug.LogError($"{index}번 레시피가 비어있음");
            return;
        }

        DataManager.Instance.AddRecipeProficiency(recipe, amount);

        var mastery = DataManager.Instance.GetRecipeMasteryData(recipe.id);
        Debug.Log($"{recipe.recipeName} 숙련도: {mastery.proficiency}");
    }
}