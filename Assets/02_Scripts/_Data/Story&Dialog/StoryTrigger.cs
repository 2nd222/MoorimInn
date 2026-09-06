using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StoryTrigger
{
    public int storyId;
    
    public int priority;

    [Header("조건")]
    public int requiredDay;
    public List<DayOfWeek> requiredDayOfWeek;

    public int requiredMoney;
    public int requiredFame;

    public int requiredDebt;

    public RecipeCondition recipeCondition;
    
    [Header("Flag 조건")]
    public string requiredFlag;      // 이 플래그 있어야 실행
    public string blockedFlag;       // 이 플래그 있으면 실행 X
    public string addFlagOnComplete; // 완료하면 플래그 갱신
    
    [Header("옵션")]
    public bool playOnce;
    
    [Header("스토리 종료 시 해금")]
    public List<int> unlockRecipeIds;
    public List<int> unlockIngredientIds;
    
    [Header("스토리 종료 시 업그레이드")]
    public List<UpgradeType> upgradeTypes;
}

[Serializable]
public class RecipeCondition
{
    public bool useCondition;

    public int recipeId; // 특정 레시피 (0이면 무시)
    public int requiredProficiency;

    public bool anyRecipe; // 🔥 아무거나 숙련도 필요한 경우 사용하는 조건
}