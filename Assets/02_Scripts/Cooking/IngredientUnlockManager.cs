using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IngredientUnlockManager : Singleton<IngredientUnlockManager>, IInitializable
{
    [SerializeField] private IngredientList allIngredients;

    private HashSet<int> unlockedIngredients = new HashSet<int>();
    
    public void Init()
    {
    }

    public bool IsUnlocked(int id)
    {
        return unlockedIngredients.Contains(id);
    }

    public void Unlock(int id)
    {
        if (unlockedIngredients.Contains(id))
            return;

        unlockedIngredients.Add(id);
        Debug.Log($"재료 해금: {id}");
    }
    
    public IngredientSaveData GetSaveData()
    {
        return new IngredientSaveData
        {
            unlockedIngredientIDs = unlockedIngredients.ToList()
        };
    }
    
    public void LoadFromData(IngredientSaveData data)
    {
        unlockedIngredients.Clear();

        foreach (var id in data.unlockedIngredientIDs)
            Unlock(id);
    }
    
    public void ResetData()
    {
        unlockedIngredients.Clear();

        foreach (var ingredient in allIngredients.ingredients)
        {
            if (!ingredient.isLocked)
                Unlock(ingredient.id);
        }

        Debug.Log("Ingredient Reset 완료");
    }
}
