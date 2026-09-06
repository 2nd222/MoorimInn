using System;
using UnityEngine;

public interface IItem
{
    int ID { get;}
    ItemData Data { get;}
    Inventory Inven { get; }
    string ItemName { get; }
    Sprite Icon { get; }
    
    int InventoryCapacity { get; }
    
    float SpoilTimer { get; set; }
    int FridgeSpoilTimer { get; set; }
    Constants.FreshState FreshState { get; set; }
}
