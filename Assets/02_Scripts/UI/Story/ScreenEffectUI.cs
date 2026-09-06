using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenEffectUI : MonoBehaviour
{
    [SerializeField] private RectTransform screenRoot;
    [SerializeField] private Image fadeImage;
    [SerializeField] private Image blurImage;
    [SerializeField] private Volume volume; // 하이어라키 창에서 global volume 추가 및 연결 필요
    
    private Vector2 defaultPos;
    private Vector3 defaultScale;
    
    private Vignette vignette;
    
    private void Awake()
    {
        volume.profile.TryGet(out vignette);
        defaultPos = screenRoot.anchoredPosition;
        defaultScale = screenRoot.localScale;
    }

    private void OnEnable()
    {
        StoryManager.Instance.SetScreenEffectUI(this);
    }

    #region Fade Effect

    public void FadeBlack(float duration)
    {
        fadeImage.gameObject.SetActive(true);

        fadeImage.color = new Color(0,0,0,0);

        fadeImage.DOFade(1f, duration).SetEase(Ease.OutQuad);
    }

    public void FadeFromBlack(float duration)
    {
        fadeImage.color = new Color(0,0,0,1);

        fadeImage
            .DOFade(0f, duration)
            .OnComplete(() =>
            {
                fadeImage.gameObject.SetActive(false);
            }).SetEase(Ease.InQuad);
    }

    public void FlashWhite(float duration = 0.75f)
    {
        fadeImage.DOKill();
        fadeImage.gameObject.SetActive(true);
        // 시작은 투명
        fadeImage.color = new Color(1f, 1f, 1f, 0f);

        Sequence seq = DOTween.Sequence();

        seq.Append(fadeImage.DOFade(1f, duration * 0.2f).SetEase(Ease.OutQuad));
        seq.Append(fadeImage.DOFade(0f, duration * 0.8f).SetEase(Ease.InQuad));

        seq.OnComplete(() =>
        {
            fadeImage.gameObject.SetActive(false);
        });
    }

    public IEnumerator FlashWhiteRoutine(float duration, float midTerm, Action onMid = null)
    {
        fadeImage.DOKill();
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(1,1,1,0);

        yield return fadeImage.DOFade(1f, duration * 0.2f).SetEase(Ease.OutQuad).WaitForCompletion();

        yield return new WaitForSeconds(midTerm);
        onMid?.Invoke();
        
        yield return fadeImage.DOFade(0f, duration * 0.8f).SetEase(Ease.InQuad).WaitForCompletion();

        fadeImage.gameObject.SetActive(false);
    }
    
    public void FlashBlack(float duration = 0.5f)
    {
        fadeImage.DOKill();

        fadeImage.gameObject.SetActive(true);

        // 검은색 + 투명 시작
        fadeImage.color = new Color(0f, 0f, 0f, 0f);

        Sequence seq = DOTween.Sequence();

        // 빠르게 검게
        seq.Append(fadeImage.DOFade(1f, duration * 0.3f).SetEase(Ease.OutQuad));

        seq.AppendInterval(duration * 0.4f);
            
        // 다시 복귀
        seq.Append(fadeImage.DOFade(0f, duration * 0.3f).SetEase(Ease.InQuad));

        seq.OnComplete(() =>
        {
            fadeImage.gameObject.SetActive(false);
        });
    }
    
    public IEnumerator FlashBlackRoutine(
        float duration,
        float holdTime,
        Action onMid = null)
    {
        fadeImage.DOKill();

        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0f, 0f, 0f, 0f);

        // 검게
        yield return fadeImage
            .DOFade(1f, duration * 0.3f)
            .SetEase(Ease.OutQuad)
            .WaitForCompletion();

        // 여기서 장면 교체
        onMid?.Invoke();

        yield return new WaitForSeconds(holdTime);

        // 다시 밝게
        yield return fadeImage
            .DOFade(0f, duration * 0.7f)
            .SetEase(Ease.InQuad)
            .WaitForCompletion();

        fadeImage.gameObject.SetActive(false);
    }
    
    public void SetBlackImmediate(bool active)
    {
        fadeImage.gameObject.SetActive(active);
        
        Color c = fadeImage.color;
        c.r = 0f;
        c.g = 0f;
        c.b = 0f;
        c.a = active ? 1f : 0f;

        fadeImage.color = c;
    }
    
    #endregion

    #region Shake Effect

    public void ShakeScreen(float power = 30f, float duration = 0.3f)
    {
        screenRoot.DOComplete();

        screenRoot.DOShakeAnchorPos(
            duration,
            power,
            20,
            90,
            false,
            true
        );
    }

    #endregion

    #region Zoom Effect

    public void ZoomTo(Vector2 targetPos, float scale, float duration)
    {
        screenRoot.DOKill();

        Vector2 focusPos =
            -targetPos * (scale - 1f);
        
        Sequence seq = DOTween.Sequence();

        seq.Join(screenRoot.DOScale(scale, duration).SetEase(Ease.OutCubic));

        seq.Join(screenRoot.DOAnchorPos(focusPos, duration).SetEase(Ease.OutCubic));
    }

    public void ResetZoom(float duration)
    {
        screenRoot.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Join(screenRoot.DOScale(defaultScale, duration).SetEase(Ease.OutCubic));

        seq.Join(screenRoot.DOAnchorPos(defaultPos, duration).SetEase(Ease.OutCubic));
    }

    #endregion

    #region Vignette Effect

    public void SetVignette(float intensity, float duration)
    {
        DOTween.To(
            () => vignette.intensity.value,
            x => vignette.intensity.value = x,
            intensity,
            duration
        );
    }

    #endregion

    #region Blur Effect

    public void FadeBlur(float duration)
    {
        blurImage.DOKill();

        blurImage.gameObject.SetActive(true);

        Color c = blurImage.color;
        c.a = 0f;
        blurImage.color = c;

        blurImage.DOFade(1f, duration).SetEase(Ease.InSine);
    }

    public void FadeFromBlur(float duration)
    {
        blurImage.DOKill();

        blurImage.DOFade(0f, duration)
            .OnComplete(() =>
            {
                blurImage.gameObject.SetActive(false);
            }).SetEase(Ease.InSine);
    }

    #endregion
    
    #region Combined Effect

    public void PlayImpact(
        float shakePower,
        float shakeDuration,
        bool useFlash)
    {
        if (useFlash)
            FlashWhite();

        ShakeScreen(shakePower, shakeDuration);

        SoundManager.Instance.PlaySFX("Hit");
    }
    #endregion
}
