using UnityEngine;

[CreateAssetMenu(fileName = "FoodData", menuName = "Scriptable Objects/FoodData")]
public class FoodData : ItemData
{
    public bool useRandomItem;
    
    public bool isLocked;
    public Constants.FoodType type;
    public Constants.RecipeClass recipeClass;
    public GameObject foodPrefab;
}
