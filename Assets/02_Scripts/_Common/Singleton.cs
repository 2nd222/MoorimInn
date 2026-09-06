using UnityEngine;

public class Singleton<T> : MonoBehaviour where T: MonoBehaviour // where T: monobehavior T에 대한 조건 monobehavior여야함
{
    // 다른 클래스에서 상속받아서 사용하면됨
    private static T instance;
    private static bool isQuitting = false;
    public static T Instance
    {
        get
        {
            if (isQuitting)
                return null;

            if (instance == null)
            {
                instance = FindFirstObjectByType<T>();

                if (instance == null)
                {
                    // GameObject go = new GameObject(typeof(T).Name);
                    // instance = go.AddComponent<T>();
                }
            }
            return instance;
        }
    }
    
    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.Log($"중복 제거: {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        instance = this as T;
        DontDestroyOnLoad(gameObject);
    }
    
    protected virtual void OnApplicationQuit()
    {
        if (instance == this)
        {
            isQuitting = true;
        }
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            isQuitting = true;
        }
    }
}
