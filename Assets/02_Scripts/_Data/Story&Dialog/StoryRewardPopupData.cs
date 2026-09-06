using System.Collections.Generic;

public class StoryRewardPopupData
{
    public int money;
    public int fame;

    public List<string> itemRewardNames = new(); 

    public List<RecipeData> unlockedRecipes = new();
    public List<IngredientData> unlockedIngredients = new();
    public List<UpgradeData> unlockedUpgrades = new();

    public bool HasReward =>
        money > 0 ||
        fame > 0 ||
        itemRewardNames.Count > 0 ||
        unlockedRecipes.Count > 0 ||
        unlockedIngredients.Count > 0 ||
        unlockedUpgrades.Count > 0;
}