using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

[Serializable]
public enum DayOfWeek
{
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
    Sunday
}

/// <summary>
/// 주간 운영에서 시간, 날짜를 관리하는 클래스
/// </summary>
public class DayManager : Singleton<DayManager>
{
    // 날짜 계산 관련 매니저
    
    [SerializeField] private float dayLength = 600f;
    [SerializeField] private float gameStartHour = 9f;
    [SerializeField] private float gameEndHour = 21f;
    
    private List<INPCSchedule> npcs = new List<INPCSchedule>();
    public List<INPCSchedule> NPCs { get => npcs; private set => npcs = value; }

    private float _totalGameMinutes;
    private float _timer = 0f;
    
    [SerializeField] private DayData dayData;
    public DayData DayData {  get => dayData; private set => dayData = value; }

    public bool IsRunning { get; private set; }
    private bool isPaused;
    public bool IsPaused => isPaused;
    
    public event Action OnDayStart; // 각 매니저에서 가게 운영이 시작될 때의 동작들을 여기에 등록
    public event Action OnDayEnd; // 각 매니저에서 가게 운영이 종료될 때의 동작들을 여기에 등록
    public event Action<int, int> OnTimeChanged;
    public event Action OnDateChanged;
    
    private int _hour;
    private int _prevHour = -1;
    private int _minute;
    private int _prevMinute = -1;
    
    private bool _isFirstDayStart = true;
    private bool _isBusinessOver = false;

    protected override void Awake()
    {
        base.Awake();
        
        _totalGameMinutes = (gameEndHour - gameStartHour) * 60f;
    }
    
    public void RegisterNPC(INPCSchedule npc)
    {
        if (!npcs.Contains(npc))
            npcs.Add(npc);
    }

    public void UnregisterNPC(INPCSchedule npc)
    {
        if (npcs.Contains(npc))
            npcs.Remove(npc);
    }
    
    /// <summary>
    /// 주간 영업이 시작될 때 호출되는 함수. OnDayStart Action을 이용해 다른 매니저들도 영업이 시작될 때 맞는 행동을 한다.
    /// </summary>
    public void DayStart(bool isLoad = false)
    {
        Debug.Log("DayStart");
        GameManager.Instance.SetState(GameFlowState.Gameplay);
        
        if (!isLoad)
            _timer = 0f;
        
        Time.timeScale = 1f;
        IsRunning = true;
        isPaused = false;
        
        // _hour = (int)gameStartHour;
        // _minute = 0;
        _prevHour = -1;
        _prevMinute = -1;
        
        _isBusinessOver = false;
        
        
        if (_isFirstDayStart)
        {
            _isFirstDayStart = false;
        }
        else if(!isLoad)
        {
            UpdateDayData();
        }
        
        OnDateChanged?.Invoke();
        
        foreach (var npc in NPCs)
        {
            npc.DailyQuestCreate();
        }
        
        OnDayStart?.Invoke();
        
        SaveManager.Instance.SaveDayStartCheckpoint();
        SaveManager.Instance.SaveGame(0, SaveSlotType.Auto);
    }
    
    private void LoadDayStart()
    {
        GameManager.Instance.SetState(GameFlowState.Gameplay);
        Debug.Log("Load Day Start");

        Time.timeScale = 1f;
        IsRunning = true;
        isPaused = false;

        _timer = 0f;
        
        _prevHour = -1;
        _prevMinute = -1;

        _isBusinessOver = false;
        
        OnDateChanged?.Invoke();
        OnTimeChanged?.Invoke((int)gameStartHour, 0);
        OnDayStart?.Invoke();
    }

    public void RemoteDayStart(bool isLoad = false)
    {
        if (isLoad)
            LoadDayStart();
        else
            DayStart(isLoad);
    }
    
    void Update()
    {
        if (!IsRunning) return;
        if (isPaused) return;
        
        _timer += Time.deltaTime;

        float t = Mathf.Clamp01(_timer / dayLength);
        float currentGameMinutes = t * _totalGameMinutes;

        float currentHour = gameStartHour + currentGameMinutes / 60f;
        _hour = Mathf.FloorToInt(currentHour);
        _minute = Mathf.FloorToInt((currentHour - _hour) * 60f);

        if (_hour != _prevHour || _minute != _prevMinute)
        {
            _prevHour = _hour;
            _prevMinute = _minute;

            OnTimeChanged?.Invoke(_hour, _minute);
        }
        
        //Debug.Log($"{_hour:00}:{_minute:00}");
        
        if (_timer >= dayLength)
        {
            DayEnd();
        }
    }
    
    /// <summary>
    /// 주간 영업이 끝날 때 호출되는 함수. OnDayStart Action을 이용해 다른 매니저들도 영업이 끝날 때 맞는 행동을 한다.
    /// </summary>
    public void DayEnd()
    {
        Debug.Log("DayEnd");
        IsRunning = false;
        _isBusinessOver = true;
        
        OnDayEnd?.Invoke();
        if (GameManager.Instance.CurrentState == GameFlowState.Gameplay)
        {
            GameManager.Instance.SetSaveState(GameFlowState.Receipt);
            SaveManager.Instance.SaveGame(0, SaveSlotType.Auto);
            
            GameManager.Instance.CompleteState(GameFlowState.Gameplay);
        }
    }

    /// <summary>
    /// 날짜 데이터 업데이트 함수
    /// </summary>
    private void UpdateDayData()
    {
        dayData.day++;
        dayData.dayOfWeek = (DayOfWeek)(((int)dayData.dayOfWeek + 1) % 7);
        Debug.Log($"오늘 날짜 {dayData.day}일, {dayData.dayOfWeek}");
    }
    
    
    public void RefreshUI()
    {
        OnDateChanged?.Invoke();
        OnTimeChanged?.Invoke(_hour, _minute);
    }
    
    public DaySaveData GetSaveData()
    {
        return new DaySaveData
        {
            day = DayData.day,
            dayOfWeek = (int)DayData.dayOfWeek,
            hour = _isBusinessOver ? (int)gameEndHour : (int)gameStartHour,
            minute = 0,
            flowState = GameManager.Instance.SaveState
        };
    }
    
    public void LoadFromData(DaySaveData data)
    {
        DayData.day = data.day;
        DayData.dayOfWeek = (DayOfWeek)data.dayOfWeek;
        _hour = data.hour;
        _minute = data.minute;
        
        float loadedMinutes = (data.hour - gameStartHour) * 60f + data.minute;
        _timer = (loadedMinutes / _totalGameMinutes) * dayLength;
        
        _isFirstDayStart = false;
    }
    
    public void ResetData()
    {
        if (dayData == null)
        {
            Debug.LogError("DayData 없음");
            return;
        }

        dayData.day = 1;
        dayData.dayOfWeek = DayOfWeek.Monday;

        _timer = 0f;
        IsRunning = false;

        _isFirstDayStart = true;

        Debug.Log("DayManager Reset 완료");
    }
    
    public void PauseTime()
    {
        isPaused = true;
    }

    public void ResumeTime()
    {
        isPaused = false;
    }
}
