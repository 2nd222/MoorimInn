using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Constants;

/// <summary>
/// 상인과 인벤토리, 객잔의 economymanager를 연결해서 아이템 구매하는데 사용할 클래스
/// </summary>
public class Shop : MonoBehaviour
{
    [SerializeField] private ItemFactory itemFactory;
    [SerializeField] private Inventory playerMeatInventory;
    [SerializeField] private Inventory playerOtherIngredientInventory;
    [SerializeField] private Inventory fridgeInventory;

    [SerializeField] private GameObject objCategory;
    [SerializeField] private Transform tsCategory;
    [SerializeField] private GameObject objProduct;
    [SerializeField] private Transform tsProduct;
    [SerializeField] private Button btnClose;

    [SerializeField] private TextMeshProUGUI txtMoney;
    [SerializeField] private TextMeshProUGUI txtFame;

    // 카테고리
    private Dictionary<IngredientType, ShopCategory> dicCategory = new Dictionary<IngredientType, ShopCategory>();
    [SerializeField]
    private ToggleGroup categoryGroup;

    // 상점 요소
    private List<GameObject> listProduct = new List<GameObject>();

    public Action<ItemData> OnProductClicked;
    public Action<RecipeData> OnRecipeClicked;

    [SerializeField]
    private ShopProductInfo infoPanel;
    
    private IngredientType currentCategory;
    private bool hasCategory = false;

    private ItemFactory ItemFactory
    {
        get
        {
            if (itemFactory == null)
                itemFactory = ItemFactory.Instance;

            return itemFactory;
        }
    }
    
    public void Init()
    {
        playerMeatInventory = InventoryManager.Instance.GetInventory(InventoryType.IngredientsButMeatOnly);
        playerOtherIngredientInventory = InventoryManager.Instance.GetInventory(InventoryType.Ingredients);
        fridgeInventory = InventoryManager.Instance.GetInventory(InventoryType.Fridge);
        CreateCategory();
    }

    public bool Buy(int itemID, int count)
    {
        if (ItemFactory == null)
        {
            Debug.LogError("itemFactory is NULL");
            return false;
        }

        if (EconomyManager.Instance == null)
        {
            Debug.LogError("EconomyManager is NULL");
            return false;
        }
    
        Debug.Log("Buying " + itemID);
        Item item = ItemFactory.CreateItem(itemID);

        if (item == null)
        {
            Debug.LogError("item is NULL");
            return false;
        }
    
        int totalPrice = item.Data.price * count;
    
        if (!EconomyManager.Instance.CanSpend(totalPrice))
        {
            Debug.Log("돈 부족");
            return false;
        }
    
        Inventory inventory = GetTargetInventory(item.Data);
    
        if (inventory != null)
        {
            // 보유 한도 검사: 현재 보유량 + 구매량이 최대 보유량을 넘으면 실패
            int owned = inventory.GetItemCount(item.ID);
            if (owned + count > item.Data.maxCapacity)
            {
                UIManager.Instance.CreateOkPopup($"최대 {item.Data.maxCapacity}개까지만 보유할 수 있습니다.", () => {}, () => {}, false);
                return false;
            }

            int canBuy = inventory.CanReceiveItem(item, count);
        
            if (canBuy == 0)
            {
                UIManager.Instance.CreateOkPopup("인벤토리가 가득 찼습니다.", () => {}, () => {}, false);
                return false;
            }
        
            if (canBuy < count)
            {
                UIManager.Instance.CreateOkPopup($"구매 실패!\n{canBuy}개만 구매 가능합니다.", () => {}, () => {}, false);
                return false;
            }
        
            inventory.GetItem(item, count);
        }
        else if (item.Data is BondItemData bondItemData)
        {
            Debug.Log($"{bondItemData.name} is a bondItem");
            BondManager.Instance.AcquireBondItem(bondItemData);
        }
        else
        {
            Debug.Log("Not a valid item");
        }
    
        EconomyManager.Instance.SpendMoney(totalPrice);

        // ★ 구매 성공 → 현재 카테고리 목록 다시 그려서 보유량(20/99 → 60/99) 갱신
        if (hasCategory)
            UpdateProductList(currentCategory);

        return true;
    }
    
    // 카테고리 생성
    public void CreateCategory()
    {
        int length = System.Enum.GetValues(typeof(IngredientType)).Length;
        for (int i = 0; i < length; i++)
        {
            var type = (IngredientType)i;
            string s = (type).ToKorean().ToString();

            ShopCategory category = Instantiate(this.objCategory, this.tsCategory).GetComponent<ShopCategory>();
            category.Init(s);
            category.SetToggleGloup(this.categoryGroup);

            category.onToggleEvent = (bool isOn) =>
            {
                if (!isOn)
                    return;

                UpdateProductList(type);
            };

            this.dicCategory.Add(type, category);
        }
    }
    // 상점 요소 생성
    public void CreateProduct()
    {
        for (int i = 0; i < 30; i++)
        {
            GameObject slot = Instantiate(this.objProduct, this.tsProduct);

            slot.SetActive(false);

            this.listProduct.Add(slot);
        }
    }

