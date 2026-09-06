using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Constants;

/// <summary>
/// Customer 생성 및 관리 매니저
/// </summary>
public class GuestManager : MonoBehaviour
{
    public enum SpawnType { All, Normal, Thief, BadGuest, Rich, Violence }
    [Header("테스트용 스폰 설정")]
    public SpawnType currentSpawnType = SpawnType.All;
    
    [Header("연결해야할 외부 정보")]
    [SerializeField] private GuestResultTracker resultTracker;
    [SerializeField] private OrdersPopupController orderPopup;

    [Header("Target Point")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform exitPoint;

    [Header("Spawn Info")]
    private readonly float startSpawnTime = 1f;
    private readonly float spawnInterval = 25f;


    [Header("Watitng Point")]
    [SerializeField] private int maxWaitingCount = 4;
    [SerializeField] private Transform[] waitingPoint;

    [Serializable]
    public struct GuestPrefabEntry
    {
       public GameObject prefab;
       public GuestAppearance appearance;
    }

    [SerializeField] private GuestPrefabEntry[] normalGuestPrefabs;
    public GameObject richGuestPrefab;
    public GameObject thiefGuestPrefab;
    public GameObject badGuestPrefab;
    public GameObject violenceGuestPrefab;

    public static event Action OnGuestServed;
    private int GuestCount = 0;

    // 손님 런타임 정보
    private int globalIDCounter = 0;

    // 현재 소환된 손님들 리스트
    private List<GuestBase> activeGuests = new List<GuestBase>();
    
    // 웨이팅 목록
    private List<GuestBase> waitingGuests = new List<GuestBase>();

    void Awake()
    {
        DayManager.Instance.OnDayStart += StartSpawn;
        DayManager.Instance.OnDayEnd += StopSpawn;

        // 풀 매니저에 프리팹 등록
        foreach (var entry in normalGuestPrefabs)
        {
            PoolManager.Instance.Register(entry.appearance.ToString(), entry.prefab, 3);
        }

        if (richGuestPrefab != null)
            PoolManager.Instance.Register(GuestAppearance.RichMan.ToString(), richGuestPrefab, 1);

        if (thiefGuestPrefab != null)
            PoolManager.Instance.Register(GuestAppearance.ThiefGuy.ToString(), thiefGuestPrefab, 1);
        
        if (badGuestPrefab != null)
            PoolManager.Instance.Register(GuestAppearance.BadGuest.ToString(), badGuestPrefab, 1);

        if (violenceGuestPrefab != null)
            PoolManager.Instance.Register(GuestAppearance.ViolenceGuest.ToString(), violenceGuestPrefab, 1);
    }
    
    void Start()
    {
        if (SeatManager.Instance != null)
        {
            SeatManager.Instance.OnSeatFreed += HandleSeatFreed;
        }
        
        Gate.Instance.OnGateStateChanged += HandleGateStateChanged;
    }

    void OnDisable()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayStart -= StartSpawn;
            DayManager.Instance.OnDayEnd -= StopSpawn;
            
            Debug.Log($"<color=red>DayManager.Instance.OnDayStart -= StartSpawn;");        

        }
        
        if (SeatManager.Instance != null)
        {
            SeatManager.Instance.OnSeatFreed -= HandleSeatFreed;
        }
        
