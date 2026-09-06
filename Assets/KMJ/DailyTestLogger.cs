using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using static Constants;

#region 저장용 데이터 클래스

[Serializable]
public class UsedIngredientLog
{
    public int itemID;
    public string itemName;
    public int usedCount;
    public int unitPrice;
    public int totalPrice;      // unitPrice * usedCount
}

[Serializable]
public class GuestTypeLog
{
    public string guestType;    // GuestAppearance 이름
    public int count;
}

[Serializable]
public class GuestMoodLog
{
    public string mood;         // 매우 좋음, 좋음, 보통, 나쁨, 매우 나쁨
    public int count;
}

[Serializable]
public class UpgradeLog
{
    public string upgradeType;  // UpgradeType 이름
    public int level;           // 업그레이드 후 레벨
}

/// <summary>
/// 퀘스트는 하루가 끝난 뒤 NPC에게서 완료하므로 일일 로그와 별도로 기록한다.
/// </summary>
[Serializable]
public class QuestClearLog
{
    public int day;
    public string questName;
    public string questType;

    public int beforeMoney;
    public int afterMoney;
    public int beforeFame;
    public int afterFame;

    public string savedAt;
}

[Serializable]
public class DailyTestLog
{
    public int day;                 // 1일차, 10일차 ...
    public string dayOfWeek;

    public int startMoney;          // 하루 시작 시점 보유 돈
    public int endMoney;            // 하루 종료 시점 보유 돈
    public int moneyIncome;         // 그날 얻은 돈 (수입)
    public int ingredientCost;      // 사용한 재료의 가격 합계
    public int netIncome;           // 수입 - 재료비

    public int startFame;           // 하루 시작 시점 명성 (그날 손님 스폰 주기를 결정)
    public int endFame;             // 하루 종료 시점 명성
    public int fameIncome;          // 그날 얻은 명성

    public int servedGuestCount;    // 결제까지 끝낸 손님 수
    public int enteredGuestCount;   // 스폰된 손님 총 수

    public List<UpgradeLog> upgrades = new();
    public List<GuestTypeLog> enteredGuestTypes = new();   // 들어온 손님 유형
    public List<GuestTypeLog> paidGuestTypes = new();      // 결제한 손님 유형
    public int caughtThiefCount;                           // 잡은 도둑 수
    public int caughtThiefMoney;                           // 도둑에게 받아낸 돈
    public int caughtThiefFame;                            // 도둑 검거로 얻은 명성
    public List<GuestMoodLog> paidGuestMoods = new();      // 결제한 손님 기분
    public List<UsedIngredientLog> usedIngredients = new();

    public string savedAt;          // 실제 시각
}

// JsonUtility는 최상위 배열을 직렬화 못 하므로 래퍼가 필요함
[Serializable]
public class DailyTestLogFile
{
    public List<DailyTestLog> logs = new();
}

#endregion

/// <summary>
/// 게임 테스트용 일일 결과 로거.
/// 씬의 빈 오브젝트에 붙여두면 DayEnd 때마다 persistentDataPath에 json/txt로 기록한다.
/// 파일은 현재 플레이 중인 세이브 슬롯별로 분리해서 저장한다.
/// </summary>
public class DailyTestLogger : MonoBehaviour
{
    public static DailyTestLogger Instance { get; private set; }

    // txt/json에 항상 이 순서로 출력
    private static readonly string[] MoodOrder =
    {
        "매우 좋음", "좋음", "보통", "나쁨", "매우 나쁨"
    };

    [Header("설정")]
    [Tooltip("켜면 인벤토리 스냅샷 차이로 사용 재료를 추정한다. 끄면 RecordUsedIngredient 호출분만 기록한다.")]
    [SerializeField] private bool useInventoryDiff = true;

    [Tooltip("켜면 IngredientData인 아이템만 재료로 집계한다. (완성 요리가 재료비에 섞이는 것 방지)")]
    [SerializeField] private bool ingredientsOnly = true;

    [Tooltip("켜면 OnDayEnd 다음 프레임에 기록한다.")]
    [SerializeField] private bool waitOneFrame = true;

    [Tooltip("저장 파일 이름 (확장자 제외). 실제 파일명 뒤에 슬롯 번호가 붙는다.")]
    [SerializeField] private string fileName = "day_test_log";

