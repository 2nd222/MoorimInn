using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ScrollMenu : MonoBehaviour
{
    [SerializeField] private RectTransform paper;
    [SerializeField] private RectTransform bottomBar;
    [SerializeField] private GameObject contentRoot;
    
    [SerializeField] private List<RectTransform> buttons;
    [SerializeField] private float spacing = 70f;
    [SerializeField] private float firstButtonOffset = 25f;

    [SerializeField] private float openHeight = 500f;
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private float buttonDuration = 0.15f;
    
    [SerializeField] private ToolTipUI toolTipUI;

    private float bottomClosedY;
    private bool isOpen;

    private void Awake()
    {
        bottomClosedY = bottomBar.anchoredPosition.y;

        Vector2 size = paper.sizeDelta;
        size.y = 0;
        paper.sizeDelta = size;

        bottomBar.anchoredPosition = new Vector2(bottomBar.anchoredPosition.x, bottomClosedY);

        contentRoot.SetActive(false);
    }

    public void Toggle()
    {
        if (isOpen)
            Close();
        else
            Open();
        
        toolTipUI.Hide();
    }

    private void Open()
    {
        if (isOpen)
            return;

        isOpen = true;

        contentRoot.SetActive(true);

        paper.DOKill();
        bottomBar.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Join(
            DOTween.To(
                () => paper.sizeDelta.y,
                y =>
                {
                    Vector2 size = paper.sizeDelta;
                    size.y = y;
                    paper.sizeDelta = size;
                    bottomBar.anchoredPosition = new Vector2(bottomBar.anchoredPosition.x, bottomClosedY);
                },
                openHeight,
                duration
            )
        );
        
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].gameObject.SetActive(true);

            // 시작 위치
            Vector2 pos = buttons[i].anchoredPosition;
            pos.y = -firstButtonOffset;
            buttons[i].anchoredPosition = pos;
            
            float targetY = -(i + 1) * spacing;
            targetY -= firstButtonOffset;

            buttons[i].DOAnchorPosY(targetY, buttonDuration).SetEase(Ease.OutBack).SetDelay(i * 0.03f);
        }

        seq.Join(bottomBar.DOAnchorPosY(bottomClosedY - openHeight, duration));
    }

    private void Close()
    {
        if (!isOpen)
            return;

        isOpen = false;

        paper.DOKill();
        bottomBar.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Join(DOTween.To(
                () => paper.sizeDelta.y,
                y =>
                {
                    Vector2 size = paper.sizeDelta;
                    size.y = y;
                    paper.sizeDelta = size;
                },
                0,
                duration
            )
        );

        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i;

            buttons[i]
                .DOAnchorPosY(-firstButtonOffset, buttonDuration)
                .OnComplete(() =>
                {
                    buttons[index].gameObject.SetActive(false);
                });
        }
        
        seq.Join(bottomBar.DOAnchorPosY(bottomClosedY, duration));

        seq.OnComplete(() =>
        {
            contentRoot.SetActive(false);
        });
    }
}
