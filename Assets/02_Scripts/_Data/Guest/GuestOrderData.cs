using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable] // 손님이 쥐고 있을 전체 주문서
public class GuestOrderData
{
    public RecipeData recipe; // 고른 메뉴
    public FoodData orderedFood; // 
    public bool isServed; // 음식을 받았는지 여부

    // 메뉴가 채워져 있으면 주문 완료 상태로 봄
    public bool HasOrdered => orderedFood != null;

    public void Clear()
    {
        recipe = null;
        orderedFood = null;
        isServed = false;
    }
}
