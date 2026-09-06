using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BondItemData", menuName = "Scriptable Objects/BondItemData")]
public class BondItemData : ItemData
{
    public int bondWithWhom;
    public bool haveThisItem = false;
    public List<StatModifier> modifiers;
    
    [TextArea(5, 20)]
    public string storyText; // 관련된 스토리 내용
}

[Serializable]
public class StatModifier
{
    public BondEffectType type;
    public float value;
}