    [Tooltip("사람이 읽는 txt도 같이 남길지")]
    [SerializeField] private bool writeTxt = true;

    // 하루 시작 시점의 아이템 개수 스냅샷 (itemID -> count)
    private readonly Dictionary<int, int> startSnapshot = new();

    private readonly Dictionary<int, int> manualUsed = new();

    // 손님 집계
    private readonly Dictionary<string, int> enteredTypeCounts = new();
    private readonly Dictionary<string, int> paidTypeCounts = new();
    private readonly Dictionary<GuestMood, int> paidMoodCounts = new();
    private int caughtThiefCount;
    private int caughtThiefMoney;
    private int caughtThiefFame;

    // 그날 진행한 업그레이드
    private readonly List<UpgradeLog> todayUpgrades = new();

    // 하루 시작 시점의 돈 / 명성
    private int startMoney;
    private int startFame;

    // 아이템 정보 캐시
    private readonly Dictionary<int, ItemInfo> itemInfoCache = new();

    private struct ItemInfo
    {
        public string name;
        public int price;
        public bool isIngredient;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (DayManager.Instance == null)
        {
            Debug.LogWarning("[DailyTestLogger] DayManager 없음. 구독 실패");
            return;
        }

        DayManager.Instance.OnDayStart += HandleDayStart;
        DayManager.Instance.OnDayEnd += HandleDayEnd;

        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.SelectedOptionUpgraded += HandleUpgraded;
    }

