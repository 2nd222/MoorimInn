using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : Singleton<SceneLoader>
{
    public void LoadScene(string sceneName, Action onSceneLoaded)
    {
        Time.timeScale = 1f;
        
        StartCoroutine(LoadRoutine(sceneName, onSceneLoaded));
    }

    private IEnumerator LoadRoutine(string sceneName, Action onSceneLoaded)
    {
        float minTime = 3f;
        float timer = 0f;

        // 1️ Fade Out
        yield return LoadUIManager.Instance.FadeOut();
        yield return new WaitForSeconds(0.2f);
        // 2️ Loading UI 표시
        LoadUIManager.Instance.ShowLoading();
        yield return LoadUIManager.Instance.FadeIn();
        

        // 3️ 씬 로딩 시작
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        float sceneProgress = 0f;

        while (!op.isDone)
        {
            // 🔹 씬 로딩 progress
            sceneProgress = Mathf.Clamp01(op.progress / 0.9f);

            // 시간 기반 + 실제 progress 섞기
            float timeProgress = timer / minTime;

            float finalProgress = Mathf.Min(sceneProgress, timeProgress);

            LoadUIManager.Instance.SetProgress(finalProgress);

            timer += Time.deltaTime;

            // 완료 조건
            if (sceneProgress >= 1f && timer >= minTime)
            {
                LoadUIManager.Instance.ForceProgress(1f);
                yield return new WaitForSeconds(0.5f);
                op.allowSceneActivation = true;
            }

            yield return null;
        }
        yield return null;
        
        onSceneLoaded?.Invoke();

        // 4️ 종료 연출
        yield return LoadUIManager.Instance.FadeOut();
        yield return new WaitForSeconds(0.2f);
        LoadUIManager.Instance.HideLoading();
        yield return LoadUIManager.Instance.FadeIn();
    }
}
