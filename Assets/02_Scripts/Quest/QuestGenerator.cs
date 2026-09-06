using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using static Constants;

public class QuestGenerator : MonoBehaviour
{
    [SerializeField] private QuestPool questPool;
    [SerializeField] private QuestPool wholeQuestPool;
    
    private RecipeUnlockManager recipeUnlockManager;
    private RecipeUnlockManager RecipeUnlockManager
    {
        get
        {
            if (recipeUnlockManager == null)
                recipeUnlockManager = RecipeUnlockManager.Instance;

            return recipeUnlockManager;
        }
    }
    
    [SerializeField] private RecipeList allRecipes;

    private QuestConditionCategory? lastCategory;
    
    /// <summary>
    /// 해금이 되어있는 아이템을 필요로 하는 퀘스트만 나오게 설정해야함
    /// 퀘스트 풀에 있는 데이터들 중 하나를 랜덤으로 골라서, 리턴
    /// </summary>
    public Quest GenerateRandomQuest(QuestType questType)
    {
        if (questPool.quests.Count == 0)
            return null;
        
        List<QuestData> validTemplates = new List<QuestData>();

        foreach (var template in questPool.quests)
        {
            if (IsValidTemplate(template, questType))
            {
                validTemplates.Add(template);
                Debug.Log(template.name);   
            }
        }

        if (validTemplates.Count == 0)
        {
            Debug.LogWarning("유효한 퀘스트 없음");
            return null;
        }

        List<QuestData> candidates = new(validTemplates);
        if(lastCategory != null)
        {
            List<QuestData> filtered = candidates.FindAll(q => q.category != lastCategory);

            if(filtered.Count > 0)
                candidates = filtered;
        }
        QuestData selected = candidates[Random.Range(0, candidates.Count)];

        lastCategory = selected.category;
        
        var inventory = InventoryManager.Instance.GetInventory(InventoryType.Fridge);
        Quest quest = new Quest(selected, inventory);
        
        if (quest.Data.useDifficultyScaling)
        {
            ApplyDifficulty(quest);
        }
        
        Debug.Log("정해진 퀘스트" + selected.name);
        
        return quest;
    }

    /// <summary>
    /// 데이터로 들어온 퀘스트가 필요로 하는 아이템이 언락됐는지 확인하고 맞을 경우 템플릿에 넣을 수 있게 true를 리턴하는 함수 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="questType">누가 주는 퀘스트인지 구분</param>
    /// <returns></returns>
    private bool IsValidTemplate(QuestData data, QuestType questType)
    {
        if (data == null)
        {
            Debug.Log("QuestData is null");
            return false;
        }

        if (questType != data.questType)
            return false;
        
        foreach (var condition in data.conditions)
        {
            if (condition is ItemCondition itemCondition)
            {
                if (itemCondition.useFoodType)
                {
                    if (!HasRecipeByType(itemCondition.foodType))
                        return false;
                }
                else if (itemCondition.useRandomItem)
                {
                    if (!HasAnyUnlockedRecipe())
                        return false;
                }
                else
                {
                    if (!RecipeUnlockManager.IsUnlocked(itemCondition.itemID))
                        return false;
                }
            }
        }
        return true;
    }
    
    /// <summary>
    /// 타입으로 검색했을 때 만약 언락된 아이템이 하나도 없을 경우 unvalid라고 표시
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private bool HasRecipeByType(Constants.FoodType type)
    {
        foreach (var recipe in allRecipes.recipes)
        {
            if (!RecipeUnlockManager.IsUnlocked(recipe.id))
                continue;

            if (recipe.foodType == type)
                return true;
        }

        return false;
    }
    
    /// <summary>
    /// 만약 무작위 음식 중에 언락된게 아예 없을 경우
    /// </summary>
    /// <returns></returns>
    private bool HasAnyUnlockedRecipe()
    {
        foreach (var recipe in allRecipes.recipes)
        {
            if (RecipeUnlockManager.IsUnlocked(recipe.id))
                return true;
        }

        return false;
    }
    
    /// <summary>
    /// 타입을 기준으로 현재 언락된 레시피들 중에서(퀘스트 완료 가능한 것) 랜덤 음식을 받아오는 함수
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private RecipeData GetRandomRecipeByType(Constants.FoodType type)
    {
        List<RecipeData> list = new List<RecipeData>();

        foreach (var recipe in allRecipes.recipes)
        {
            if (!RecipeUnlockManager.IsUnlocked(recipe.id))
                continue;

            if (recipe.foodType == type)
                list.Add(recipe);
        }

        if (list.Count == 0)
            return null;

        return list[Random.Range(0, list.Count)];
    }
    
    /// <summary>
    /// 무작위 레시피 리턴 함수
    /// </summary>
    /// <returns></returns>
    private RecipeData GetRandomUnlockedRecipe(HashSet<int> usedIds)
    {
        List<RecipeData> list = new();

        foreach (var recipe in allRecipes.recipes)
        {
            if (!RecipeUnlockManager.IsUnlocked(recipe.id))
                continue;

            if (usedIds.Contains(recipe.id))
                continue;

            list.Add(recipe);
        }

        if (list.Count == 0)
            return null;

        return list[Random.Range(0, list.Count)];
    }
    