        if (Gate.Instance != null)
            Gate.Instance.OnGateStateChanged -= HandleGateStateChanged;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            EconomyManager.Instance.AddFame(100);
    }

    private void StartSpawn()
    {
        Debug.Log($"<color=green> StartSpawn");        
        InvokeRepeating(nameof(SpawnGuset), startSpawnTime, spawnInterval * 100 / (100 + EconomyManager.Instance.RestaurantEconomy.Fame));
        //Invoke(nameof(SpawnGuset), startSpawnTime);
    }
    
    public GuestBase SpawnGuset()
    {
        if (DayManager.Instance != null && DayManager.Instance.IsPaused)
            return null;
        
        bool canEnterDirectly = Gate.Instance.IsOpen && (SeatManager.Instance.FindEmptyChair() != null);
        
        if (!canEnterDirectly && waitingGuests.Count >= maxWaitingCount)
        {
            Debug.Log("<color=orange>스폰 스킵: 입장 불가 + 대기열 만석</color>");
            return null;
        }
        
        GuestAppearance appearance = GuestAppearance.NormalMaleA;

        if (currentSpawnType != SpawnType.All)
        {
            switch (currentSpawnType)
            {
                case SpawnType.Normal:
                    appearance = GuestAppearance.NormalFemaleA;
                    break;
                case SpawnType.Thief:
                    appearance = GuestAppearance.ThiefGuy;
                    break;
                case SpawnType.BadGuest:
                    appearance = GuestAppearance.BadGuest; 
                    break;
                case SpawnType.Rich:
                    appearance = GuestAppearance.RichMan;
                    break;
                case SpawnType.Violence:
                    appearance = GuestAppearance.ViolenceGuest;
                    break;
            }
        }
        else
        {
            appearance = GetRandomGuestAppearance();
        }

        return InstantiateAndSetupGuest(appearance);
    }

    public GuestBase SpawnGuset(SpawnType spawnType)
    {
        bool canEnterDirectly = Gate.Instance.IsOpen && (SeatManager.Instance.FindEmptyChair() != null);
        
        if (!canEnterDirectly && waitingGuests.Count >= maxWaitingCount) 
            return null;

        GuestAppearance appearance = GuestAppearance.NormalMaleA;

        switch (spawnType)
        {
            case SpawnType.Normal:
                if (normalGuestPrefabs.Length > 0)
                    appearance = normalGuestPrefabs[0].appearance;
                break;
            case SpawnType.Thief:
                appearance = GuestAppearance.ThiefGuy;
                break;
            case SpawnType.BadGuest:
                appearance = GuestAppearance.BadGuest; 
                break;
            case SpawnType.Rich:
                appearance = GuestAppearance.RichMan;
                break;
            case SpawnType.Violence:
                appearance = GuestAppearance.ViolenceGuest;
                break;
            case SpawnType.All:
            default:
                if (normalGuestPrefabs.Length > 0)
                    appearance = normalGuestPrefabs[0].appearance;
                break;
        }

        return InstantiateAndSetupGuest(appearance);
    }

    private GuestBase InstantiateAndSetupGuest(GuestAppearance appearance)
    {
        globalIDCounter++; 
        
        DailyTestLogger.Instance?.RecordGuestSpawned(appearance);

        GameObject guestObj = PoolManager.Instance.Get(appearance.ToString());
        Debug.Log($"풀 Get({appearance}) → {(guestObj != null ? guestObj.name : "<color=red>NULL</color>")}");

        
        if (guestObj != null)
        {
            guestObj.transform.position = spawnPoint.position;
            guestObj.transform.rotation = Quaternion.identity;

            GuestBase guest = guestObj.GetComponent<GuestBase>();

            if (guest != null)
            {
                activeGuests.Add(guest);
                
                guest.ResetState();
                guest.Init(globalIDCounter, appearance, exitPoint, this);

                guest.OnGuestPaid += () =>
                {
                    GuestCount++; 
                    OnGuestServed?.Invoke(); 
                    DailyTestLogger.Instance?.RecordGuestPaid(appearance, guest.CurrentMood); // DailyTestLogger.cs용
                };

                guest.OnGuestExited += () =>
                {
                    resultTracker.RecordGuest(guest.CurrentMood); 
                    activeGuests.Remove(guest);
                };

                guest.OnOrderPlaced += (g, order) => { orderPopup.AddOrder(g, order); };
                guest.OnOrderChanged += (g, order) =>
                {
                    orderPopup.RemoveOrder(g);
                    orderPopup.AddOrder(g, order);
                };
                guest.OnOrderServed += (g) => { orderPopup.RemoveOrder(g); };
                guest.OnGuestAngryExited += (g) => { orderPopup.RemoveOrder(g); };

                return guest; // 세팅 끝난 손님 반환
            }
        }
        
        return null;
    }
    
    private GuestAppearance GetRandomGuestAppearance()
    {
        // 기본 비율
        float goodChance = 5f; // 부자
        float badChance = 30f;  // 도둑 + 악성 손님

        // Upgrade
        goodChance += UpgradeManager.Instance.GetValue(UpgradeType.RichGuestChance);
        badChance -= UpgradeManager.Instance.GetValue(UpgradeType.BadGuestChance);

        // Bond
        goodChance += BondManager.Instance.GetModifier(BondEffectType.RichGuestChance);
        badChance -= BondManager.Instance.GetModifier(BondEffectType.BadGuestChance);

        // 제한
        goodChance = Mathf.Clamp(goodChance, 0, 100);
        badChance = Mathf.Clamp(badChance, 0, 100);

        // 일반 손님 비율
        float normalChance = 100f - goodChance - badChance;


        // 혹시 합이 100 초과하면 비율 조정
        if(normalChance < 0)
        {
            float total = goodChance + badChance;

            goodChance = goodChance / total * 100f;
            badChance = badChance / total * 100f;
            normalChance = 0;
        }
        
        Debug.Log($"{goodChance}, {badChance}");
        float rand = UnityEngine.Random.Range(0f, 100f);

        // 좋은 손님
        if(rand < goodChance)
        {
            return GuestAppearance.RichMan;
        }

        rand -= goodChance;

        // 나쁜 손님
        if(rand < badChance)
        {
            // Bad 내부 종류
            float badRand = UnityEngine.Random.value;


            if(badRand < 0.33f)
                return GuestAppearance.ThiefGuy;

            if(badRand < 0.66f)
                return GuestAppearance.BadGuest;

            return GuestAppearance.ViolenceGuest;
        }


        // 일반 손님
        return normalGuestPrefabs[UnityEngine.Random.Range(0, normalGuestPrefabs.Length)].appearance;
    }
    
    private void HandleSeatFreed(Chair freedChair)
    {
        ProcessWaitingQueue();
    }

    // 문이 열리거나 닫혔을 때
    private void HandleGateStateChanged(bool isOpen)
    {
        Debug.Log($"<color=cyan>GateStateChanged 수신: {isOpen}</color>");

        if (isOpen)
        {
            ProcessWaitingQueue();
        }
    }

    // 대기열 손님들을 조건에 맞춰 빈자리로 들여보내는 핵심 로직
    private void ProcessWaitingQueue()
    {
        // 문이 닫혀있으면 아무도 들여보낼 수 없음
        if (!Gate.Instance.IsOpen) return;

        // 대기 인원이 있고 빈 자리가 있는 동안 반복해서 입장
        while (waitingGuests.Count > 0)
        {
            Chair emptyChair = SeatManager.Instance.FindEmptyChair();
            if (emptyChair == null) break; // 더 이상 빈 자리가 없으면 중단

            // 대기열 첫 번째 손님을 빼서 자리 할당
            GuestBase firstWaitingGuest = waitingGuests[0];
            LeaveWaiting(firstWaitingGuest);
            firstWaitingGuest.AssignChairAndEnter(emptyChair);
        }
    }
    

    public bool JoinWaiting(GuestBase guest, out Vector3 waitPosition)
    {
        waitPosition = Vector3.zero;

        if (waitingGuests.Count >= maxWaitingCount)
            return false;
        
        int pointIndex = waitingGuests.Count;
        waitingGuests.Add(guest);

        if (waitingPoint != null && pointIndex < waitingPoint.Length)
            waitPosition = waitingPoint[pointIndex].position;
        else
            waitPosition = spawnPoint.position;

        return true;
    }

    public void LeaveWaiting(GuestBase guest)
    {
        if (waitingGuests.Remove(guest))
            RearrangeWaitingSpots();
    }

    private void RearrangeWaitingSpots()
    {
        for (int i = 0; i < waitingGuests.Count; i++)
        {
            if (waitingPoint != null && i < waitingPoint.Length)
            {
                GuestMover mover = waitingGuests[i].GetComponent<GuestMover>();

                if (mover != null)
                    mover.MoveTo(waitingPoint[i].position);
                else
                    waitingGuests[i].transform.position = waitingPoint[i].position;
            }
        }
    }

    public void StopSpawn()
    {
        CancelInvoke();
        ForceAllGuestsExit();
        SeatManager.Instance.EmptyAllChairs();
    }

    private void ForceAllGuestsExit()
    {
        GuestBase[] allGuests = FindObjectsByType<GuestBase>(FindObjectsSortMode.None);

        foreach (var guest in allGuests)
        {
            if (guest == null) continue;
            guest.ForceExit();
        }

        activeGuests.Clear();
        waitingGuests.Clear();
    }
}


