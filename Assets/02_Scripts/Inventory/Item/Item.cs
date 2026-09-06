using UnityEngine;

/// <summary>
/// 인벤토리에 들어가는 아이템을 선언할 때 사용하기 위한 클래스 new Item(~~~) 이런 식으로 호출해서 사용
/// </summary>
public class Item : IItem
{
    public int ID { get; }
    public Inventory Inven { get; }
    public string ItemName { get; }
    public Sprite Icon { get; }
    public int InventoryCapacity { get; }
    
    public ItemData Data { get; private set; }

    public float SpoilTimer { get; set; }
    public int FridgeSpoilTimer { get; set; }
    public Constants.FreshState FreshState { get; set; }
    
    public Item(ItemData data)
    {
        Data = data;

        ID = data.id;
        ItemName = data.itemName;
        InventoryCapacity = data.maxCapacity;
        Icon = data.icon;
        
        SpoilTimer = 0;
        FridgeSpoilTimer = 0;
        FreshState = Constants.FreshState.Good;
    }
    
    /// <summary>
    /// 아이템 여러개로 나눌 때 데이터 복사용
    /// </summary>
    /// <param name="other"></param>
    public Item(IItem other)
    {
        Data = other.Data;

        ID = other.ID;
        ItemName = other.ItemName;
        InventoryCapacity = other.InventoryCapacity;
        Icon = other.Icon;

        FreshState = other.FreshState;
        SpoilTimer = other.SpoilTimer;
        FridgeSpoilTimer = other.FridgeSpoilTimer;
    }
}
