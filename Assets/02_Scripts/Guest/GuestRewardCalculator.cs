using UnityEngine;
using static Constants;

public static class GuestRewardCalculator
{
    public struct RewardResult
    {
        public int money;
        public int reputation;
    }

    // 보상 데이터를 받아서 계산
    public static RewardResult Calculate(int basePrice, GuestMood mood, GuestRewardData data)
    {
        return new RewardResult
        {
            money = Mathf.RoundToInt(basePrice * data.GetMoneyMultiplier(mood)),
            reputation =  Mathf.RoundToInt(data.GetReputationMultipliter(mood))
        };
    }

    // 패널티는 타입 부관하게 고정값
    public static RewardResult Penalty(int repAmount)
    {
        return new RewardResult
        {
            money = 0,
            reputation = repAmount
        };
    }
}
