using System.Collections.Generic;
using UnityEngine;

public class Order
{
    public int id;
    
    // 주문한 레시피와 개수
    public Dictionary<Recipe, int> recipes;
    

    public Order(Dictionary<Recipe, int> recipes)
    {
        this.recipes = recipes;
    }
}