    private void OnDisable()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayStart -= HandleDayStart;
            DayManager.Instance.OnDayEnd -= HandleDayEnd;
        }

        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.SelectedOptionUpgraded -= HandleUpgraded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private static string MoodToKorean(GuestMood mood)
    {
        switch (mood)
        {
            case GuestMood.VeryGood: return "매우 좋음";
            case GuestMood.Good:     return "좋음";
            case GuestMood.Neutral:  return "보통";
            case GuestMood.Bad:      return "나쁨";
            case GuestMood.VeryBad:  return "매우 나쁨";
            default:                 return mood.ToString();
        }
    }

    #region 외부에서 직접 기록할 때 쓰는 API

    /// <summary>
    /// 손님 스폰 시 호출.
    /// GuestManager.InstantiateAndSetupGuest()에 아래 한 줄:
    /// DailyTestLogger.Instance?.RecordGuestSpawned(appearance);
    /// </summary>
    public void RecordGuestSpawned(string guestType)
    {
        if (string.IsNullOrEmpty(guestType))
            guestType = "Unknown";

        enteredTypeCounts.TryGetValue(guestType, out int prev);
        enteredTypeCounts[guestType] = prev + 1;
    }

    public void RecordGuestSpawned(GuestAppearance appearance)
    {
        RecordGuestSpawned(appearance.ToString());
    }

    /// <summary>
    /// 손님이 결제를 마쳤을 때 호출. (돈/명성을 실제로 준 손님만 집계)
    /// GuestManager의 guest.OnGuestPaid 람다 안에 아래 한 줄:
    /// DailyTestLogger.Instance?.RecordGuestPaid(appearance, guest.CurrentMood);
    /// </summary>
    public void RecordGuestPaid(GuestAppearance appearance, GuestMood mood)
    {
        string type = appearance.ToString();

        paidTypeCounts.TryGetValue(type, out int prevType);
        paidTypeCounts[type] = prevType + 1;

        paidMoodCounts.TryGetValue(mood, out int prevMood);
        paidMoodCounts[mood] = prevMood + 1;
    }
    
    public void RecordThiefCaught(int money, int fame)
    {
        caughtThiefCount++;
        caughtThiefMoney += money;
        caughtThiefFame += fame;
    }

    /// <summary>
    /// 재료를 실제로 소모하는 지점에서 호출 (useInventoryDiff를 끌 때 사용).
    /// </summary>
    public void RecordUsedIngredient(int itemID, int count = 1)
    {
        if (itemID <= 0 || count <= 0)
            return;

        manualUsed.TryGetValue(itemID, out int prev);
        manualUsed[itemID] = prev + count;
    }

    #endregion

    #region 퀘스트 완료 기록 (하루 로그와 별도)

    /// <summary>
    /// 퀘스트 완료 시 호출.
    /// 퀘스트는 DayEnd 이후 NPC에게서 완료하므로 일일 로그에 포함되지 않고 별도 블록으로 남긴다.
    /// Quest.Complete() 맨 위에 아래 한 줄:
    /// DailyTestLogger.Instance?.RecordQuestCompleted(this);
    /// </summary>
    public void RecordQuestCompleted(Quest quest)
    {
        if (quest == null)
            return;

        var log = new QuestClearLog
        {
            questName = quest.QuestName,
            questType = quest.QuestType.ToString(),
            savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        if (DayManager.Instance != null && DayManager.Instance.DayData != null)
            log.day = DayManager.Instance.DayData.day;

        if (EconomyManager.Instance != null && EconomyManager.Instance.RestaurantEconomy != null)
        {
            log.beforeMoney = EconomyManager.Instance.RestaurantEconomy.Money;
            log.beforeFame = EconomyManager.Instance.RestaurantEconomy.Fame;
        }

        // 보상 지급(AddMoney/AddFame)은 이 호출 직후에 일어나므로
        // 프레임 끝에 after 값을 읽어야 인연 보너스까지 반영된 실제 증가분이 잡힌다
        if (isActiveAndEnabled)
            StartCoroutine(WriteQuestLogEndOfFrame(log));
        else
            WriteQuestLog(log);
    }

    private IEnumerator WriteQuestLogEndOfFrame(QuestClearLog log)
    {
        yield return new WaitForEndOfFrame();
        WriteQuestLog(log);
    }

    private void WriteQuestLog(QuestClearLog log)
    {
        if (EconomyManager.Instance != null && EconomyManager.Instance.RestaurantEconomy != null)
        {
            log.afterMoney = EconomyManager.Instance.RestaurantEconomy.Money;
            log.afterFame = EconomyManager.Instance.RestaurantEconomy.Fame;
        }

        if (writeTxt)
            AppendQuestToTxt(log);

        Debug.Log($"[DailyTestLogger] 퀘스트 완료 기록: {log.questName}");
    }

    #endregion

    #region 업그레이드 기록

    private void HandleUpgraded(UpgradeType type)
    {
        int level = UpgradeManager.Instance != null
            ? UpgradeManager.Instance.GetLevel(type)
            : 0;

        todayUpgrades.Add(new UpgradeLog
        {
            upgradeType = type.ToString(),
            level = level
        });
    }

    #endregion

    #region DayManager 이벤트 핸들러

    private void HandleDayStart()
    {
        manualUsed.Clear();
        enteredTypeCounts.Clear();
        paidTypeCounts.Clear();
        paidMoodCounts.Clear();
        caughtThiefCount = 0;
        caughtThiefMoney = 0;
        caughtThiefFame = 0;
        startSnapshot.Clear();
        todayUpgrades.Clear();
        
        

        // 하루 시작 시점의 돈 / 명성
        // (명성은 그날 손님 스폰 주기를 결정하는 값이라 따로 남긴다)
        if (EconomyManager.Instance != null && EconomyManager.Instance.RestaurantEconomy != null)
        {
            startMoney = EconomyManager.Instance.RestaurantEconomy.Money;
            startFame = EconomyManager.Instance.RestaurantEconomy.Fame;
        }

        if (!useInventoryDiff)
            return;

        foreach (var pair in TakeInventorySnapshot())
            startSnapshot[pair.Key] = pair.Value;
    }

    private void HandleDayEnd()
    {
        if (waitOneFrame && isActiveAndEnabled)
            StartCoroutine(WriteNextFrame());
        else
            WriteLog();
    }

    private IEnumerator WriteNextFrame()
    {
        // OnDayEnd에 걸린 다른 핸들러(StopSpawn 등)가 먼저 끝나도록 한 프레임 대기
        yield return null;

        WriteLog();
    }

    private void WriteLog()
    {
        DailyTestLog log = BuildLog();

        AppendToJson(log);

        if (writeTxt)
            AppendToTxt(log);

        Debug.Log($"[DailyTestLogger] {log.day}일차 기록 완료 → {GetTxtPath()}");
    }

    #endregion

    #region 로그 생성

    private DailyTestLog BuildLog()
    {
        var log = new DailyTestLog
        {
            savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        // 일자
        if (DayManager.Instance != null && DayManager.Instance.DayData != null)
        {
            log.day = DayManager.Instance.DayData.day;
            log.dayOfWeek = DayManager.Instance.DayData.dayOfWeek.ToString();
        }

        // 하루 시작 시점 값
        log.startMoney = startMoney;
        log.startFame = startFame;

        // 돈 / 명성 / 응대 손님 수
        if (EconomyManager.Instance != null)
        {
            log.moneyIncome = EconomyManager.Instance.TodayMoneyIncome;
            log.fameIncome = EconomyManager.Instance.TodayFameIncome;
            log.servedGuestCount = EconomyManager.Instance.TodayCustomerCount;

            if (EconomyManager.Instance.RestaurantEconomy != null)
            {
                log.endMoney = EconomyManager.Instance.RestaurantEconomy.Money;
                log.endFame = EconomyManager.Instance.RestaurantEconomy.Fame;
            }
        }

        // 그날 진행한 업그레이드
        log.upgrades.AddRange(todayUpgrades);

        // 들어온 손님 유형
        foreach (var entry in SortedTypeList(enteredTypeCounts))
        {
            log.enteredGuestTypes.Add(entry);
            log.enteredGuestCount += entry.count;
        }

        // 결제한 손님 유형
        foreach (var entry in SortedTypeList(paidTypeCounts))
            log.paidGuestTypes.Add(entry);
        
        log.caughtThiefCount = caughtThiefCount;
        log.caughtThiefMoney = caughtThiefMoney;
        log.caughtThiefFame = caughtThiefFame;

        // 결제한 손님 기분 (정해진 순서대로)
        foreach (var moodName in MoodOrder)
        {
            foreach (var pair in paidMoodCounts)
            {
                if (MoodToKorean(pair.Key) != moodName || pair.Value <= 0)
                    continue;

                log.paidGuestMoods.Add(new GuestMoodLog { mood = moodName, count = pair.Value });
            }
        }

        // 사용한 재료 + 재료비
        foreach (var pair in CollectUsedIngredients())
        {
            ItemInfo info = GetItemInfo(pair.Key);

            // 완성 요리(FoodData) 등이 재료비에 섞이지 않도록
            if (ingredientsOnly && !info.isIngredient)
                continue;

            int total = info.price * pair.Value;

            log.usedIngredients.Add(new UsedIngredientLog
            {
                itemID = pair.Key,
                itemName = info.name,
                usedCount = pair.Value,
                unitPrice = info.price,
                totalPrice = total
            });

            log.ingredientCost += total;
        }

        // 금액 큰 재료부터
        log.usedIngredients.Sort((a, b) => b.totalPrice.CompareTo(a.totalPrice));

        log.netIncome = log.moneyIncome - log.ingredientCost;

        return log;
    }

    private List<GuestTypeLog> SortedTypeList(Dictionary<string, int> counts)
    {
        var keys = new List<string>(counts.Keys);
        keys.Sort(StringComparer.Ordinal);

        var result = new List<GuestTypeLog>();

        foreach (var key in keys)
            result.Add(new GuestTypeLog { guestType = key, count = counts[key] });

        return result;
    }

    private Dictionary<int, int> CollectUsedIngredients()
    {
        if (!useInventoryDiff)
            return new Dictionary<int, int>(manualUsed);

        var result = new Dictionary<int, int>(manualUsed);
        var endSnapshot = TakeInventorySnapshot();

        foreach (var pair in startSnapshot)
        {
            endSnapshot.TryGetValue(pair.Key, out int endCount);

            int diff = pair.Value - endCount; // 줄어든 만큼 = 사용량
            if (diff <= 0)
                continue;

            result.TryGetValue(pair.Key, out int prev);
            result[pair.Key] = prev + diff;
        }

        return result;
    }

    /// <summary>
    /// 모든 인벤토리의 아이템 개수를 itemID 기준으로 합산.
    /// </summary>
    private Dictionary<int, int> TakeInventorySnapshot()
    {
        var snapshot = new Dictionary<int, int>();

        if (InventoryManager.Instance == null)
            return snapshot;

        List<Inventory> inventories = InventoryManager.Instance.GetAll();
        if (inventories == null)
            return snapshot;

        foreach (var inv in inventories)
        {
            if (inv == null)
                continue;

            // 인연 아이템은 소모품이 아니므로 제외
            if (inv.Type == InventoryType.BondItem)
                continue;

            InventorySaveData data = inv.GetSaveData();
            if (data?.slots == null)
                continue;

            foreach (var slot in data.slots)
            {
                if (slot == null || slot.itemID <= 0 || slot.count <= 0)
                    continue;

                snapshot.TryGetValue(slot.itemID, out int prev);
                snapshot[slot.itemID] = prev + slot.count;
            }
        }

        return snapshot;
    }

    #endregion

    #region 아이템 이름 / 가격 조회

    private ItemInfo GetItemInfo(int id)
    {
        if (itemInfoCache.TryGetValue(id, out ItemInfo cached))
            return cached;

        var info = new ItemInfo { name = $"ID_{id}", price = 0, isIngredient = false };

        ItemData data = null;

        // ItemDatabase.Get()은 없는 키면 예외를 던지므로 감싼다
        try
        {
            if (DataManager.Instance != null)
                data = DataManager.Instance.GetItemData(id);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DailyTestLogger] ItemData 조회 실패 (ID {id}): {e.Message}");
        }

        if (data == null)
        {
            Debug.LogWarning($"[DailyTestLogger] ItemData 못 찾음 (ID {id})");
            itemInfoCache[id] = info;
            return info;
        }

        info.name = string.IsNullOrEmpty(data.itemName) ? data.name : data.itemName;
        info.price = data.price;
        info.isIngredient = data is IngredientData;

        itemInfoCache[id] = info;
        return info;
    }

    #endregion

    #region 파일 저장

    private string GetDirectory()
    {
        string dir = Path.Combine(Application.persistentDataPath, "TestLogs");

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        return dir;
    }

    /// <summary>
    /// 현재 플레이 중인 세이브 슬롯에 따라 파일명을 나눈다.
    /// 예) day_test_log_auto.txt, day_test_log_slot2.txt
    /// </summary>
    private string GetSlotSuffix()
    {
        if (SaveManager.Instance == null)
            return "";

        if (SaveManager.Instance.CurrentSlotType == SaveSlotType.Auto)
            return "_auto";

        return $"_slot{SaveManager.Instance.CurrentSlotIndex}";
    }

    private string GetJsonPath() => Path.Combine(GetDirectory(), fileName + GetSlotSuffix() + ".json");
    private string GetTxtPath() => Path.Combine(GetDirectory(), fileName + GetSlotSuffix() + ".txt");

    private void AppendToJson(DailyTestLog log)
    {
        string path = GetJsonPath();
        DailyTestLogFile file = new DailyTestLogFile();

        if (File.Exists(path))
        {
            try
            {
                string prev = File.ReadAllText(path);
                var loaded = JsonUtility.FromJson<DailyTestLogFile>(prev);

                if (loaded?.logs != null)
                    file = loaded;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DailyTestLogger] 기존 json 파싱 실패, 새로 만듦: {e.Message}");
            }
        }

        file.logs.Add(log);

        File.WriteAllText(path, JsonUtility.ToJson(file, true), Encoding.UTF8);
    }

    private void AppendToTxt(DailyTestLog log)
    {
        var sb = new StringBuilder();

        // 시작 + 수입 - 종료 = 그날 쓴 돈 (상점 구매, 업그레이드 등)
        int spent = log.startMoney + log.moneyIncome - log.endMoney;

        sb.AppendLine("========================================");
        sb.AppendLine($"{log.day}일차 ({log.dayOfWeek})   [{log.savedAt}]");
        sb.AppendLine($"돈   : {log.startMoney:N0} → {log.endMoney:N0}   (수입 +{log.moneyIncome:N0} / 지출 -{spent:N0})");
        sb.AppendLine($"명성 : {log.startFame:N0} → {log.endFame:N0}   (획득 +{log.fameIncome:N0})");
        sb.AppendLine($"들어온 손님 : {log.enteredGuestCount}");
        sb.AppendLine($"결제한 손님 : {log.servedGuestCount}");

        sb.AppendLine("업그레이드:");
        if (log.upgrades.Count == 0)
        {
            sb.AppendLine("  (없음)");
        }
        else
        {
            foreach (var up in log.upgrades)
                sb.AppendLine($"  - {up.upgradeType} → Lv.{up.level}");
        }

        sb.AppendLine("들어온 손님 유형:");
        if (log.enteredGuestTypes.Count == 0)
        {
            sb.AppendLine("  (기록 없음 - RecordGuestSpawned 훅 확인)");
        }
        else
        {
            foreach (var t in log.enteredGuestTypes)
                sb.AppendLine($"  - {t.guestType}손님 {t.count}명");
        }

        sb.AppendLine("결제한 손님 유형:");
        if (log.paidGuestTypes.Count == 0)
        {
            sb.AppendLine("  (없음)");
        }
        else
        {
            foreach (var t in log.paidGuestTypes)
                sb.AppendLine($"  - {t.guestType}손님 {t.count}명");
        }
        sb.AppendLine("잡은 도둑:");
        if (log.caughtThiefCount == 0)
        {
            sb.AppendLine("  (없음)");
        }
        else
        {
            sb.AppendLine($"  - {log.caughtThiefCount}명 검거" +
                          $"  (받아낸 돈 {log.caughtThiefMoney:N0} / 명성 +{log.caughtThiefFame})");
        }

        sb.AppendLine("결제한 손님 기분:");
        if (log.paidGuestMoods.Count == 0)
        {
            sb.AppendLine("  (없음)");
        }
        else
        {
            foreach (var m in log.paidGuestMoods)
                sb.AppendLine($"  - {m.mood} 손님 {m.count}명");
        }

        sb.AppendLine("사용한 재료:");
        if (log.usedIngredients.Count == 0)
        {
            sb.AppendLine("  (없음)");
        }
        else
        {
            foreach (var ing in log.usedIngredients)
            {
                sb.AppendLine($"  - {ing.itemName} (ID {ing.itemID}) x{ing.usedCount}" +
                              $" @ {ing.unitPrice:N0} = {ing.totalPrice:N0}");
            }
        }

        sb.AppendLine("----------------------------------------");
        sb.AppendLine($"재료비 합계 : {log.ingredientCost:N0}");
        sb.AppendLine($"순수입      : {log.netIncome:N0}   (수입 {log.moneyIncome:N0} - 재료비 {log.ingredientCost:N0})");
        sb.AppendLine();

        File.AppendAllText(GetTxtPath(), sb.ToString(), Encoding.UTF8);
    }

    private void AppendQuestToTxt(QuestClearLog log)
    {
        var sb = new StringBuilder();

        int moneyGain = log.afterMoney - log.beforeMoney;
        int fameGain = log.afterFame - log.beforeFame;

        sb.AppendLine("========================================");
        sb.AppendLine($"{log.day}일차 종료 후 퀘스트 완료   [{log.savedAt}]");
        sb.AppendLine($"완료한 퀘스트 : [{log.questType}] {log.questName}");
        sb.AppendLine($"돈   : {log.beforeMoney:N0} → {log.afterMoney:N0}   (획득 +{moneyGain:N0})");
        sb.AppendLine($"명성 : {log.beforeFame:N0} → {log.afterFame:N0}   (획득 +{fameGain:N0})");
        sb.AppendLine();

        File.AppendAllText(GetTxtPath(), sb.ToString(), Encoding.UTF8);
    }

    #endregion

    #region 편의 기능

    [ContextMenu("로그 폴더 열기")]
    private void OpenLogFolder()
    {
        Application.OpenURL("file://" + GetDirectory());
    }

    [ContextMenu("현재 슬롯 로그 초기화")]
    private void ClearLogs()
    {
        if (File.Exists(GetJsonPath())) File.Delete(GetJsonPath());
        if (File.Exists(GetTxtPath())) File.Delete(GetTxtPath());

        Debug.Log($"[DailyTestLogger] 로그 삭제 완료 ({GetSlotSuffix()})");
    }

    [ContextMenu("모든 슬롯 로그 초기화")]
    private void ClearAllLogs()
    {
        string dir = GetDirectory();

        foreach (string path in Directory.GetFiles(dir, fileName + "*"))
            File.Delete(path);

        Debug.Log("[DailyTestLogger] 전체 로그 삭제 완료");
    }

    #endregion
}