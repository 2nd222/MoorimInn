using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopProductInfo : MonoBehaviour
{
    [SerializeField]
    private Image imageIcon;
    [SerializeField]
    private TextMeshProUGUI txtName;
    [SerializeField]
    private TextMeshProUGUI txtDescription;


    void Start()
    {
        HideInfo();
    }

    public void ShowInfo(ItemData data)
    {
        this.imageIcon.sprite = data.icon;
        this.txtName.text = data.itemName;
        this.txtDescription.text = data.description;

        this.gameObject.SetActive(true);
    }
    
    public void ShowInfo(RecipeData data)
    {
        this.imageIcon.sprite = data.icon;
        this.txtName.text = data.recipeName;

        string description = $"{data.description}";
        foreach (IngredientAmount ingredient in data.ingredients)
        {
            description += $"\n{ingredient.ingredient.itemName} {ingredient.amount}개";
        }
        this.txtDescription.text = description;

        this.gameObject.SetActive(true);
    }
    
    public void HideInfo()
    {
        this.gameObject.SetActive(false);
    }
}
