using System.Collections.Generic;
using UnityEngine;


/// <summary>
///  인벤토리에 들어갈 모든 아이템 (재료, 요리, 인연 아이템)들을 모두 담고 있을 데이터베이스 SO 아이템ID를 키값으로 아이템 데이터를 받는다.
/// </summary>
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Scriptable Objects/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> items;
    private Dictionary<int, ItemData> itemMap;

    public void Init()
    {
        itemMap = new Dictionary<int, ItemData>();

        foreach (var item in items)
        {
            itemMap[item.id] = item;
        }
    }
    
    public ItemData Get(int id)
    {
        if (itemMap == null)
            Init();

        if (itemMap != null) return itemMap[id];
        
        return null;
    }
}
