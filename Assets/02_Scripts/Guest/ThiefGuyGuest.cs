using UnityEngine;
using UnityEngine.AI;
using static Constants;

public class ThiefGuyGuest : GuestBase
{
    public bool isCaught = false; 
    public bool isEscape = false; 

    [Header("술래잡기 설정")]
    public float fleeDetectRadius = 100f; 
    private float fleeTimer = 0f;

    [Header("속도 설정")]
    public float normalSpeed = 3.5f;   // 평소 속도 및 검거 후 걸어가는 속도
    public float fleeSpeed = 7.0f;     // 도망칠 때 전력질주 속도
    
    [Header("그리드 도망 좌표 설정")]
    private readonly float[] gridX = { -4f, 2f, 8f, 14f, 20f };
    private readonly float[] gridZ = { -8.8f, -5f, 0f, 5f, 8.8f };

    private NavMeshAgent agent;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
    }
    
    public override void ResetState()
    {
        base.ResetState();
        recipeUnlockManager = RecipeUnlockManager.Instance;
        isCaught = false; 
        isEscape = false;
        
        if (agent != null) agent.speed = normalSpeed;
        if (guestAnim != null) guestAnim.OnCatchedFinished -= OnResumeEscape;
        
        agent.angularSpeed = 120f; 
        agent.acceleration = 8f;     
        agent.autoBraking = true;    
    }
    
    protected override void HandleWaitOrderState() { base.HandleWaitOrderState(); }
    protected override void HandleWaitFoodState() { base.HandleWaitFoodState(); }
    protected override void HandleEatState() { base.HandleEatState(); }

    protected override void HandlePayState()
    {
        if (isPlayingHandDown) return;
        if (!isEscape)
        {
            isEscape = true;
            isPlayingStandUp = true;
            
            guestAnim.PlayStandUp();
        }
    }

    protected override void HandleExitState()
    {
        if (isCaught) return;

        base.HandleExitState(); 

        if (IsAngryExiting) return;

        agent.angularSpeed = 10000f; 
        agent.acceleration = 1000f;  
        agent.autoBraking = false;
        
        fleeTimer -= Time.deltaTime;
        if (fleeTimer > 0f) return;

        // 1. 문이 열려있다면 출구로 전력 질주
        if (Gate.Instance.IsOpen)
        {
            if (exitDoor != null)
            {
                if (agent != null) agent.speed = fleeSpeed;
                mover.MoveTo(exitDoor.position);
            }
            fleeTimer = 0.1f; 
            return;
        }

        // 2. 문이 닫혀있을 때 그리드 기반으로 도망침
        PlayerController chaser = PlayerManager.Instance.SelectedUnit; 
        if (chaser == null) return;

        float distToPlayer = Vector3.Distance(transform.position, chaser.transform.position);

        if (distToPlayer < fleeDetectRadius)
        {
            if (agent != null) agent.speed = fleeSpeed;
            
            // [1] 25개 좌표 중 전체 최적의 탈출 목적지 계산
            Vector3 ultimateBestPos = GetBestGridEscapePoint(chaser.transform.position);
            
            // [2] 대각선 방지를 위해 최종 목적지까지 '직선(상하좌우)'으로 이동할 바로 다음 경유지 계산
            Vector3 nextStraightStep = GetNextStraightStep(transform.position, ultimateBestPos, chaser.transform.position);
            
            mover.MoveTo(nextStraightStep);
            
            fleeTimer = 0.1f; // 반응 속도를 위해 갱신 주기를 조금 더 촘촘하게 설정
        }
        else
        {
            mover.StopAndDisable();
            fleeTimer = 0.5f; 
        }
    }

    // 최종 목적지까지 대각선이 아닌 '축과 평행한 직선(상하좌우)'으로만 움직이게 단계를 쪼개주는 함수
    private Vector3 GetNextStraightStep(Vector3 currentPos, Vector3 ultimateTarget, Vector3 chaserPos)
    {
        // 1. 현재 내 위치와 가장 가까운 그리드 배열 인덱스 찾기
        int curXIdx = 0, curZIdx = 0;
        float minDistX = float.MaxValue, minDistZ = float.MaxValue;
        
        for (int i = 0; i < gridX.Length; i++)
        {
            float d = Mathf.Abs(currentPos.x - gridX[i]);
            if (d < minDistX) { minDistX = d; curXIdx = i; }
        }
        for (int i = 0; i < gridZ.Length; i++)
        {
            float d = Mathf.Abs(currentPos.z - gridZ[i]);
            if (d < minDistZ) { minDistZ = d; curZIdx = i; }
        }

        // 2. 최종 목적지의 그리드 배열 인덱스 찾기
        int tgtXIdx = 0, tgtZIdx = 0;
        float minTgtX = float.MaxValue, minTgtZ = float.MaxValue;
        
        for (int i = 0; i < gridX.Length; i++)
        {
            float d = Mathf.Abs(ultimateTarget.x - gridX[i]);
            if (d < minTgtX) { minTgtX = d; tgtXIdx = i; }
        }
        for (int i = 0; i < gridZ.Length; i++)
        {
            float d = Mathf.Abs(ultimateTarget.z - gridZ[i]);
            if (d < minTgtZ) { minTgtZ = d; tgtZIdx = i; }
        }

        // 이미 인덱스상 최종 목적지에 도달했다면 그대로 최종 목적지 반환
        if (curXIdx == tgtXIdx && curZIdx == tgtZIdx) return ultimateTarget;

        bool canMoveX = (curXIdx != tgtXIdx);
        bool canMoveZ = (curZIdx != tgtZIdx);

        // 수평(X)과 수직(Z) 둘 다 이동해야 하는 경우 (대각선 각도가 나올 때)
        if (canMoveX && canMoveZ)
        {
            int stepX = curXIdx + System.Math.Sign(tgtXIdx - curXIdx);
            int stepZ = curZIdx + System.Math.Sign(tgtZIdx - curZIdx);

            // 가상으로 X축 한 칸 옆으로 간 좌표 vs Z축 한 칸 옆으로 간 좌표 생성
            Vector3 pathX = new Vector3(gridX[stepX], currentPos.y, gridZ[curZIdx]);
            Vector3 pathZ = new Vector3(gridX[curXIdx], currentPos.y, gridZ[stepZ]);

            // 두 경로 중 플레이어와 더 멀어질 수 있는 '직선 축'을 실시간으로 선택 (더 안전한 길)
            float distX = Vector3.Distance(pathX, chaserPos);
            float distZ = Vector3.Distance(pathZ, chaserPos);

            return (distX > distZ) ? pathX : pathZ;
        }
        // X축 정렬만 필요한 경우 (오직 좌우 직선 이동)
        else if (canMoveX)
        {
            int stepX = curXIdx + System.Math.Sign(tgtXIdx - curXIdx);
            return new Vector3(gridX[stepX], currentPos.y, gridZ[curZIdx]);
        }
        // Z축 정렬만 필요한 경우 (오직 상하 직선 이동)
        else if (canMoveZ)
        {
            int stepZ = curZIdx + System.Math.Sign(tgtZIdx - curZIdx);
            return new Vector3(gridX[curXIdx], currentPos.y, gridZ[stepZ]);
        }

        return ultimateTarget;
    }

    // 도둑이 도망갈 최적의 25개 좌표 중 하나를 고르는 함수 (이전 완성본 로직 유지)
    private Vector3 GetBestGridEscapePoint(Vector3 chaserPos)
    {
        Vector3 bestPoint = transform.position;
        float maxScore = -9999f;

        foreach (float x in gridX)
        {
            foreach (float z in gridZ)
            {
                Vector3 targetGrid = new Vector3(x, transform.position.y, z);
                float distFromChaser = Vector3.Distance(targetGrid, chaserPos);
                float clearance = GetMinDistanceToSegment(chaserPos, transform.position, targetGrid);

                // 목적지가 플레이어와 멀고, 도망 동선에 플레이어가 끼어있지 않을수록 고득점
                float score = distFromChaser + (clearance * 2.0f);

                if (score > maxScore)
                {
                    maxScore = score;
                    bestPoint = targetGrid;
                }
            }
        }
        return bestPoint;
    }

    private float GetMinDistanceToSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 p = new Vector3(point.x, 0, point.z);
        Vector3 a = new Vector3(lineStart.x, 0, lineStart.z);
        Vector3 b = new Vector3(lineEnd.x, 0, lineEnd.z);

        Vector3 lineDir = b - a;
        float lineLen = lineDir.magnitude;
        if (lineLen < 0.001f) return Vector3.Distance(p, a);

        Vector3 dirNormalized = lineDir / lineLen;
        Vector3 pointDir = p - a;

        float dot = Vector3.Dot(pointDir, dirNormalized);
        dot = Mathf.Clamp(dot, 0f, lineLen);

        Vector3 closestPoint = a + dirNormalized * dot;
        return Vector3.Distance(p, closestPoint);
    }

    public void CatchThief()
    {
        if (currentState == Constants.GuestState.Exit && !isCaught)
        {
            Debug.Log("도둑 검거 성공 음식값이랑 명성값을 받아냄.");
            isCaught = true;

            int money = 0;
            int fame = 2;

            if (myOrder != null && myOrder.orderedFood != null)
            {
                money = myOrder.orderedFood.price;

                EconomyManager.Instance.AddMoney(money); 
                EconomyManager.Instance.AddFame(fame); 
            }
            else
            {
                fame = 0;   // 주문이 없으면 보상도 없음
            }

            DailyTestLogger.Instance?.RecordThiefCaught(money, fame);
        
            QuestManager.Instance.NotifyListener(new QuestEvent
            {
                type = QuestEventType.CatchThief
            });
        
            mover.StopAndDisable();
            guestAnim.OnCatchedFinished += OnResumeEscape;
            guestAnim.PlayCatched();
        }
    }
    
    private void OnResumeEscape()
    {
        guestAnim.OnCatchedFinished -= OnResumeEscape;
        guestAnim.PlayWalk();

        if (exitDoor != null)
        {
            if (agent != null) agent.speed = normalSpeed;
            mover.EnableAgent(); 
            mover.MoveTo(exitDoor.position);
        }
    }
}