    /// <summary>
    /// 언락된 레시피 개수 호출 함수
    /// </summary>
    /// <returns></returns>
    private int GetUnlockedRecipeCount()
    {
        int count = 0;

        foreach (var recipe in allRecipes.recipes)
        {
            if (RecipeUnlockManager.IsUnlocked(recipe.id))
                count++;
        }

        return count;
    }
    
    /// <summary>
    /// 아이디 기반 레시피 탐색 함수
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private RecipeData GetRecipeByID(int id)
    {
        foreach (var recipe in allRecipes.recipes)
        {
            if (recipe.id == id)
                return recipe;
        }

        return null;
    }
    
    /// <summary>
    /// 퀘스트에 있는 퀘스트 조건의 퀘스트 조건들의 완료 개수 랜덤 설정
    /// </summary>
    /// <param name="quest"></param>
    public void ApplyDifficulty(Quest quest)
    {
        if (!quest.Data.useDifficultyScaling)
            return;
        
        int tier = GetQuestTier();
        float rewardMultiplier = 1f + (tier - 1);
        
        int unlockedRecipeCount = GetUnlockedRecipeCount();
        int randomFoodTypes = Mathf.Min(tier, unlockedRecipeCount);
        quest.AdjustRandomItemConditions(randomFoodTypes);
        
        HashSet<int> selectedRecipes = new();
        
        foreach (var condition in quest.Conditions)
        {
            if (condition is ItemCondition itemCondition)
            {
                //Debug.Log($"useRandomItem: {itemCondition.useRandomItem}, useFoodType: {itemCondition.useFoodType}, itemID: {itemCondition.itemID}");
                RecipeData recipe = null;
                if (itemCondition.useFoodType)
                {
                    recipe = GetRandomRecipeByType(itemCondition.foodType);
                }
                else if (itemCondition.useRandomItem)
                {
                    recipe = GetRandomUnlockedRecipe(selectedRecipes);
                }
                else
                    recipe = GetRecipeByID(itemCondition.itemID);
                
                if (recipe != null)
                {
                    itemCondition.itemID = recipe.id;
                    selectedRecipes.Add(recipe.id);
                }
                
                itemCondition.requiredCount = Random.Range(1, 1 + (tier/2) + 1); // 1 2 2 3 3 티어별로 이렇게 올라감
                itemCondition.useRandomItem = false;
                itemCondition.useFoodType = false;
                
                if (recipe != null)
                    Debug.Log($"{recipe.recipeName} / require count: {itemCondition.requiredCount}");
                else
                    Debug.LogWarning("레시피 없음");
            }

            if (condition is FailLimitCondition failCondition)
            {
                failCondition.maxFailCount = Mathf.Max(1, Random.Range(2, 6) - (tier - 1));
            }

            if (condition is ServeCondition serveCondition)
            {
                serveCondition.requireCount = Random.Range(tier, tier + 3);
            }

            if (condition is CatchThiefCondition catchCondition)
            {
                catchCondition.requiredCatchCount = Random.Range(1, tier + 1);
            }
        }

        // 보상도 랜덤으로 설정 (보상 하한선이 돈, 명예가 같은 대신 상인은 골드 상한선, 우체부는 명성 상한선이 더 높은 방향으로 보상 설정)
        // 에셋(quest.Data.reward)이 아니라 이 퀘스트의 런타임 보상만 수정한다
        if (quest.QuestType == QuestType.Merchant)
        {
            quest.SetRuntimeReward(
                Mathf.RoundToInt(Random.Range(50, 150) * rewardMultiplier),
                Mathf.RoundToInt(Random.Range(8, 12) * rewardMultiplier));
        } 
        else if (quest.QuestType == QuestType.PostMan)
        {
            quest.SetRuntimeReward(
                Mathf.RoundToInt(Random.Range(50, 100) * rewardMultiplier),
                Mathf.RoundToInt(Random.Range(10, 16) * rewardMultiplier));
        }
    }
    
    /// <summary>
    /// ID로 퀘스트 찾아서 퀘스트 데이터 넣을 틀 만들어서 리턴
    /// </summary>
    /// <param name="questID"></param>
    /// <returns></returns>
    public Quest GenerateQuestByID(int questID)
    {
        var data = wholeQuestPool.quests.Find(q => q.id == questID);

        if (data == null)
        {
            Debug.LogError($"QuestData 없음: {questID}");
            return null;
        }

        var inventory = InventoryManager.Instance.GetInventory(InventoryType.Fridge);

        return new Quest(data, inventory);
    }
    
    /// <summary>
    /// 데이터로 퀘스트 복원할 때 쓰는 함수
    /// </summary>
    /// <param name="runtimeData"></param>
    /// <returns></returns>
    public Quest GenerateQuestFromSave(QuestRuntimeData runtimeData)
    {
        var data = wholeQuestPool.quests.Find(q => q.id == runtimeData.questID);
        if (data == null)
        {
            Debug.LogError($"QuestData 없음 : {runtimeData.questID}");
            return null;
        }
        var inventory = InventoryManager.Instance.GetInventory(InventoryType.Fridge);

        return new Quest(data, inventory, runtimeData);
    }
    
    /// <summary>
    /// 퀘스트 난이도용 함수
    /// </summary>
    /// <returns></returns>
    private int GetQuestTier()
    {
        int fame = EconomyManager.Instance.RestaurantEconomy.Fame;

        if (fame < 50) return 1;
        if (fame < 100) return 2;
        if (fame < 150) return 3;
        if (fame < 200) return 4;
        return 5;
    }
}