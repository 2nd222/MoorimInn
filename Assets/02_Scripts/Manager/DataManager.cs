using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static Constants;
using System;
using System.Linq;

[Serializable]
public class RecipeMastery
{
    public RecipeData data; 
    public int proficiency;

    public RecipeMastery(RecipeData data)
    {
        this.data = data;
        this.proficiency = 0;
    }
}
public class DataManager : Singleton<DataManager>
{
    // ��� ������
    private ItemDatabase itemDatabase;

    // ����
    private IngredientList ingredientList;

    // Book
    private AffinityNPCList affinityList;
    private RecipeList recipeList;
    public RecipeList RecipeList => recipeList;

    private Dictionary<IngredientType, List<IngredientData>> dicIngredient = new Dictionary<IngredientType, List<IngredientData>>();
    private Dictionary<IngredientType, List<BondItemData>> dicBuff = new Dictionary<IngredientType, List<BondItemData>>();
    private Dictionary<FoodType, List<RecipeMastery>> dicRecipe = new Dictionary<FoodType, List<RecipeMastery>>();

    // Quest
    private QuestList questList;
    
    protected override void Awake()
    {
        base.Awake();
        DataLoad();
    }
    public void DataLoad()
    {
        ItemDatabaseDataLoad();
        IngredientDataLoad();
        AffinityDataLoad();
        RecipeDataLoad();
        QuestDataLoad();

        RecipeCategorize();
        IngredientCategorize();
    }
    private void ItemDatabaseDataLoad()
    {
        this.itemDatabase = Resources.Load<ItemDatabase>("Data/ItemDatabase");
    }
    private void AffinityDataLoad()
    {
        this.affinityList = Resources.Load<AffinityNPCList>("Data/AffinityNPCList");
    }
    private void IngredientDataLoad()
    {
        this.ingredientList = Resources.Load<IngredientList>("Data/IngredientList");
    }
    private void RecipeDataLoad()
    {
        this.recipeList = Resources.Load<RecipeList>("Data/RecipeList");
    }
    private void QuestDataLoad()
    {
        questList = Resources.Load<QuestList>("QuestPool/WholeQuestPool");   
    }


    // ��� �з�
    private void IngredientCategorize()
    {
        foreach (IngredientType type in System.Enum.GetValues(typeof(IngredientType)))
        {
            this.dicIngredient[type] = new List<IngredientData>();
            dicBuff[type] = new List<BondItemData>();
        }

        foreach (IngredientData data in this.ingredientList.ingredients)
        {
            IngredientType targetType = data.type;

            if (this.dicIngredient.ContainsKey(targetType))
            {
                this.dicIngredient[targetType].Add(data);
            }
        }

        foreach (BondItemData data in ingredientList.bondItems)
        {
            IngredientType targetType = IngredientType.Buff;
            if (this.dicIngredient.ContainsKey(targetType))
                dicBuff[targetType].Add(data);
        }
    }
    // ������ �з�
    private void RecipeCategorize()
    {
        foreach (FoodType type in System.Enum.GetValues(typeof(FoodType)))
        {
            this.dicRecipe[type] = new List<RecipeMastery>();
        }

        foreach (RecipeData data in this.recipeList.recipes)
        {
            FoodType targetType = data.foodType;

            if (this.dicRecipe.ContainsKey(targetType))
            {
                RecipeMastery mastery = new RecipeMastery(data);
                this.dicRecipe[targetType].Add(mastery);
            }
        }
    }
    // ItemDatabase���� �˻�(ItemData ����)
    public ItemData GetItemData(int id)
    {
        return this.itemDatabase.Get(id);
    }
    // ItemDatabase���� �˻�(�̸� ����)
    public string GetItemName(int id)
    {
        return this.itemDatabase.Get(id).itemName;
    }
    // ��� Ÿ�� ����Ʈ ��ȯ
    public List<IngredientData> GetIngredientList(IngredientType type)
    {
        return this.dicIngredient[type];
    }

    public List<BondItemData> GetBondItemList(IngredientType type)
    {
        return this.dicBuff[type];
    }
    
    // id�� �˻��ؼ� ��� ������ ��ȯ
    public IngredientData GetIngredientData(int id)
    {
        IngredientData findData = this.dicIngredient.Values
            .SelectMany(list => list)
            .FirstOrDefault(data => data.id == id);

        return findData;
    }
    public BondItemData GetBondItemData(int id)
    {
        BondItemData findData = dicBuff.Values
            .SelectMany(list => list)
            .FirstOrDefault(data => data.id == id);
        
        return findData;
    }
    
    // id�� �˻��ؼ� ��� ������ �̸� ��ȯ
    public string GetIngredientName(int id)
    {
        return GetIngredientData(id).itemName;
    }
    public string GetBondItemName(int id)
    {
        return GetBondItemData(id).itemName;
    }
    // �ο� ĳ���� ����Ʈ ��ȯ
    public List<AffinityNPCData> GetAffinityNPCList()
    {
        return this.affinityList.listAffinityNPC;
    }
    // ������ Ÿ�� ����Ʈ ��ȯ
    public List<RecipeMastery> GetRecipeList(FoodType type)
    {
        return this.dicRecipe[type];
    }
    // id�� �˻��ؼ� ������ ������ ��ȯ
    public RecipeMastery GetRecipeMasteryData(int id)
    {
        RecipeMastery findData = this.dicRecipe.Values
            .SelectMany(list => list)
            .FirstOrDefault(mastery => mastery.data.id == id);

        return findData;
    }
    // id�� �˻��ؼ� ������ �̸� ��ȯ
    public string GetRecipeName(int id)
    {
        return GetRecipeMasteryData(id).data.recipeName;
    }
    
    public bool HasAnyRecipeProficiency(int value)
    {
        foreach (var list in dicRecipe.Values)
        {
            foreach (var mastery in list)
            {
                if (mastery.proficiency >= value)
                    return true;
            }
        }

        return false;
    }
    
    // �ش� ������ ���õ� ���
    public void AddRecipeProficiency(RecipeData targetRecipe, int amount)
    {
        FoodType targetType = targetRecipe.foodType;

        if (this.dicRecipe.ContainsKey(targetType))
        {
            foreach (RecipeMastery mastery in this.dicRecipe[targetType])
            {
                if (mastery.data == targetRecipe)
                {
                    mastery.proficiency = Mathf.Clamp(mastery.proficiency + amount, 0, 100);

                    return;
                }
            }
        }
    }
    
    public List<RecipeMastery> GetAllRecipeMastery()
    {
        return dicRecipe.Values
            .SelectMany(list => list)
            .ToList();
    }
    
    public QuestData GetQuest(int id)
    {
        return questList.quests.Find(q => q.QuestID == id).Data;
    }
    
    // 음식으로 레시피 숙련도를 찾는 메서드
    public RecipeMastery GetMasteryByFood(FoodData food)
    {
        return dicRecipe.Values
            .SelectMany(list => list)
            .FirstOrDefault(mastery => mastery.data.result == food);
    }
    
    // 숙련도에 따른 음식 품질 변환
    public FoodQuality GetQualityByProficiency(int proficiency)
    {
        if (proficiency >= 80) return FoodQuality.Good;   
        if (proficiency >= 40) return FoodQuality.Fine;   
        return FoodQuality.Normal;                       
    }
}
