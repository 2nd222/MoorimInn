using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SaveData
{   
    public int version = 1;

    public DaySaveData day;
    public QuestSaveData quest;
    public RecipeSaveData recipe;
    public IngredientSaveData ingredient;
    public StorySaveData story;
    public EconomySaveData economy;
    public DailyResultSaveData dailyResult;
    public List<InventorySaveData> inventories;
    public BondSaveData bond;
    public UpgradeSaveData upgrade;
}

[Serializable]
public class ManualSaveData
{
    // 이어하기용
    public SaveData current;

    // 오늘 시작으로 되돌리기용
    public SaveData checkpoint;

    // UI 표시용
    public SaveMetaData meta;
}

public enum SaveSlotType
{
    Auto,
    Manual
}
[Serializable]
public class SaveMetaData
{
    public int day;
    public int hour;
    public int minute;
    public string saveTime; // 실제 시간
    public GameFlowState flowState;
}

public class SaveManager : Singleton<SaveManager>
{
    
    public bool isLoadedGame = false;
    private bool isQuitting = false;
    
    private int loadSlotIndex;      // DailyTestLogger.cs용
    private SaveSlotType loadType;  // DailyTestLogger.cs용
    
    // 현재 플레이 중인 슬롯 (테스트 로그 파일 분리 등에 사용)
    public int CurrentSlotIndex => loadSlotIndex;
    public SaveSlotType CurrentSlotType => loadType;
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    // ------------------------
    // 새게임 시작
    // ------------------------
    public void StartNewGame()
    {
        Time.timeScale = 1f;
        isLoadedGame = false;
        
        loadSlotIndex = 0;                  // DailyTestLogger.cs용
        loadType = SaveSlotType.Auto;       // DailyTestLogger.cs용
        
        SceneLoader.Instance.LoadScene("01_Scenes/03.Game", () =>
        {
            InitAll();
            InitializeNewGameData();

            UIManager.Instance.CloseAllUI();
            DayManager.Instance.RemoteDayStart(isLoadedGame);
        });
    }
    
    // ------------------------
    // 저장
    // ------------------------
    public void SaveGame(int slotIndex, SaveSlotType type)
    {
        if (isQuitting)
        {
            Debug.Log("종료 중 저장 스킵");
            return;
        }
        
        if(type == SaveSlotType.Manual)
        {
            SaveManual(slotIndex);
            return;
        }
        
        string path = GetSavePath(slotIndex, type);
        
        SaveData data = CreateSaveData();
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        
        Debug.Log($"저장 완료: {path}");
    }
    
    private void SaveManual(int slot)
    {
        ManualSaveData manual = new();

        // 현재 상태
        manual.current = CreateSaveData();

        // DayStart 체크포인트
        string checkpointPath = GetDayStartCheckpointPath();

        if (!File.Exists(checkpointPath))
        {
            Debug.LogError("DayStart 체크포인트 없음");
            return;
        }

        manual.checkpoint = JsonUtility.FromJson<SaveData>(File.ReadAllText(checkpointPath));
        
        manual.meta = new SaveMetaData()
        {
            day = manual.current.day.day,
            hour = manual.current.day.hour,
            minute = manual.current.day.minute,
            saveTime = DateTime.Now.ToString(),
            flowState = manual.current.day.flowState,
        };
        
        File.WriteAllText(GetManualPath(slot), JsonUtility.ToJson(manual, true));
        Debug.Log($"수동 저장 완료 : 슬롯 {slot}");
        
        // 만약 슬롯 삭제 기능 넣을 거면 두 개 호출 필수
        // File.Delete(GetCheckpointPath(slot));
        // File.Delete(GetMetaPath(slot));
    }
    
    public void SaveDayStartCheckpoint()
    {
        SaveData data = CreateSaveData();

        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(GetDayStartCheckpointPath(), json);

        Debug.Log("DayStart 체크포인트 저장");
    }
    
    private string GetSavePath(int slotIndex, SaveSlotType type)
    {
        string fileName = type == SaveSlotType.Auto
            ? "auto_save.json"
            : $"save_{slotIndex}.json";

        return Path.Combine(Application.persistentDataPath, fileName);
    }
    
