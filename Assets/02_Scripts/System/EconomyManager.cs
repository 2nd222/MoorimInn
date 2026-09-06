using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 객잔의 경제 관련 기능을 할 때 사용하는 클래스
/// </summary>
public class EconomyManager : Singleton<EconomyManager>, IInitializable
{
    // 가게의 재정 관련 행동을 할 클래스
    [SerializeField] private RestaurantEconomy restaurantEconomy;
    [SerializeField] private GuestResultTracker guestResultTracker;
    public RestaurantEconomy RestaurantEconomy => restaurantEconomy;
    public  GuestResultTracker GuestResultTracker => guestResultTracker;

    private int _todayMoneyIncome;
    public int TodayMoneyIncome => _todayMoneyIncome;
    private int _todayFameIncome;
    public int TodayFameIncome => _todayFameIncome;
    private int _todayCustomerCount;
    public int TodayCustomerCount => _todayCustomerCount;

    private DailyResultSaveData _lastDailyResult;
    public DailyResultSaveData LastDailyResult => _lastDailyResult;
    
    private ReceiptData lastReceiptData;
    public ReceiptData LastReceiptData => lastReceiptData;
    
    public event Action RestaurantEconomyChanged;
    public event Action TodayEconomyChanged;

    public void Init()
    {
        _todayMoneyIncome = 0;
        _todayFameIncome = 0;
        _todayCustomerCount = 0;
        
        //RegisterGuestResultTracker(guestResultTracker);
        DayManager.Instance.OnDayStart += ResetDay;
        
        DayManager.Instance.OnDayEnd += TotalEarningToday;
        DayManager.Instance.OnDayEnd += TodayResult;

        GuestManager.OnGuestServed += OnCustomerServed;
        
        if (guestResultTracker == null) 
            guestResultTracker = FindFirstObjectByType<GuestResultTracker>(); 
        
        if (guestResultTracker != null) 
            RegisterGuestResultTracker(guestResultTracker); 
        else 
            Debug.LogWarning("GuestResultTracker 없음");
    }
    
    /// <summary>
    /// 주간 영업 중에 금일 수입을 업데이트하기 위한 함수
    /// </summary>
    /// <param name="amount"></param>
    public void AddMoney(int amount, bool applyBonus = true)
    {
        int change = amount;

        if (applyBonus)
        {
            float bonus = BondManager.Instance.GetPercentModifier(BondEffectType.MoneyGainRate);
            change = Mathf.RoundToInt(amount * (1f + bonus));
        }
        
        if (change < 0)
            change = Mathf.Max(change, -RestaurantEconomy.Money);

        if (change == 0)
            return;

        _todayMoneyIncome += change;
        RestaurantEconomy.Money += change;

        TodayEconomyChanged?.Invoke();
        RestaurantEconomyChanged?.Invoke();
    }
    
    /// <summary>
    /// 주간 영업 중에 금일 명성 치 수입을 업데이트하기 위한 함수
    /// </summary>
    /// <param name="amount"></param>
    public void AddFame(int amount, bool applyBonus = true)
    {
        int change = amount;

        if (applyBonus)
        {
            float bonus = BondManager.Instance.GetPercentModifier(BondEffectType.FameGainRate);
            change = Mathf.RoundToInt(amount * (1f + bonus));
        }
        
        if (change < 0)
            change = Mathf.Max(change, -RestaurantEconomy.Fame);

        if (change == 0)
            return;

        _todayFameIncome += change;
        RestaurantEconomy.Fame += change;

        TodayEconomyChanged?.Invoke();
        RestaurantEconomyChanged?.Invoke();
    }
    
    /// <summary>
    /// CustomerManager 액션 받는 장소
    /// </summary>
    void OnCustomerServed()
    {
        _todayCustomerCount++;
        TodayEconomyChanged?.Invoke();
    }
    
    /// <summary>
    /// 영업 시작 시, 금일 영업 이득을 0으로 초기화하기 위한 함수
    /// </summary>
    public void ResetDay()
    {
        _todayMoneyIncome = 0;
        _todayFameIncome = 0;
        _todayCustomerCount = 0;
        TodayEconomyChanged?.Invoke();
    }

