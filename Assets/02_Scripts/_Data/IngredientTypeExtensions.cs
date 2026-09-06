using UnityEngine;
using static Constants;

public static class IngredientTypeExtensions
{
    public static string ToKorean(this IngredientType type)
    {
        switch (type)
        {
            case IngredientType.Vegetable: return "채소";
            case IngredientType.Meats: return "육류";
            case IngredientType.Dairy: return "유제품";
            case IngredientType.Spices: return "향신료";
            case IngredientType.Processed: return "가공품";
            case IngredientType.Buff: return "장식품";
            case IngredientType.Recipe: return "조리비급";
            default: return "에러 발생";
        }
    }
}