    // 카테고리 클릭시 카테고리에 해당되는 상점 요소로 바꾸기
    public void UpdateProductList(IngredientType type)
    {
        // 현재 카테고리 기억 (구매 후 갱신에 사용)
        currentCategory = type;
        hasCategory = true;

        switch(type)
        {
            case IngredientType.Recipe:
                UpdateRecipeProductList();
                break;

            case IngredientType.Buff:
                UpdateBuffProductList();
                break;

            default:
                UpdateIngredientProductList(type);
                break;
        }
    }
    
    private void UpdateIngredientProductList(IngredientType type)
    {
        List<ItemData> targetDataList = DataManager.Instance.GetIngredientList(type).Cast<ItemData>().ToList();

        RefreshProducts(targetDataList);
    }
    
    private void UpdateBuffProductList()
    {
        List<ItemData> targetDataList = DataManager.Instance.GetBondItemList(IngredientType.Buff).Where(x => !x.haveThisItem).Cast<ItemData>().ToList();

        RefreshProducts(targetDataList);
    }
    
    private void UpdateRecipeProductList()
    {
        List<RecipeData> recipes =
            DataManager.Instance.RecipeList.recipes
                .Where(x => !RecipeUnlockManager.Instance.IsUnlocked(x.id))
                .ToList();

        int minSlotCount = 3;
        int displayCount = Mathf.Max(recipes.Count, minSlotCount);

        for (int i = 0; i < displayCount; i++)
        {
            if (i >= listProduct.Count)
            {
                GameObject newGo = Instantiate(objProduct, tsProduct);
                listProduct.Add(newGo);
            }

            listProduct[i].SetActive(true);
            ShopProduct product = listProduct[i].GetComponent<ShopProduct>();

            if (i < recipes.Count)
            {
                product.Init(recipes[i]);

                product.onClickRecipeBuy = null;
                product.onClickRecipeBuy += (recipe) =>
                {
                    OnRecipeClicked?.Invoke(recipe);
                };
                
                product.onHoverRecipeEnter = (recipe) => {
                    this.infoPanel.ShowInfo(recipe);
                };
                product.onHoverExit = () => {
                    this.infoPanel.HideInfo();
                };
            }
            else
            {
                product.Clear();
                product.onClickRecipeBuy = null;
                product.onHoverRecipeEnter  = null;
            }
        }

        for (int i = displayCount; i < listProduct.Count; i++)
        {
            listProduct[i].SetActive(false);
        }
    }
    
    private void RefreshProducts(List<ItemData> targetDataList)
    {
        int minSlotCount = 3; // 3개의 슬롯 유지
        int displayCount = Mathf.Max(targetDataList.Count, minSlotCount);

        for (int i = 0; i < displayCount; i++)
        {
            if (i >= this.listProduct.Count)
            {
                GameObject newGo = Instantiate(this.objProduct, this.tsProduct);
                this.listProduct.Add(newGo);
            }

            this.listProduct[i].SetActive(true);

            ShopProduct product = this.listProduct[i].GetComponent<ShopProduct>();

            if (i < targetDataList.Count)
            {
                product.Init(targetDataList[i], GetOwnedCount(targetDataList[i]));

                product.onClickBuy = null;
                product.onClickBuy += (itemData) =>
                {
                    OnProductClicked?.Invoke(itemData);
                };

                product.onHoverEnter = (itemData) => {
                    this.infoPanel.ShowInfo(itemData);
                };
                product.onHoverExit = () => {
                    this.infoPanel.HideInfo();
                };
            }
            else
            {
                product.Clear(); 
                product.onClickBuy = null; 
                product.onHoverEnter = null;
            }
        }

        for (int i = displayCount; i < this.listProduct.Count; i++)
        {
            this.listProduct[i].SetActive(false);
        }
    }
    
    private void OnEnable()
    {
        EconomyManager.Instance.RestaurantEconomyChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.RestaurantEconomyChanged -= Refresh;
        }
    }

    private void Refresh()
    {
        this.txtMoney.text = EconomyManager.Instance.RestaurantEconomy.Money.ToString();
        this.txtFame.text = EconomyManager.Instance.RestaurantEconomy.Fame.ToString();
    }
    
    // 아이템이 들어갈 인벤토리 결정 (Buy와 동일)
    private Inventory GetTargetInventory(ItemData itemData)
    {
        if (itemData is IngredientData ingredientData)
            return ingredientData.type == IngredientType.Meats ? playerMeatInventory : playerOtherIngredientInventory;

        if (itemData is FoodData)
            return fridgeInventory;

        return null; 
    }

    // 현재 보유 개수 (팝업 표시용)
    public int GetOwnedCount(ItemData itemData)
    {
        Inventory inventory = GetTargetInventory(itemData);
        return inventory != null ? inventory.GetItemCount(itemData.id) : 0;
    }

    // 추가 구매 가능 개수 = 최대 보유량 - 현재 보유량
    public int GetMaxBuyable(ItemData itemData)
    {
        Inventory inventory = GetTargetInventory(itemData);
        if (inventory == null)
            return 1; // 인연 아이템 등은 1개 구매

        int owned = inventory.GetItemCount(itemData.id);
        return Mathf.Max(0, itemData.maxCapacity - owned);
    }
}
