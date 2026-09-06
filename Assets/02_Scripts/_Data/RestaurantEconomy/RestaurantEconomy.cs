using UnityEngine;

[CreateAssetMenu(fileName = "RestaurantEconomy", menuName = "Scriptable Objects/RestaurantEconomy")]
public class RestaurantEconomy : ScriptableObject
{
    public int Money;
    public int Fame;

    public int Debt;
}
