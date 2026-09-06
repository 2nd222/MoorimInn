using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeIngredientSlot : MonoBehaviour
{
    [SerializeField] private Image imgIcon;
    [SerializeField] private TextMeshProUGUI txtAmount;
    public void Init(Sprite icon, int amount)
    {
        this.imgIcon.sprite = icon;
        this.txtAmount.text = amount.ToString();
    }
}
