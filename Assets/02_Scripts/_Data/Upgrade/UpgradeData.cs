using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Upgrade/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public int id;
    public string upgradeName;
    
    [TextArea]
    public string description;

    public UpgradeType type;
    public UpgradeAcquireType acquireType = UpgradeAcquireType.Normal;
    
    public List<UpgradeLevelData> levels;
    public int MaxLevel => levels.Count;
}

[Serializable]
public class UpgradeLevelData
{
    public List<UpgradeCost> costs;

    [TextArea]
    public string effectDescription;
    
    public float value;
}

[Serializable]
public class UpgradeCost
{
    public int itemID;
    public int count;
    public int money;
}

public enum UpgradeType
{
    Null,
    AutoIngredient,
    TableCount,
    CharacterSkill,
    CookSpeed,
    CookStationAmount,
    ServingSlotAmount,
    RefridgeSlotAmount,
    ServerSlotAmount,
    Guard,
    
    // 손님 관련
    RichGuestChance,
    BadGuestChance,
}

public enum CharacterSkillLevel
{
    Ghost = 1,
    Teleport = 2,
}

public enum UpgradeAcquireType
{
    Normal,
    Story
}