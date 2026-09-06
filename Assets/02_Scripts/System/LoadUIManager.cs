using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadUIManager : Singleton<LoadUIManager>
{
    [SerializeField] private CanvasGroup fade;
    [SerializeField] private GameObject loadingUI;

    [Header("Loading UI")]
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI percentText;
    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private float followSpeed = 800f;
    
    [Header("Progress Mover")]
    [SerializeField] private RectTransform progressMover;
    [SerializeField] private RectTransform progressStart;
    [SerializeField] private RectTransform progressEnd;
    
    private float currentProgress = 0f;
    private Vector2 targetPos;
    
    private void LateUpdate()
    {
        if (!loadingUI.activeInHierarchy)
            return;

        progressMover.anchoredPosition = Vector2.MoveTowards(progressMover.anchoredPosition, targetPos, followSpeed * Time.deltaTime);
    }
    
    #region Fade

    public IEnumerator FadeOut(float duration = 1f)
    {
        fade.gameObject.SetActive(true);
        fade.blocksRaycasts = true;

        fade.alpha = 0f;
        
        yield return fade.DOFade(1f, duration).SetEase(Ease.OutQuad).SetUpdate(true).WaitForCompletion();
    }

    public IEnumerator FadeIn(float duration = 1f)
    {
        fade.alpha = 1f;
        yield return fade.DOFade(0f, duration).SetEase(Ease.InQuad).SetUpdate(true).WaitForCompletion();

        fade.blocksRaycasts = false;
        fade.gameObject.SetActive(false);
    }
    
    public void SetFadeImmediate(bool black)
    {
        fade.alpha = black ? 1 : 0;
        fade.blocksRaycasts = black;
        fade.gameObject.SetActive(true);
    }

    #endregion

    #region Loading UI

    public void ShowLoading()
    {
        loadingUI.SetActive(true);
        
        currentProgress = 0f;
        UpdateUI(0f);
        
        progressMover.anchoredPosition = progressStart.anchoredPosition;
        targetPos = progressStart.anchoredPosition;
    }

    public void HideLoading()
    {
        loadingUI.SetActive(false);
    }

    public void SetProgress(float target)
    {
        // 🔥 Lerp 적용 (부드럽게)
        currentProgress = Mathf.Lerp(currentProgress, target, Time.deltaTime * 5f);
        UpdateUI(currentProgress);
    }

    private void UpdateUI(float value)
    {
        progressBar.fillAmount = value;
        percentText.text = $"{(int)(value * 100)}%";
        
        UpdateProgressMover(value);
        
        if (value < 0.3f)
            tipText.text = "객잔을 정리하는 중...";
        else if (value < 0.7f)
            tipText.text = "손님 맞을 준비 중...";
        else
            tipText.text = "가게 문을 여는 중...";
    }
    
    public void ForceProgress(float value)
    {
        currentProgress = value;
        UpdateUI(value);
        
        progressMover.anchoredPosition = targetPos;
    }
    
    private void UpdateProgressMover(float value)
    {
        targetPos  = Vector2.Lerp(progressStart.anchoredPosition, progressEnd.anchoredPosition, value);
    }
    #endregion
}
