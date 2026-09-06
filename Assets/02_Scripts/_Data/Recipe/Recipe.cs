using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  레시피에 필요한 재료, 양을 묶은 데이터 클래스
/// </summary>
[Serializable]
public class IngredientAmount
{
    public IngredientData ingredient;
    public int amount;
}


/// <summary>
/// 레시피 데이터를 받아 조리에 사용할 데이터 클래스
/// </summary>
[Serializable]
public class Recipe
{
    public RecipeData Data { get; private set; }
    public bool IsCookCompleted { get; private set; }

    public Recipe(RecipeData data)
    {
        Data = data;
    }

    public void CompleteCook()
    {
        IsCookCompleted = true;
    }
}

// ordering, serving 기능을 위해서 GameObject로 생성하는 아이템을 만들기는 해야할 듯