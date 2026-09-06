using System;
using TMPro;
using UnityEngine;

/// <summary>
/// 돈이나 명성 바뀔 때마다 UI 업데이트(action이 있다면 action에 추가하는 식으로 변경 가능)
/// </summary>
public class MoneyRepuTextController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI fameText;
    [SerializeField] private RestaurantEconomy restaurantEconomy;
    
    EconomyManager economyManager;
    
    private int currMoney;
    private int currFame;

    void Awake()
    {
        economyManager = EconomyManager.Instance;
        
    }

    private void OnEnable()
    {
        economyManager.RestaurantEconomyChanged += Refresh;
        DayManager.Instance.OnDayStart += Refresh;
    }

    private void OnDisable()
    {
        if (economyManager != null)
        {
            economyManager.RestaurantEconomyChanged -= Refresh;
        }
        
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayStart -= Refresh;
        }
    }

    private void Refresh()
    {
        moneyText.text = economyManager.RestaurantEconomy.Money.ToString();
        fameText.text = economyManager.RestaurantEconomy.Fame.ToString();
    }
}