    private string GetManualPath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"save_{slot}.json");
    }

    private string GetDayStartCheckpointPath()
    {
        return Path.Combine(
            Application.persistentDataPath,
            "daystart_checkpoint.json");
    }
    
    // ------------------------
    // 불러오기
    // ------------------------
    public void LoadGame(int slotIndex, SaveSlotType type)
    {
        Time.timeScale = 1f;
        isLoadedGame = true;
        
        loadSlotIndex = slotIndex;
        loadType = type;
        
        SceneLoader.Instance.LoadScene("01_Scenes/03.Game", () =>
        {
            InitAll();
            
            if(type == SaveSlotType.Manual)
            {
                ManualSaveData manual = JsonUtility.FromJson<ManualSaveData>(File.ReadAllText(GetManualPath(slotIndex)));

                if(!File.Exists(GetManualPath(slotIndex)))
                    return;
                
                if(manual == null)
                    return;
                
                SaveData loadData = null;

                if(manual.current.day.flowState == GameFlowState.Gameplay)
                {
                    loadData = manual.checkpoint;
                }
                else
                {
                    loadData = manual.current;
                }

                ApplySaveData(loadData);
                GameManager.Instance.ResetFlow();
                UIManager.Instance.CloseAllUI();
                ApplyFlowState(loadData.day.flowState);

                return;
            }
            
            string path = GetSavePath(loadSlotIndex, loadType);

            if (!File.Exists(path))
            {
                Debug.Log("세이브 없음 → 새 게임");
                InitializeNewGameData();
                return;
            }
            else
            {
                string json = File.ReadAllText(path);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                ApplySaveData(data);
                
                GameManager.Instance.ResetFlow();
                UIManager.Instance.CloseAllUI();

                ApplyFlowState(data.day.flowState);
            }
        });
    }
    
    private IEnumerator StartRoutine()
    {
        yield return null; // 씬 초기화 대기
        
        InitAll();
        
        yield return null;
        yield return new WaitUntil(() => InventoryManager.Instance.IsInitialized);
        yield return null;
        
        if (isLoadedGame)
        {
            string path = GetSavePath(loadSlotIndex, loadType);
            
            if (!File.Exists(path))
            {
                Debug.Log("세이브 파일 없음 → 새 게임 시작");
            }
            else
            {
                string json = File.ReadAllText(path);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                ApplySaveData(data);
            }
        }
        else if (!isLoadedGame)
        {
            InitializeNewGameData(); 
        }
        
        UIManager.Instance.CloseAllUI();
        
        DayManager.Instance.RemoteDayStart(isLoadedGame);
        Debug.Log(isLoadedGame ? "로드 완료" : "새 게임 시작");
    }
    
    private void InitAll()
    {
        EconomyManager.Instance.Init();
        RecipeUnlockManager.Instance.Init();
        IngredientUnlockManager.Instance.Init();
        InventoryManager.Instance.Init();
        StoryManager.Instance.Init();
        QuestManager.Instance.Init();
        BondManager.Instance.Init();
        UpgradeManager.Instance.Init();
    }
    
    private void ApplyFlowState(GameFlowState flowState)
    {
        switch(flowState)
        {
            case GameFlowState.Gameplay:
                DayManager.Instance.RemoteDayStart(true);
                break;

            case GameFlowState.Receipt:
                DayManager.Instance.RefreshUI();
                GameManager.Instance.SetState(GameFlowState.Gameplay);
                GameManager.Instance.CompleteState(GameFlowState.Gameplay);
                break;

            case GameFlowState.Story:
                // Story 복구
                break;

            case GameFlowState.NPC:
                GameManager.Instance.EnterState(GameFlowState.NPC);
                break;
        }
    }
    
    // ------------------------
    // 데이터 생성
    // ------------------------
    private SaveData CreateSaveData()
    {
        SaveData data = new SaveData();

        if (DayManager.Instance)
            data.day = DayManager.Instance.GetSaveData();

        if (InventoryManager.Instance)
            data.inventories = InventoryManager.Instance.GetAll()
                .Where(inv => inv.Type != Constants.InventoryType.BondItem)
                .Select(inv => inv.GetSaveData()).ToList();

        if (RecipeUnlockManager.Instance)
            data.recipe = RecipeUnlockManager.Instance.GetSaveData();

        if (IngredientUnlockManager.Instance)
            data.ingredient = IngredientUnlockManager.Instance.GetSaveData();

        if (StoryManager.Instance)
            data.story = StoryManager.Instance.GetSaveData();

        if (EconomyManager.Instance)
        {
            data.economy = EconomyManager.Instance.GetSaveData();
            data.dailyResult = EconomyManager.Instance.LastDailyResult;;
        }

        if (QuestManager.Instance)
            data.quest = QuestManager.Instance.GetSaveData();
        
        if (BondManager.Instance)
            data.bond = BondManager.Instance.GetSaveData();

        if (UpgradeManager.Instance)
            data.upgrade = UpgradeManager.Instance.GetSaveData();
        
        return data;
    }

    // ------------------------
    // 데이터 적용
    // ------------------------
    private void ApplySaveData(SaveData data)
    {
        DayManager.Instance.LoadFromData(data.day);
        RecipeUnlockManager.Instance.LoadFromData(data.recipe);
        IngredientUnlockManager.Instance.LoadFromData(data.ingredient);
        EconomyManager.Instance.LoadFromData(data.economy);
        if (data.dailyResult != null)
        {
            EconomyManager.Instance.LoadDailyResult(data.dailyResult);
        }
        if (data.bond != null)
        {
            BondManager.Instance.LoadFromData(data.bond);
        }
        
        if (data.upgrade != null)
        {
            UpgradeManager.Instance.LoadFromData(data.upgrade);
            InventoryManager.Instance.ApplyInventoryUpgrade();
            UpgradeManager.Instance.ApplyLoadedUpgrades();
        }
        
        if (data.inventories != null)
        {
            foreach (var invData in data.inventories)
            {
                if (invData.type == Constants.InventoryType.BondItem)
                    continue;
                
                Debug.Log($"로드 시도 타입: {invData.type}");
                var inv = InventoryManager.Instance.GetInventory(invData.type);
                Debug.Log($"찾은 인벤토리: {inv}");
                inv.LoadFromData(invData);
            }
        }
        QuestManager.Instance.LoadFromData(data.quest);
        StoryManager.Instance.LoadFromData(data.story);
    }

    private void InitializeNewGameData()
    {
        DayManager.Instance.ResetData();
        RecipeUnlockManager.Instance.ResetData();
        IngredientUnlockManager.Instance.ResetData();
        InventoryManager.Instance.ResetData();
        StoryManager.Instance.ResetData();
        EconomyManager.Instance.ResetData();
        QuestManager.Instance.ResetData();
        BondManager.Instance.ResetData();
        UpgradeManager.Instance.ResetData();
    }
    
    public bool CanManualSave()
    {
        return File.Exists(GetDayStartCheckpointPath());
    }
    
    public void SaveAndQuit()
    {
        SaveGame(0, SaveSlotType.Auto);
        Application.Quit();
    }
}

