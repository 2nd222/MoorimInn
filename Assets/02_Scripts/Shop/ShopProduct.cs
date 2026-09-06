using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

public class ShopProduct : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private ItemData itemData;
    private RecipeData recipeData;
    [SerializeField]
    private Image iconBg;
    [SerializeField]
    private Image icon;
    [SerializeField]
    private TextMeshProUGUI itemName;
    [SerializeField]
    private TextMeshProUGUI amount;
    [SerializeField]
    private TextMeshProUGUI price;


    [SerializeField]
    private Image imageBg;
    [SerializeField]
    private Sprite spriteEmpty;
    [SerializeField]
    private Sprite spriteFilled;

    [SerializeField]
    private Button btnBuy;

    public UnityAction<ItemData> onClickBuy;
    public Action<RecipeData> onClickRecipeBuy;
    public Action<ItemData> onHoverEnter;
    public Action<RecipeData> onHoverRecipeEnter;
    
    public Action onHoverExit;
    
    public void Init(ItemData itemData, int owned)
    {
        recipeData = null;
        
        this.itemData = itemData;

        this.icon.sprite = this.itemData.icon;
        this.itemName.text = this.itemData.itemName;
        this.amount.text = $"{owned} / {this.itemData.maxCapacity}";
        this.price.text = this.itemData.price.ToString();

        this.btnBuy.onClick.RemoveAllListeners();

        this.btnBuy.onClick.AddListener(() =>
        {
            this.onClickBuy?.Invoke(this.itemData);
        });

        Background();
    }
    
    public void Init(RecipeData data)
    {
        itemData = null;
        
        recipeData = data;
        itemName.text = data.recipeName;
        icon.sprite = data.icon;
        this.amount.text = "1";
        price.text = data.price.ToString();
        
        this.btnBuy.onClick.RemoveAllListeners();

        this.btnBuy.onClick.AddListener(() =>
        {
            this.onClickRecipeBuy?.Invoke(recipeData);
        });
        
        Background();
    }
    
    public void Clear()
    {
        this.onClickBuy = null;
        this.onClickRecipeBuy = null;
        this.itemData = null;
        this.recipeData = null;
        this.itemName.text = "";
        this.amount.text = "";
        this.price.text = "";
        this.onHoverEnter = null;
        this.onHoverRecipeEnter = null;
        this.onHoverExit = null;

        Background();
    }
    
    public void OnClick()
    {
        if(itemData != null)
        {
            onClickBuy?.Invoke(itemData);
        }
        else if(recipeData != null)
        {
            onClickRecipeBuy?.Invoke(recipeData);
        }
    }
    
    private void Background()
    {
        if (this.itemData != null || recipeData != null)
        {
            this.imageBg.sprite = this.spriteFilled;
            this.iconBg.gameObject.SetActive(true);
            this.icon.gameObject.SetActive(true);
            this.btnBuy.interactable = true;
        }
        else
        {
            this.imageBg.sprite = this.spriteEmpty;
            this.iconBg.gameObject.SetActive(false);
            this.icon.gameObject.SetActive(false);
            this.btnBuy.interactable = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (this.itemData)
            this.onHoverEnter?.Invoke(this.itemData);
        else if (this.recipeData)
            this.onHoverRecipeEnter?.Invoke(this.recipeData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        this.onHoverExit?.Invoke();
    }
}