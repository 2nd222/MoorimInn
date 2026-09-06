using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class PoolManager : Singleton<PoolManager>
{
    [SerializeField] private Transform poolParent;

    // 타입별 비활성 오브젝트 큐
    private Dictionary<string, Queue<GameObject>> pools
        = new Dictionary<string, Queue<GameObject>>();

    // 타입 → 프리팹 매핑
    private Dictionary<string, GameObject> prefabMap
        = new Dictionary<string, GameObject>();

    protected override void Awake()
    {
        base.Awake();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        poolParent = GameObject.FindWithTag("Pool").transform;

        // 씬이 바뀌면 이전 씬의 풀 오브젝트들은 파괴됨 = 죽은 참조 정리
        CleanupDestroyedObjects();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬 전환 등으로 파괴된 오브젝트 참조를 큐에서 제거
    private void CleanupDestroyedObjects()
    {
        foreach (var key in pools.Keys)
        {
            var queue = pools[key];
            int count = queue.Count;
            for (int i = 0; i < count; i++)
            {
                GameObject obj = queue.Dequeue();
                if (obj != null)          // 파괴된 오브젝트는 == null
                    queue.Enqueue(obj);   // 살아있는 것만 다시 넣음
            }
        }
    }

    // 프리팹 정보를 여기에 등록하는 메소드
    public void Register(string key, GameObject prefab, int preloadCount = 3)
    {
        // 프리팹 매핑은 항상 갱신 (씬 재로드 대비)
        prefabMap[key] = prefab;

        // 이미 풀이 있으면 프리로드는 생략
        if (pools.ContainsKey(key))
            return;

        var queue = new Queue<GameObject>();

        for (int i = 0; i < preloadCount; i++)
        {
            GameObject obj = CreateObject(key);
            queue.Enqueue(obj);
        }

        pools[key] = queue;
        Debug.Log($"[풀] {key} 등록 완료 {preloadCount}개");
    }

    // 풀에서 꺼내기. 죽은 참조는 건너뛰고, 큐가 비면 새로 만듦
    public GameObject Get(string key)
    {
        if (!prefabMap.ContainsKey(key))
        {
            Debug.LogError($"[풀] 등록되지 않은 키: {key}");
            return null;
        }

        if (pools.TryGetValue(key, out var queue))
        {
            // 살아있는 오브젝트가 나올 때까지 꺼냄 (죽은 참조는 버림)
            while (queue.Count > 0)
            {
                GameObject obj = queue.Dequeue();
                if (obj != null)
                    return obj;
            }
        }

        // 큐가 비었거나 전부 죽은 참조였음 = 새로 생성
        return CreateObject(key);
    }

    // Destroy 대신 이 메소드를 호출. 비활성화 후 큐에 반환
    public void Return(string key, GameObject obj)
    {
        if (obj == null)
            return;

        obj.SetActive(false);

        if (poolParent != null)
            obj.transform.SetParent(poolParent);

        if (!pools.ContainsKey(key))
            pools[key] = new Queue<GameObject>();

        pools[key].Enqueue(obj);
    }

    private GameObject CreateObject(string key)
    {
        GameObject obj = Instantiate(prefabMap[key], poolParent);

        // NavMeshAgent가 있으면 끄기 (손님용)
        NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.enabled = false;

        obj.SetActive(false);
        return obj;
    }
}