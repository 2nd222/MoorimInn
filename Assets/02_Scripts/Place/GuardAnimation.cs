using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class GuardAnimation : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Guard guard;

    private Animator _animator;

    private static readonly int Move = Animator.StringToHash("Move");
    private static readonly int Subdue = Animator.StringToHash("Subdue");

    // Animator Controller의 State 이름과 정확히 일치해야 함
    private const string STATE_SUBDUE = "Guard_Subdue";

    private Coroutine waitCoroutine;

    void Awake()
    {
        guard = GetComponentInParent<Guard>();
        _animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        if (guard != null)
            guard.OnStateChanged += StateChanged;
    }

    void OnDisable()
    {
        if (guard != null)
            guard.OnStateChanged -= StateChanged;

        StopWait();
    }

    private void StateChanged(GuardState state)
    {
        switch (state)
        {
            case GuardState.Idle:
                PlayIdle();
                break;
            case GuardState.Chasing:
            case GuardState.Returning:
                PlayMove();
                break;
            case GuardState.Suppressing:
                PlaySubdue();
                break;
        }
    }

    private void PlayIdle()
    {
        if (_animator != null)
            _animator.SetBool(Move, false);
    }

    private void PlayMove()
    {
        if (_animator != null)
            _animator.SetBool(Move, true);
    }

    private void PlaySubdue()
    {
        if (_animator == null)
            return;

        StopWait();

        _animator.SetBool(Move, false);
        _animator.SetTrigger(Subdue);

        waitCoroutine = StartCoroutine(WaitForAnimation(STATE_SUBDUE, () =>
        {
            guard.OnSubdueFinished();
        }));
    }

    // ── 애니메이션 완료 대기 코루틴 ──

    private IEnumerator WaitForAnimation(string stateName, System.Action onComplete)
    {
        yield return null;

        float safety = 0f;
        while (true)
        {
            if (DayManager.Instance != null && DayManager.Instance.IsPaused)
            {
                yield return null;
                continue;
            }

            var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName(stateName) && stateInfo.normalizedTime < 0.3f)
                break;

            safety += Time.deltaTime;
            if (safety > 2f)
            {
                Debug.LogWarning($"[GuardAnimation] '{stateName}' 상태 진입 실패");
                onComplete?.Invoke();
                waitCoroutine = null;
                yield break;
            }
            yield return null;
        }

        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
        while (info.IsName(stateName) && info.normalizedTime < 0.95f)
        {
            yield return null;

            if (DayManager.Instance != null && DayManager.Instance.IsPaused)
                continue;

            info = _animator.GetCurrentAnimatorStateInfo(0);
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
}