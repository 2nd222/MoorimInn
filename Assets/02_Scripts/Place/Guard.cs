using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum GuardState { Idle, Chasing, Suppressing, Returning }

[RequireComponent(typeof(NavMeshAgent))]
public class Guard : MonoBehaviour
{
    [Header("가드 설정")]
    [SerializeField] private float suppressDistance = 1f; // 제압 가능한 도달 거리

    private NavMeshAgent agent;
    private Vector3 idlePosition;
    private ViolenceGuest currentTarget;

    // 아직 제압하지 못한 난동꾼 대기열 (먼저 난동 부린 순서)
    private readonly List<ViolenceGuest> troubleQueue = new List<ViolenceGuest>();
    
    /// 가드 상태가 변경될 때 발행됩니다. GuardAnimation 등 연출 컴포넌트가 구독합니다.
    public event Action<GuardState> OnStateChanged;

    public GuardState CurrentState { get; private set; } = GuardState.Idle;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        idlePosition = transform.position; // 배치된 시작 위치를 기본 대기 장소로 설정
    }

    void OnEnable()
    {
        // 난동 시작 이벤트 구독
        ViolenceGuest.OnViolenceStarted += HandleViolenceStarted;
    }

    void OnDisable()
    {
        // 난동 시작 이벤트 구독 해제
        ViolenceGuest.OnViolenceStarted -= HandleViolenceStarted;
    }

    private void HandleViolenceStarted(ViolenceGuest guest)
    {
        if (guest == null)
            return;

        // 제압 중이든 추격 중이든 일단 대기열에 넣어둔다.
        // (예전엔 Idle/Returning이 아니면 이벤트를 버려서 두 번째 난동꾼이 방치됐음)
        if (!troubleQueue.Contains(guest))
            troubleQueue.Add(guest);

        // 지금 손이 비어 있으면 바로 출동
        if (CurrentState == GuardState.Idle || CurrentState == GuardState.Returning)
            TryTakeNextTarget();
    }

    void Update()
    {
        GuardFSM();
    }

    private void GuardFSM()
    {
        switch (CurrentState)
        {
            case GuardState.Idle:
                // 대기 중에 남은 난동꾼이 있으면 출동 (이벤트를 놓쳤을 때의 안전망)
                if (HasPendingTarget())
                    TryTakeNextTarget();
                break;

            case GuardState.Chasing:
                if (!IsValidTarget(currentTarget))
                {
                    // 놓친 대상은 대기열에서 제거하고 다음 대상으로
                    troubleQueue.Remove(currentTarget);
                    currentTarget = null;

                    if (!TryTakeNextTarget())
                        ReturnToPost();

                    return;
                }

                agent.SetDestination(currentTarget.transform.position);

                if (Vector3.Distance(transform.position, currentTarget.transform.position) <= suppressDistance)
                {
                    StartSuppress();
                }
                break;

            case GuardState.Suppressing:
                // 제압 애니메이션이 끝날 때까지 대기 (OnSubdueFinished 호출)
                break;

            case GuardState.Returning:
                // 돌아가는 중에 새 난동이 터졌으면 즉시 방향 전환
                if (HasPendingTarget())
                {
                    TryTakeNextTarget();
                    return;
                }

                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    SetState(GuardState.Idle);
                }
                break;
        }
    }

    // ── 대상 관리 ──

    private bool IsValidTarget(ViolenceGuest guest)
    {
        if (guest == null)                      return false;   // 파괴됨
        if (!guest.gameObject.activeInHierarchy) return false;   // 풀에 반납됨 (마감 강제 퇴장 등)
        if (!guest.isCausingTrouble)             return false;   // 이미 진정됨

        return true;
    }

    private void PruneQueue()
    {
        for (int i = troubleQueue.Count - 1; i >= 0; i--)
        {
            if (!IsValidTarget(troubleQueue[i]))
                troubleQueue.RemoveAt(i);
        }
    }

    private bool HasPendingTarget()
    {
        PruneQueue();
        return troubleQueue.Count > 0;
    }
    
    /// 대기열에서 다음 난동꾼을 골라 추격을 시작한다. 대상이 없으면 false.
    private bool TryTakeNextTarget()
    {
        PruneQueue();

        if (troubleQueue.Count == 0)
            return false;

        currentTarget = troubleQueue[0];

        agent.isStopped = false;
        SetState(GuardState.Chasing);
        agent.SetDestination(currentTarget.transform.position);

        return true;
    }

    // 제압 시작
    private void StartSuppress()
    {
        // 직전에 플레이어가 먼저 제압했을 수 있으므로 한 번 더 확인
        if (!IsValidTarget(currentTarget))
        {
            troubleQueue.Remove(currentTarget);
            currentTarget = null;

            if (!TryTakeNextTarget())
                ReturnToPost();

            return;
        }

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        currentTarget.SuppressViolence();

        // 제압했으니 대기열에서 제거
        troubleQueue.Remove(currentTarget);

        SetState(GuardState.Suppressing);
    }


    // 제압 애니메이션 재생이 끝났을 때 GuardAnimation이 호출합니다.
    public void OnSubdueFinished()
    {
        if (CurrentState != GuardState.Suppressing)
            return;

        currentTarget = null;
        agent.isStopped = false;

        // 남은 난동꾼이 있으면 제자리로 안 가고 바로 다음 대상으로
        if (TryTakeNextTarget())
            return;

        ReturnToPost();
    }

    private void ReturnToPost()
    {
        currentTarget = null;

        agent.isStopped = false;
        SetState(GuardState.Returning);
        agent.SetDestination(idlePosition);
    }

    // 상태 변경
    private void SetState(GuardState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;

        OnStateChanged?.Invoke(newState);
    }
}