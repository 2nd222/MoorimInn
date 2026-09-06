using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 레시피 저장용 리스트 SO
/// </summary>
[CreateAssetMenu(fileName = "RecipeList", menuName = "Scriptable Objects/RecipeList")]
public class RecipeList : ScriptableObject
{
    public List<RecipeData> recipes;
}
