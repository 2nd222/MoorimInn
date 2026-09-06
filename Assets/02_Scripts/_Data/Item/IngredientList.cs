using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 재료 아이템 & 데이터 체크용 리스트 SO
/// </summary>
[CreateAssetMenu(fileName = "IngredientList", menuName = "Scriptable Objects/IngredientList")]
public class IngredientList : ScriptableObject
{
    public List<IngredientData> ingredients;
    public List<BondItemData> bondItems;
}