    /// <summary>
    /// 하루 영업 종료 후, 가게 수익을 가게 재정 상태에 업데이트 
    /// </summary>
    public void TotalEarningToday()
    {
        RestaurantEconomyChanged?.Invoke();
    }

    /// <summary>
    /// 증가량, 결과 재정, 왔던 손님 수 등등 전체 종합
    /// </summary>
    public void TodayResult()
    {
        SetLastReceiptData();
        Debug.Log($"금일 수입 : {_todayMoneyIncome}, 금일 명성 증가량 : {_todayFameIncome}");
        Debug.Log($"처리한 손님 수 : {_todayCustomerCount}");
        Debug.Log($"결과 재정 상태: {RestaurantEconomy.Money}, 결과 명성 수치 : {RestaurantEconomy.Fame}");
    }
    
    /// <summary>
    /// 돈 사용 가능 여부
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool CanSpend(int amount)
    {
        return restaurantEconomy.Money >= amount;
    }

    /// <summary>
    /// 돈 사용 함수
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool SpendMoney(int amount)
    {
        if (!CanSpend(amount))
            return false;

        restaurantEconomy.Money -= amount;

        RestaurantEconomyChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// 빚 상환 시 호출 함수
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool RepayDebt(int amount)
    {
        if (!CanSpend(amount))
            return false;
        
        restaurantEconomy.Debt -= amount;

        RestaurantEconomyChanged?.Invoke();
        return true;
    }
    
    public void RegisterGuestResultTracker(GuestResultTracker tracker)
    {
        Debug.Log("Tracker 등록됨");
        this.guestResultTracker = tracker;
        
        DayManager.Instance.OnDayEnd -= tracker.EndDay; // 중복 방지
        DayManager.Instance.OnDayEnd += tracker.EndDay;
    }

    public void UnregisterGuestResultTracker(GuestResultTracker tracker)
    {
        if (this.guestResultTracker == tracker)
        {
            DayManager.Instance.OnDayEnd -= tracker.EndDay;
            this.guestResultTracker = null;
        }
    }
    
    
    public EconomySaveData GetSaveData()
    {
        return new EconomySaveData
        {
            money = restaurantEconomy.Money,
            fame = restaurantEconomy.Fame,
            debt = restaurantEconomy.Debt
        };
    }
    
    public void LoadFromData(EconomySaveData data)
    {
        restaurantEconomy.Money = data.money;
        restaurantEconomy.Fame = data.fame;
        restaurantEconomy.Debt = data.debt;

        RestaurantEconomyChanged?.Invoke();
    }
    
    public void LoadDailyResult(DailyResultSaveData data)
    {
        _lastDailyResult = data;
        
        lastReceiptData = new ReceiptData
        {
            week = DayManager.Instance.DayData,

            totalRevenue = data.todayMoneyIncome,
            fame = data.todayFameIncome,
            debt = RestaurantEconomy.Debt,

            happyGuests = data.happyGuests,
            angryGuests = data.angryGuests,

            netIncome = data.todayMoneyIncome
        };
    }
    
    public void ResetData()
    {
        RestaurantEconomy.Money = 0;
        RestaurantEconomy.Fame = 0;
        RestaurantEconomy.Debt = 100000; // 기본값

        _todayMoneyIncome = 0; 
        _todayFameIncome = 0; 
        _todayCustomerCount = 0;

        TodayEconomyChanged?.Invoke();
        Debug.Log("Economy Reset 완료");
    }

    private void SetLastReceiptData()
    {
        int happy = guestResultTracker.DailyResult.goodCount + guestResultTracker.DailyResult.veryGoodCount;

        int angry = guestResultTracker.DailyResult.badCount + guestResultTracker.DailyResult.veryBadCount;

        // 영수증용
        lastReceiptData = new ReceiptData
        {
            week = DayManager.Instance.DayData,
            totalRevenue = TodayMoneyIncome,
            fame = TodayFameIncome,
            debt = RestaurantEconomy.Debt,
            happyGuests = happy,
            angryGuests = angry,
            netIncome = TodayMoneyIncome
        };

        // 세이브용
        _lastDailyResult = new DailyResultSaveData
        {
            todayMoneyIncome = _todayMoneyIncome,
            todayFameIncome = _todayFameIncome,
            todayCustomerCount = _todayCustomerCount,
            happyGuests = happy,
            angryGuests = angry
        };
    }
}
