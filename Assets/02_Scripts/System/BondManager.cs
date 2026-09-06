using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BondEffectType
{
    FameGainRate,
    MoneyGainRate,
    CookingSpeed,
    CustomerPatience,
    MoveSpeed,
    RichGuestChance,
    BadGuestChance
}

public class BondManager : Singleton<BondManager>, IInitializable
{
    private HashSet<int> ownedBondItems = new();
    private Dictionary<BondEffectType, float> cachedModifiers = new();
    
    private bool isInitialized;
    
    public void Init()
    {
        if (isInitialized)
            return;
        
        cachedModifiers.Clear();
        ownedBondItems.Clear();
        
        isInitialized = true;
    }
    
    public void AcquireBondItem(BondItemData item)
    {
        if (item == null)
            return;

        if (ownedBondItems.Contains(item.id))
            return;

        item.haveThisItem = true;
        RegisterBondItem(item);
        AddToBondInventory(item);
    }
    
    public float GetModifier(BondEffectType type)
    {
        return cachedModifiers.GetValueOrDefault(type, 0f);
    }
    
    public float GetPercentModifier(BondEffectType type)
    {
        return GetModifier(type) / 100f;
    }
    
    private void RegisterBondItem(BondItemData item)
    {
        ownedBondItems.Add(item.id);

        foreach (var modifier in item.modifiers)
        {
            if (!cachedModifiers.ContainsKey(modifier.type))
                cachedModifiers[modifier.type] = 0;
            
            cachedModifiers[modifier.type] += modifier.value;
        }
    }
    
    private void AddToBondInventory(BondItemData item)
    {
        Inventory bondInventory = InventoryManager.Instance.GetInventory(Constants.InventoryType.BondItem);

        bondInventory.GetItem(new Item(item), 1);
    }
    
    public BondSaveData GetSaveData()
    {
        return new BondSaveData
        {
            ownedBondItemIds = ownedBondItems.ToList()
        };
    }
    
    public void LoadFromData(BondSaveData data)
    {
        ownedBondItems.Clear();
        cachedModifiers.Clear();
        
        Inventory bondInventory = InventoryManager.Instance.GetInventory(Constants.InventoryType.BondItem);
        bondInventory.ResetData();

        if (data?.ownedBondItemIds == null)
            return;
        
        foreach (int id in data.ownedBondItemIds)
        {
            var bondItem = DataManager.Instance.GetItemData(id) as BondItemData;

            if (bondItem == null)
                continue;

            AcquireBondItem(bondItem);
        }
    }
    
    public void ResetData()
    {
        ownedBondItems.Clear();
        cachedModifiers.Clear();
        
        InventoryManager.Instance.GetInventory(Constants.InventoryType.BondItem).ResetData();
    }
}
