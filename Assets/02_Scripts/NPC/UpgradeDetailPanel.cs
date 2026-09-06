using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeDetailPanel : MonoBehaviour
{
    [Header("기본 UI")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI description;

    [Header("업그레이드 내용 UI")] 
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI currentEffectText;
    [SerializeField] private TextMeshProUGUI nextLevelText;
    [SerializeField] private TextMeshProUGUI nextEffectText;

    [Header("비용 UI")]
    [SerializeField] private Transform costContent;
    [SerializeField] private GameObject costPrefab;
    
    [Header("버튼")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI buttonText;
    
    private UpgradeData currentData;

    public void Show(UpgradeData data)
    {
        currentData = data;

        nameText.text = data.upgradeName;
        description.text = data.description;

        RefreshAll();
        
        gameObject.SetActive(true);
    }
    
    private void RefreshAll()
    {
        RefreshEffects();
        RefreshCosts();
        SetupButton();
    }

    
    private void SetupButton()
    {
        upgradeButton.onClick.RemoveAllListeners();

        int lv = UpgradeManager.Instance.GetLevel(currentData.type);

        if (lv >= currentData.MaxLevel)
        {
            buttonText.text = "최대";
            upgradeButton.interactable = false;
            return;
        }

        var nextLevel = currentData.levels[lv];

        if (!CanUpgrade(nextLevel))
        {
            buttonText.text = "개량 불가";
            upgradeButton.interactable = false;
            return;
        }

        buttonText.text = "개량";
        upgradeButton.interactable = true;

        upgradeButton.onClick.AddListener(() =>
        {
            ConsumeCost(nextLevel);

            UpgradeManager.Instance.Upgrade(currentData.type);

            InventoryManager.Instance.ApplyInventoryUpgrade();
            RefreshEffects();
            RefreshCosts();
            SetupButton();
        });
    }

    private void RefreshEffects()
    {
        int lv = UpgradeManager.Instance.GetLevel(currentData.type);

        currentLevelText.text = lv == 0 ? "0 단계" : $"{lv} 단계";
        currentEffectText.text = lv == 0 ? "없음" : currentData.levels[lv-1].effectDescription;
        
        if (lv >= currentData.MaxLevel)
        {
            nextLevelText.text = "최대 단계";
            nextEffectText.text = "최대";
        }
        else
        {
            nextLevelText.text = $"{lv+1} 단계";
            nextEffectText.text = currentData.levels[lv].effectDescription;
        }
    }
    
    private void RefreshCosts()
    {
        foreach (Transform child in costContent)
            Destroy(child.gameObject);

        int lv = UpgradeManager.Instance.GetLevel(currentData.type);

        if (lv >= currentData.MaxLevel)
            return;

        var nextLevel = currentData.levels[lv];

        foreach (var cost in nextLevel.costs)
        {
            var obj = Instantiate(costPrefab, costContent);

            var ui = obj.GetComponent<UpgradeCostUI>();

            ui.Init(cost);
        }
    }
    
    private void ConsumeCost(UpgradeLevelData nextLevel)
    {
        Inventory inventory = InventoryManager.Instance.GetInventory(Constants.InventoryType.Fridge);

        foreach (var cost in nextLevel.costs)
        {
            if (cost.money > 0)
            {
                EconomyManager.Instance.SpendMoney(cost.money);
            }
            else
            {
                inventory.RemoveItem(cost.itemID, cost.count);
            }
        }
    }

    private bool CanUpgrade(UpgradeLevelData nextLevel)
    {
        Inventory inventory = InventoryManager.Instance.GetInventory(Constants.InventoryType.Fridge);

        foreach (var cost in nextLevel.costs)
        {
            if (cost.money > 0)
            {
                if (!EconomyManager.Instance.CanSpend(cost.money))
                    return false;
            }
            else
            {
                if (inventory.GetItemCount(cost.itemID) < cost.count)
                    return false;
            }
        }

        return true;
    }

    public void Clear()
    {
        currentData = null;

        foreach (Transform child in costContent)
            Destroy(child.gameObject);

        gameObject.SetActive(false);
    }
}