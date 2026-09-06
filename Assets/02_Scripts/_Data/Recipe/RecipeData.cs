using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeData", menuName = "Scriptable Objects/RecipeData")]
public class RecipeData : ScriptableObject
{
    public int id;
    public string recipeName;
    public List<IngredientAmount> ingredients;
    public Sprite icon;
    public int price;
    public bool isLocked;
    public Constants.FoodType foodType; // 음식 타입
    public Constants.RecipeClass recipeClass; // 레시피 분류
    public string description;
    public FoodData result;
}
