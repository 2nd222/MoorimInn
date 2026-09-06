using System;
using System.Collections;
using UnityEngine;
using static Constants;

public class ViolenceGuest : GuestBase
{
    [Header("난동 패널티")]
    [SerializeField] private float angryDuration = 2.767f;   // Guest Angry 애니메이션 길이
    [SerializeField] private int maxTotalDamage = 5000;      // 난동 1회당 깎일 수 있는 최대 금액
    [SerializeField] private float badDamageRatio = 0.6f;    // Bad일 때는 최대치의 몇 %까지 깎을지
    [SerializeField] private float damageTickInterval = 0.1f;// 돈이 깎이는 주기 (초)

    [Header("난동 UI")]
    [SerializeField] private Sprite violenceSprite; // 난동 중 말풍선에 띄울 아이콘

    public bool isCausingTrouble = false;
    private Coroutine troubleCoroutine;

    private GuestBubbleUI bubble;
    
    public static event Action<ViolenceGuest> OnViolenceStarted;

    public override void ResetState()
    {
        base.ResetState();

        if (bubble == null)
            bubble = GetComponentInChildren<GuestBubbleUI>(true);

        isCausingTrouble = false;
        if (troubleCoroutine != null)
        {
            StopCoroutine(troubleCoroutine);
            troubleCoroutine = null;
        }
    }

    public override void Init(int id, GuestAppearance appearance, Transform exit, GuestManager manager)
    {
        base.Init(id, appearance, exit, manager);
        OnGuestMoodChanged += HandleViolenceMood;
    }

    private void HandleViolenceMood(GuestBase guest, GuestMood mood)
    {
        // 기분이 Bad 또는 VeryBad가 되면 난동 시작
        if ((mood == GuestMood.Bad || mood == GuestMood.VeryBad) && !isCausingTrouble)
        {
            StartTrouble(mood);
        }
    }

    private void StartTrouble(GuestMood mood)
    {
        isCausingTrouble = true;
    
        if (guestAnim != null)
        {
            guestAnim.PlayAngry(); // 난동 애니메이션
        }
    
        if (bubble != null)
            bubble.SetViolenceBubble(true, violenceSprite);

        InvokeOrderServed();   // 난동 시작하면 주문표에서 제거

        troubleCoroutine = StartCoroutine(TroubleRoutine(mood));
    
        OnViolenceStarted?.Invoke(this);
    }

    /// <summary>
    /// 난동(Angry 애니메이션) 재생 시간 동안만 돈을 깎는다.
    /// 총 피해액은 maxTotalDamage를 넘지 않으며, 시간이 다 되면 스스로 일어나 나간다.
    /// </summary>
    private IEnumerator TroubleRoutine(GuestMood mood)
    {
        int totalLimit = (mood == GuestMood.VeryBad)
            ? maxTotalDamage
            : Mathf.RoundToInt(maxTotalDamage * badDamageRatio);

        float damagePerSecond = totalLimit / angryDuration;

        float elapsed = 0f;
        float tickTimer = 0f;
        int paid = 0;

        while (isCausingTrouble && elapsed < angryDuration)
        {
            yield return null;

            // 일시정지 중에는 시간도, 피해도 진행되지 않는다
            if (DayManager.Instance != null && DayManager.Instance.IsPaused)
                continue;

            elapsed += Time.deltaTime;
            tickTimer += Time.deltaTime;

            if (tickTimer < damageTickInterval && elapsed < angryDuration)
                continue;

            tickTimer = 0f;

            // 지금까지 깎였어야 할 누적 금액에서 이미 깎은 만큼만 추가로 깎는다.
            // 이렇게 하면 반올림 오차가 쌓여도 총액이 totalLimit을 넘지 않는다.
            int target = Mathf.Min(totalLimit, Mathf.RoundToInt(damagePerSecond * elapsed));
            int tickDamage = target - paid;

            if (tickDamage <= 0)
                continue;

            EconomyManager.Instance.AddMoney(-tickDamage);
            paid = target;
        }

        troubleCoroutine = null;

        Debug.Log($"<color=red>괴한의 난동 종료. 총 피해: -{paid}</color>");

        // 제압당하지 않고 시간이 다 됐으면 스스로 일어나서 나간다
        if (isCausingTrouble)
            StopTroubleAndLeave();
    }

    // 도둑의 CatchThief()와 같은 역할
    public void SuppressViolence()
    {
        if (!isCausingTrouble) return;

        Debug.Log("<color=cyan>괴한을 제압했습니다!</color>");

        if (troubleCoroutine != null)
        {
            StopCoroutine(troubleCoroutine);
            troubleCoroutine = null;
        }

        StopTroubleAndLeave();
    }

    /// <summary>
    /// 난동 상태를 끝내고 일어나서 퇴장시킨다. (제압당했을 때 / 난동 시간이 끝났을 때 공통)
    /// </summary>
    private void StopTroubleAndLeave()
    {
        isCausingTrouble = false;

        if (bubble != null)
            bubble.SetViolenceBubble(false);

        isPlayingStandUp = true;

        if (guestAnim != null)
        {
            guestAnim.PlayStandUp(); 
        }
        else
        {
            ChangeState(GuestState.Exit);
        }
    }

    // --- 부모(GuestBase)의 자동 퇴장 타이머 차단 ---
    protected override void HandleWaitOrderState()
    {
        if (isCausingTrouble) return;
        base.HandleWaitOrderState();
    }

    protected override void HandleWaitFoodState()
    {
        if (isCausingTrouble) return;
        base.HandleWaitFoodState();
    }
}