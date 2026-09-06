using System;
using System.Collections.Generic;
using UnityEngine;

public class SafeEventBus : MonoBehaviour // 일단 고칠거 많아서 사용 X  너무 필요한 경우 사용할 예정
{
    private static SafeEventBus _instance;
    public static SafeEventBus Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("SafeEventBus");
                _instance = go.AddComponent<SafeEventBus>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private Dictionary<Type, List<Delegate>> _subs = new();

    public void Subscribe<T>(Action<T> handler)
    {
        var t = typeof(T);
        if (!_subs.TryGetValue(t, out var list))
        {
            list = new List<Delegate>();
            _subs[t] = list;
        }

        list.Add(handler);
    }

    public void Publish<T>(T message)
    {
        var t = typeof(T);
        if (!_subs.TryGetValue(t, out var list)) return;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            var del = list[i];

            // 🔥 Unity Destroy 체크
            if (del.Target is UnityEngine.Object obj && obj == null)
            {
                list.RemoveAt(i);
                continue;
            }

            ((Action<T>)del).Invoke(message);
        }
    }

    public void Clear()
    {
        _subs.Clear();
    }
}
