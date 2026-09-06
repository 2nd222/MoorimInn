using UnityEngine;

/// <summary>
/// 요리, 구매 등으로 아이템을 생성할 필요가 있을 때 호출하는 생성기 클래스
/// </summary>
public class ItemFactory : Singleton<ItemFactory>
{
    [SerializeField] private ItemDatabase itemDatabase;

    public Item CreateItem(int itemID)
    {
        ItemData itemData = itemDatabase.Get(itemID);
        return new Item(itemData);
    }
}
