using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RecipeUnlockManager : Singleton<RecipeUnlockManager>, IInitializable
{
    [SerializeField] private RecipeList recipeList;
    private HashSet<int> unlockedRecipes = new HashSet<int>();
    
    
    public void Init()
    {
        
    }

    public bool IsUnlocked(int recipeID)
    {
        return unlockedRecipes.Contains(recipeID);
    }

    public void Unlock(int recipeID)
    {
        if (unlockedRecipes.Contains(recipeID))
            return;
        
        Debug.Log("Unlocking recipe " + recipeID);
        unlockedRecipes.Add(recipeID);
    }

    public RecipeData RandomUnlockedRecipe()
    {
        var list = new List<int>(unlockedRecipes);

        if (list.Count == 0)
            return null;
        
        int randomIndex = UnityEngine.Random.Range(0, list.Count);
        int recipeID = list[randomIndex];

        return DataManager.Instance.GetRecipeMasteryData(recipeID).data;
    }
    
    public RecipeSaveData GetSaveData()
    {
        return new RecipeSaveData
        {
            unlockedRecipeIDs = unlockedRecipes.ToList(),
            proficiencies = DataManager.Instance.GetAllRecipeMastery().Select(m => new RecipeProficiencyData
                {
                    recipeID = m.data.id,
                    proficiency = m.proficiency
                }).ToList()
        };
    }
    
    public void LoadFromData(RecipeSaveData data)
    {
        unlockedRecipes.Clear();

        foreach (var id in data.unlockedRecipeIDs)
            Unlock(id);

        foreach (var p in data.proficiencies)
        {
            var mastery = DataManager.Instance.GetRecipeMasteryData(p.recipeID);
            mastery.proficiency = p.proficiency;
        }
    }
    
    public void ResetData()
    {
        unlockedRecipes.Clear();

        foreach (var recipe in recipeList.recipes)
        {
            if (!recipe.isLocked)
                Unlock(recipe.id);
        }

        // 숙련도 초기화
        foreach (var mastery in DataManager.Instance.GetAllRecipeMastery())
        {
            mastery.proficiency = 0;
        }

        Debug.Log("Recipe Reset 완료");
    }
}
