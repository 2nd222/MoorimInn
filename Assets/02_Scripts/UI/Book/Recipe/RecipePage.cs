using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Constants;

public class RecipePage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeName;
    [SerializeField] private Image imgIcon;
    [SerializeField] private Transform ingredientParent;
    [SerializeField] private Image imgProgress;
    [SerializeField] private TextMeshProUGUI txtExplain;

    [SerializeField] private GameObject ingredientSlotPrefab;

    public void UpdatePage(RecipeMastery mastery)
    {
        RecipeData data = mastery.data;
        this.gameObject.SetActive(true);

        this.recipeName.text = data.recipeName;
        this.imgIcon.sprite = data.icon;
        
        // ���� ����
        foreach (Transform child in this.ingredientParent)
        {
            Destroy(child.gameObject);
        }
        
        for (int i = 0; i < data.ingredients.Count; i++)
        {
            RecipeIngredientSlot slot = Instantiate(this.ingredientSlotPrefab,
                                                        this.ingredientParent).GetComponent<RecipeIngredientSlot>();
            slot.Init(data.ingredients[i].ingredient.icon, data.ingredients[i].amount);
        }

        // ���õ�
        this.imgProgress.fillAmount = GetCustomFillAmount(mastery.proficiency);

        // ����
        this.txtExplain.text = mastery.data.description;
    }
    public void HidePage()
    {
        this.gameObject.SetActive(false);
    }
    private float GetCustomFillAmount(int proficiency)
    {
        if (proficiency <= 50)
            return Mathf.Lerp(0f, 0.52f, proficiency / 50f);
        else
            return Mathf.Lerp(0.52f, 1f, (proficiency - 50f) / 50f);
    }
}