[Serializable]
public class DaySaveData
{
    public int day;
    public int dayOfWeek;
    public int hour;
    public int minute;
    
    public GameFlowState flowState;
}

[Serializable]
public class QuestSaveData
{
    public List<QuestRuntimeData> waitingQuests = new();
    public List<QuestRuntimeData> activeQuests = new();
    public List<int> completedQuestIDs = new();
}

[Serializable]
public class QuestRuntimeData
{
    public int questID;

    public int rewardGold;
    public int rewardFame;

    public List<ConditionRuntimeData> conditions = new();
}

[Serializable]
public class ConditionRuntimeData
{
    public int conditionIndex;
    public QuestConditionType conditionType;
    
    // ===== ItemCondition =====
    public int itemID;
    public int requiredCount;
    public int currentCount;

    // ===== ProgressCondition =====
    public ProgressType progressType;
    public int requiredMoney;
    public int requiredFame;
    public int requiredDay;
    public int prerequisiteQuestID;

    // ===== FailLimitCondition =====
    public int maxFailCount;
    public int failCount;

    // ===== ServeCondition =====
    public int serveRequired;
    public int serveCurrent;

    // ===== CatchThiefCondition =====
    public int catchRequired;
    public int catchCurrent;
}

[Serializable]
public class RecipeSaveData
{
    public List<int> unlockedRecipeIDs = new();
    public List<RecipeProficiencyData> proficiencies = new();
}

[Serializable]
public class RecipeProficiencyData
{
    public int recipeID;
    public int proficiency;
}

[Serializable]
public class IngredientSaveData
{
    public List<int> unlockedIngredientIDs = new();
}

[Serializable]
public class StorySaveData
{
    public List<int> playedStoryIDs = new();
    public List<int> playedStoryHistory = new();
    public List<string> flags = new();
}

[Serializable]
public class EconomySaveData
{
    public int money;
    public int fame;
    public int debt;
}

[Serializable]
public class DailyResultSaveData
{
    public int todayMoneyIncome;
    public int todayFameIncome;
    public int todayCustomerCount;

    public int happyGuests;
    public int angryGuests;
}

[Serializable]
public class InventorySaveData
{
    public Constants.InventoryType type;
    public List<SlotSaveData> slots = new();
}

[Serializable]
public class SlotSaveData
{
    public int itemID;
    public int count;

    public float spoilTimer;
    public int fridgeSpoilTimer;
    
    public Constants.FreshState freshState;
}

[Serializable]
public class BondSaveData
{
    public List<int> ownedBondItemIds = new();
}

[Serializable]
public class UpgradeSaveData
{
    public List<UpgradeLevelSaveData> levels = new();
}

[Serializable]
public class UpgradeLevelSaveData
{
    public UpgradeType type;
    public int level;
}