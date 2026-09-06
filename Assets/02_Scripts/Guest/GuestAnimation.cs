using System;
using System.Collections;
using UnityEngine;
using static Constants;

[RequireComponent(typeof(Animator))]
public class GuestAnimation : MonoBehaviour
{
    private Animator anim;
    private GuestBase guest;

    // Parameter 해시
    private static readonly int IS_WALKING  = Animator.StringToHash("IsWalking");
    private static readonly int IS_SITTING  = Animator.StringToHash("IsSitting");
    private static readonly int IS_EATING   = Animator.StringToHash("IsEating");
    private static readonly int IS_ANGRY    = Animator.StringToHash("IsAngry");
    private static readonly int SITDOWN_TRIGGER  = Animator.StringToHash("SitDown");
    private static readonly int STANDUP_TRIGGER  = Animator.StringToHash("StandUp");
    private static readonly int HANDUP_TRIGGER   = Animator.StringToHash("HandUp");
    private static readonly int HANDDOWN_TRIGGER = Animator.StringToHash("HandDown");
    private static readonly int CATCHED_TRIGGER = Animator.StringToHash("Catched");
    

    // ★ Animator State 이름 상수 (Animator Controller의 State 이름과 정확히 일치해야 함)
    private const string STATE_SITDOWN  = "Guest_SitDown";
    private const string STATE_HANDUP   = "Guest_HandUp";
    private const string STATE_HANDDOWN = "Guest_HandDown";
    private const string STATE_STANDUP  = "Guest_StandUp";
    private const string STATE_ANGRY    = "Guest_Angry";
    private const string STATE_CATCHED = "Guest_Catched";

    // 애니메이션 완료 콜백
    public event Action OnSitDownFinished;
    public event Action OnStandUpFinished;
    public event Action OnHandUpFinished;
    public event Action OnHandDownFinished;
    public event Action OnAngryFinished;
    public event Action OnCatchedFinished;

    private Coroutine waitCoroutine;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Bind(GuestBase target)
    {
        guest = target;
        guest.OnGuestStateChanged += StateChanged;
        guest.OnGuestMoodChanged  += MoodChanged;
    }

    private void StateChanged(GuestBase g, GuestState state)
    {
        switch (state)
        {
            case GuestState.Enter:      PlayWalk();     break;
            case GuestState.Waiting:    PlayIdle();     break;
            case GuestState.Exit:       PlayWalk();     break;
        }
    }

    private void MoodChanged(GuestBase g, GuestMood mood)
    {
        // Angry는 GuestBase에서 PlayAngry() 직접 호출
    }

    // ── 외부 호출 메서드 ──

    public void PlayWalk()
    {
        ResetAll();
        anim.SetBool(IS_WALKING, true);
    }

    public void PlayIdle()
    {
        ResetAll();
    }

    public void PlaySitDown()
    {
        StopWait();
        anim.SetBool(IS_WALKING, false);
        anim.SetTrigger(SITDOWN_TRIGGER);
        anim.SetBool(IS_SITTING, true);
        waitCoroutine = StartCoroutine(WaitForAnimation(STATE_SITDOWN, () =>
        {
            OnSitDownFinished?.Invoke();
        }));
    }

    public void PlaySit()
    {
        anim.SetBool(IS_SITTING, true);
    }

    public void PlayHandUp()
    {
        StopWait();
        anim.SetBool(IS_SITTING, true);
        anim.SetTrigger(HANDUP_TRIGGER);
        waitCoroutine = StartCoroutine(WaitForAnimation(STATE_HANDUP, () =>
        {
            anim.SetBool(IS_EATING, true);
            OnHandUpFinished?.Invoke();
        }));
    }

    public void PlayHandDown()
    {
        StopWait();
        anim.SetTrigger(HANDDOWN_TRIGGER);
        anim.SetBool(IS_EATING, false);
        waitCoroutine = StartCoroutine(WaitForAnimation(STATE_HANDDOWN, () =>
        {
            OnHandDownFinished?.Invoke();
        }));
    }

    public void PlayStandUp()
    {
        StopWait();
        anim.SetBool(IS_SITTING, false);
        anim.SetBool(IS_EATING, false);
        anim.SetBool(IS_ANGRY, false);
        anim.ResetTrigger(STANDUP_TRIGGER);
        anim.SetTrigger(STANDUP_TRIGGER);
        waitCoroutine = StartCoroutine(WaitForAnimation(STATE_STANDUP, () =>
        {
            OnStandUpFinished?.Invoke();
        }));
    }

    public void PlayAngry()
    {
        StopWait();
        anim.SetBool(IS_ANGRY, true);
        waitCoroutine = StartCoroutine(WaitForAnimation(STATE_ANGRY, () =>
        {
            OnAngryFinished?.Invoke();
        }));
    }
    
    public void PlayCatched()
    {
        StopWait();
        anim.SetBool(IS_WALKING, false); // 걷기 상태 해제
        anim.SetTrigger(CATCHED_TRIGGER);
    
        waitCoroutine = StartCoroutine(WaitForAnimation(STATE_CATCHED, () =>
        {
            OnCatchedFinished?.Invoke();
        }));
    }

    // ── 애니메이션 완료 대기 코루틴 ──

    private IEnumerator WaitForAnimation(string stateName, Action onComplete)
    {
        yield return null;

        float safety = 0f;
        while (true)
        {
            // ★ 일시정지 중엔 타이머 누적 없이 대기만
            if (DayManager.Instance != null && DayManager.Instance.IsPaused)
            {
                yield return null;
                continue;
            }

            var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName(stateName) && stateInfo.normalizedTime < 0.3f)
                break;
        
            safety += Time.deltaTime;
            if (safety > 2f)
            {
                Debug.LogWarning($"[GuestAnimation] '{stateName}' 상태 진입 실패");
                onComplete?.Invoke();
                waitCoroutine = null;
                yield break;
            }
            yield return null;
        }

        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        while (info.IsName(stateName) && info.normalizedTime < 0.95f)
        {
            yield return null;

            // ★ 일시정지 중엔 진행 체크 안 함
            if (DayManager.Instance != null && DayManager.Instance.IsPaused)
                continue;

            info = anim.GetCurrentAnimatorStateInfo(0);
        }

        onComplete?.Invoke();
        waitCoroutine = null;
    }

    private void StopWait()
    {
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }
    }

    // ── 초기화 ──

    public void ResetAnimation()
    {
        StopWait();
        if (anim == null) anim = GetComponent<Animator>();
        ResetAll();

        OnSitDownFinished  = null;
        OnStandUpFinished  = null;
        OnHandUpFinished   = null;
        OnHandDownFinished = null;
        OnAngryFinished    = null;
        OnCatchedFinished  = null;
    }

    private void ResetAll()
    {
        anim.SetBool(IS_WALKING, false);
        anim.SetBool(IS_SITTING, false);
        anim.SetBool(IS_EATING, false);
        anim.SetBool(IS_ANGRY, false);
        anim.ResetTrigger(SITDOWN_TRIGGER);
        anim.ResetTrigger(STANDUP_TRIGGER);
        anim.ResetTrigger(HANDUP_TRIGGER);
        anim.ResetTrigger(HANDDOWN_TRIGGER);
        anim.ResetTrigger(CATCHED_TRIGGER);
    }
    
    
    public void SetPaused(bool pause)
    {
        if (anim != null)
            anim.speed = pause ? 0f : 1f;
    }
}