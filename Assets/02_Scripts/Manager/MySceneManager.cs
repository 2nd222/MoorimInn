using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : Singleton<MySceneManager>
{
    public void LoadSceneWithCallback<TargetType>(string sceneName, Action<TargetType> onSceneLoad) where TargetType : MonoBehaviour
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        asyncLoad.completed += (operation) =>
        {
            TargetType script = FindAnyObjectByType<TargetType>();

            if (script != null)
            {
                onSceneLoad?.Invoke(script);
            }
            else
            {
                Debug.LogWarning($"{sceneName} 씬에서 {typeof(TargetType).Name} 를 찾지 못함");
            }
        };
    }

    //찾는 스크립트 없는 버전
    public void LoadSceneWithCallback(string sceneName, Action onSceneLoad = null)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        asyncLoad.completed += (operation) =>
        {
            onSceneLoad?.Invoke();
        };
    }
}
