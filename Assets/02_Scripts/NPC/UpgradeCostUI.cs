using TMPro;
using UnityEngine;

public class UpgradeCostUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI currentCountText;
    [SerializeField] private TextMeshProUGUI costText;

    public void Init(UpgradeCost cost)
    {
        bool enough;

        if (cost.money > 0)
        {
            itemNameText.text = "돈";
            int currentMoney = EconomyManager.Instance.RestaurantEconomy.Money;
            currentCountText.text = currentMoney.ToString();
            costText.text = cost.money.ToString();
            enough = currentMoney >= cost.money;
        }
        else
        {
            ItemData itemData = DataManager.Instance.GetItemData(cost.itemID);
            itemNameText.text = itemData.itemName;
            int currentCount = InventoryManager.Instance.GetInventory(Constants.InventoryType.Fridge).GetItemCount(cost.itemID);
            currentCountText.text = currentCount.ToString();
            costText.text = cost.count.ToString();
            enough = currentCount >= cost.count;
        }
        currentCountText.color = enough ? Color.green : Color.red;
    }
}
