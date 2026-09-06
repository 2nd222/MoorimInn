using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    public int id;
    public string itemName;
    public int price;
    public int maxCapacity;
    public Sprite icon;
    public string description;
}
