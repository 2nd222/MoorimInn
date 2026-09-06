using System;
using DG.Tweening;
using UnityEngine;

public class TitleMenuAnimator : MonoBehaviour
{
    [SerializeField] private RectTransform logo;
    [SerializeField] private CanvasGroup logoGroup;

    [SerializeField] private RectTransform buttonGroup;
    [SerializeField] private CanvasGroup buttonCanvas;

    [SerializeField] private float buttonMove = 60f;

    private Vector2 logoOriginPos;
    private Vector2 buttonOriginPos;

    private void Awake()
    {
        logoOriginPos = logo.anchoredPosition;
        buttonOriginPos = buttonGroup.anchoredPosition;
    }
    
    private void Start()
    {
        logo.gameObject.SetActive(false);
        buttonGroup.gameObject.SetActive(false);
    }

    public void Play()
    {
        logoGroup.alpha = 0;
        buttonCanvas.alpha = 0;

        Vector2 buttonPos = buttonGroup.anchoredPosition;
        buttonGroup.anchoredPosition = buttonPos - Vector2.up * buttonMove;

        logo.gameObject.SetActive(true);
        buttonGroup.gameObject.SetActive(true);
        
        
        UIAnimationManager.Instance.ShowBounce(logo, null, () =>
        {
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(0.5f);
            
            seq.Append(buttonCanvas.DOFade(1, 1f));
            seq.Join(buttonGroup.DOAnchorPos(buttonPos, 1f).SetEase(Ease.OutCubic));
        });
    }
    
    public void ShowImmediate()
    {
        logo.DOKill();
        buttonGroup.DOKill();

        logo.gameObject.SetActive(true);
        buttonGroup.gameObject.SetActive(true);

        logoGroup.alpha = 1f;
        buttonCanvas.alpha = 1f;

        logo.anchoredPosition = logoOriginPos;
        buttonGroup.anchoredPosition = buttonOriginPos;
    }
}
