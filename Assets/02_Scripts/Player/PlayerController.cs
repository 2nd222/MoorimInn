using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Playables;
using static Constants;

public enum PlayerState
{
    Idle,
    Walk,
    Run,
    Interact,
    PickUp,
    Give
}

public class PlayerController : MonoBehaviour, IControllable
{
    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;
    public event UnityAction<PlayerState> OnStateChanged;

    public IInteractable ActiveInteractable { get; set; }

    private float defaultSpeed;

    public bool IsMoving
    {
        get
        {
            if (agent == null) return false;
            if (agent.pathPending) return true;
            return agent.hasPath && agent.remainingDistance > agent.stoppingDistance;
        }
    }

    [SerializeField] private PlayerType playerType;
    public PlayerType PlayerType => playerType;

    private NavMeshAgent agent;

    private UnityAction onArrived; // 콜백
    
    // 플레이어 속도 배율
    public virtual float SpeedMultiplier => 1.0f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // 초기 설정된 기본 속도
        defaultSpeed = agent.speed;
    }

    void Update()
    {
        CheckArrival();
    }

    public virtual void OnSlotClicked(Slot slot) { }

    private void CheckArrival()
    {
        // 이동 중이 아니면 체크할 필요 없음
        if (agent.pathPending) return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
            {
                if (onArrived != null)
                {
                    UnityAction callback = onArrived;
                    onArrived = null;
                    callback?.Invoke();
                }
                // 정지 애니메이션
                ChangeState(PlayerState.Idle);
            }
        }
    }

    public virtual void MoveTo(Vector3 destination, UnityAction onArrived = null)
    {
        // 갈 수 있는 가장 가까운 위치로 목적지 수정
        if (NavMesh.SamplePosition(destination, out NavMeshHit navHit, 2.0f, NavMesh.AllAreas))
        {
            destination = navHit.position; 
        }
        
        agent.ResetPath();
        agent.velocity = Vector3.zero;
        agent.SetDestination(destination);

        this.onArrived = onArrived;

        // 현재 설정된 기본 속도보다 높으면 Run 애니메이션
        if(agent.speed > defaultSpeed)
            ChangeState(PlayerState.Run);
        else
            ChangeState(PlayerState.Walk);
    }

    public void Stop()
    {
        agent.ResetPath();
        agent.velocity = Vector3.zero;
        this.onArrived = null;

        ChangeState(PlayerState.Idle);
    }

    public void SetSpeed(float move, float angular, float accel)
    {
        this.agent.speed = move;
        this.agent.angularSpeed = angular;
        this.agent.acceleration = accel;
    }
    public void ChangeState(PlayerState state)
    {
        if (this.CurrentState == state) return;

        this.CurrentState = state;

        this.OnStateChanged?.Invoke(this.CurrentState);
    }

    // 선택/해제
    public virtual void OnSelected() {}

    public virtual void OnDeselected() {}
    
    public void PauseMovement()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero; 
        }
    }

    public void ResumeMovement()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
    }
}