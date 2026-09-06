using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SeatManager : Singleton<SeatManager>
{
    private List<Chair> allChairs = new List<Chair>();
    private HashSet<Chair> emptyChairSet = new HashSet<Chair>();
    private List<Chair> emptyChairList = new List<Chair>();
    
    public event Action<Chair> OnSeatFreed;

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected override void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        base.OnDestroy();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        allChairs.Clear();
        emptyChairSet.Clear();
        emptyChairList.Clear();
    }

    public void RegisterChair(Chair chair)
    {
        if (!allChairs.Contains(chair))
            allChairs.Add(chair);

        if (chair.IsEmpty && !emptyChairSet.Contains(chair))
        {
            emptyChairSet.Add(chair);
            emptyChairList.Add(chair);
        }
    }
    
    public Chair FindEmptyChair()
    {
        // 죽은 참조 방어 (혹시 정리 타이밍을 빠져나간 경우 대비)
        emptyChairList.RemoveAll(c => c == null);
        emptyChairSet.RemoveWhere(c => c == null);

        if (emptyChairList.Count == 0)
            return null; 

        int randomIndex = UnityEngine.Random.Range(0, emptyChairList.Count);
        return emptyChairList[randomIndex];        
    }

    public void ReserveChair(Chair chair)
    {
        if (emptyChairSet.Remove(chair))
            emptyChairList.Remove(chair);
    }

    public void FreeChair(Chair chair)
    {
        if (chair == null) return;

        if (chair.IsEmpty && emptyChairSet.Add(chair))
        {
            emptyChairList.Add(chair);
            OnSeatFreed?.Invoke(chair);
        }
    }

    public void EmptyAllChairs()
    {
        allChairs.RemoveAll(c => c == null);
        emptyChairList.RemoveAll(c => c == null);
        emptyChairSet.RemoveWhere(c => c == null);

        foreach (var chair in allChairs)
        {
            chair.Empty();
        }

        Debug.Log($"[SeatManager] 전체 의자 초기화. 빈 의자: {emptyChairList.Count}/{allChairs.Count}");
    }
}