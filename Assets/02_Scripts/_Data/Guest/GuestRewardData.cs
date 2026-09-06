using UnityEngine;
using static Constants;

/// <summary>
/// 손님 타입별 보상 배율 데이터.
/// 
/// 사용법:
/// 프로젝트 창에서 우클릭 → Create → Guest → RewardData
/// NormalReward, RichReward 등 타입별로 만들어서
/// 각 손님 프리팹의 GuestBase에 끌어다 넣기.
/// </summary>
[CreateAssetMenu(fileName = "GuestRewardData", menuName = "Scriptable Objects/GuestRewardData")]
public class GuestRewardData : ScriptableObject
{
    [Header("기분 별 재화 배율")]
    public float moneyVeryBad = 0f;
    public float moneyBad = 0.5f;
    public float moneyNeutral = 1.0f;
    public float moneyGood = 1.5f;
    public float moneyVeryGood = 2.0f;

    [Header("기분 별 명성 배율")]
    public float repVeryBad = -1f;
    public float repBad = -0.5f;
    public float repNeutral = 1.0f;
    public float repGood = 1.5f;
    public float repVeryGood = 2.0f;

    public float GetMoneyMultiplier(GuestMood mood)
    {
        switch (mood)
        {
            case GuestMood.VeryBad: return moneyVeryBad;
            case GuestMood.Bad: return moneyBad;
            case GuestMood.Neutral: return moneyNeutral;
            case GuestMood.Good: return moneyGood;
            case GuestMood.VeryGood: return moneyVeryGood;
            default: return moneyNeutral;
        }
    }

    public float GetReputationMultipliter(GuestMood mood)
    {
        switch (mood)
        {
            case GuestMood.VeryBad: return repVeryBad;
            case GuestMood.Bad: return repBad;
            case GuestMood.Neutral: return repNeutral;
            case GuestMood.Good: return repGood;
            case GuestMood.VeryGood: return repVeryGood;
            default: return repNeutral;
        }
    }
}
