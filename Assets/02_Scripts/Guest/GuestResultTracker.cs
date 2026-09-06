using System;
using UnityEngine;
using static Constants;

public class GuestResultTracker : MonoBehaviour
{
    // 하루 집계
    private GuestResultData dailyResult = new GuestResultData();
    public GuestResultData DailyResult => dailyResult;

    // 누적 집계
    private GuestResultData totalResult = new GuestResultData();

    // 하루 마감 시 외부에 알림
    public event Action<GuestResultData, GuestResultData> OnDayResultReady;

    // 손님 퇴장시 마다 외부에 알림
    public event Action<GuestMood> OnGuestRecorded;

    // private void OnEnable()
    // {
    //     Debug.Log("GuestResultTracker OnEnable 실행됨");
    //     EconomyManager.Instance.RegisterGuestResultTracker(this);
    // }

    private void OnDisable()
    {
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.UnregisterGuestResultTracker(this);
    }
    
    public void RecordGuest(GuestMood mood)
    {
        dailyResult.AddResult(mood);
        totalResult.AddResult(mood);
        OnGuestRecorded?.Invoke(mood);
    }

    public void EndDay()
    {
        Debug.Log("EndDay by GuestTracker");
        OnDayResultReady?.Invoke(dailyResult, totalResult);
        dailyResult.Reset();
    }

    public GuestResultData GetDailyResult() => dailyResult;
    public GuestResultData GetTotalResult() => totalResult;
}
