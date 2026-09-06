using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogueLogUI : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    
    [SerializeField] private Transform contentRoot;
    [SerializeField] private LogItem logPrefab;
    
    [SerializeField] private Button exitButton;

    private StoryUI storyUI;
    private void OnEnable()
    {
        exitButton.onClick.AddListener(storyUI.CloseLogUI);
    }

    private void OnDisable()
    {
        if (exitButton)
            exitButton.onClick.RemoveListener(storyUI.CloseLogUI);
    }

    public void Open(StoryUI story)
    {
        storyUI = story;
        
        gameObject.SetActive(true);
        Refresh();
        StartCoroutine(MoveToBottomRoutine());
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void Refresh()
    {
        foreach (Transform child in contentRoot)
        {
            Destroy(child.gameObject);
        }

        var logs = StoryManager.Instance.StoryUI.DialogueLogs;

        foreach (var log in logs)
        {
            var item = Instantiate(logPrefab, contentRoot);
            item.Setup(log);
        }
    }

    private IEnumerator MoveToBottomRoutine()
    {
        // 레이아웃 rebuild 대기
        yield return null;

        Canvas.ForceUpdateCanvases();

        scrollRect.verticalNormalizedPosition = 0f;
    }

}
