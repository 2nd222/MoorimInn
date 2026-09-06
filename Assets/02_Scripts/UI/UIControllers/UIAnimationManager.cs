using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.UI;

public enum Direction
{
    Top,
    Bottom,
    Left,
    Right
}
public enum PaperDirection
{
    TopLeft,      // ���� ��
    TopRight,     // ������ ��
    BottomLeft,   // ���� �Ʒ�
    BottomRight   // ������ �Ʒ�
}

public class UIAnimationManager : Singleton<UIAnimationManager>
{
    [Header("=== ���� Ŀ���� ���� ===")]
    [Header("Show ����")]
    [Range(0.1f, 1f)]
    [SerializeField]
    private float popShowDuration = 0.3f;
    [Range(0f, 1f)]
    [SerializeField]
    private float popStartScale = 0.5f;
    [SerializeField]
    private Ease popShowEase = Ease.OutBack;

    [Header("Hide ����")]
    [Range(0.1f, 1f)]
    [SerializeField]
    private float popHideDuration = 0.2f;
    [Range(0f, 1f)]
    [SerializeField]
    private float popEndScale = 0.0f;
    [SerializeField]
    private Ease popHideEase = Ease.InBack;

    [Space(30)]

    [Header("=== ��ǳ�� ��ġ�� ���� ===")]
    [Header("Show ����")]
    [Range(0.1f, 1f)]
    [SerializeField]
    private float orderShowDuration = 0.3f;
    [SerializeField]
    private Ease orderShowEase = Ease.OutBack;

    [Header("Hide ����")]
    [Range(0.1f, 1f)]
    [SerializeField]
    private float orderHideDuration = 0.2f;
    [SerializeField]
    private Ease orderHideEase = Ease.InBack;

    [Space(30)]

    [Header("=== ���� ��ġ�� ���� ===")]
    [Header("Show ����")]
    [Range(0.1f, 1f)]
    [SerializeField]
    private float vUnfoldShowDuration = 0.3f;
    [SerializeField]
    private Ease vUnfoldShowEase = Ease.OutBack;

    [Header("Hide ����")]
    [Range(0.1f, 1f)]
    [SerializeField]
    private float vUnfoldHideDuration = 0.2f;
    [SerializeField]
    private Ease vUnfoldHideEase = Ease.InBack;

    [Space(30)]

    [Header("=== ���� ��ġ�� ���� ===")]
    [Header("Show ����")]
    [Range(0.1f, 1f)]
    [SerializeField]
    private float hUnfoldShowDuration = 0.3f;
    [SerializeField]
    private Ease hUnfoldShowEase = Ease.OutBack;

    [Header("Hide ����")]
    [Range(0.1f, 1f)]
    [SerializeField]
    private float hUnfoldHideDuration = 0.2f;
    [SerializeField]
    private Ease hUnfoldHideEase = Ease.InBack;

    [Space(30)]

    [Header("=== ���� ��ġ�� ���� ===")]
    [Header("Show ����")]
    [Range(0.1f, 1f)]
    [SerializeField]
    private float paperShowDuration = 0.5f;
    [SerializeField]
    private Ease paperShowEase = Ease.OutQuart;

    [Header("Hide ����")]
    [Range(0.1f, 1f)]
    [SerializeField]
    private float paperHideDuration = 0.3f;
    [SerializeField]
    private Ease paperHideEase = Ease.InQuart;

    [Space(30)]

    [Header("=== ������ ������ ���� Ƣ�� ���� ===")]
    [Header("Show ����")]
    [Range(0.1f, 2f)]
    [SerializeField] 
    private float bouncePopShowDuration = 0.8f;
    [SerializeField]
    private Ease bouncePopShowEase = Ease.OutBounce;

    [Header("Hide ����")]
    [Range(0.1f, 1f)]
    [SerializeField]
    private float bounceHideDuration = 0.3f;
    [SerializeField]
    private Ease bounceHideEase = Ease.InBack;

    [Tooltip("���� ��ġ���� �󸶳� ���ʿ��� �������� ����")]
    [SerializeField] 
    private float bouncePopStartOffset = 1000f;

    [Space(30)]

    [Header("=== ������� �ٿ ���� ===")]
    [Header("Show ����")]
    [Range(0.5f, 2f)]
    [SerializeField] 
    private float zigzagShowDuration = 1.2f; // ƨ��� �̵��� ����� �ð�

    [Header("Hide ����")]
    [Range(0.1f, 1f)]
    [SerializeField]
    private float zigzagHideDuration = 0.3f;
    [SerializeField]
    private Ease zigzagHideEase = Ease.InBack;

    [Tooltip("������ �� ���� ��")]
    [SerializeField] 
    private float zigzagDropHeight = 800f;
    [Tooltip("������ �� ���� ��")]
    [SerializeField] 
    private float zigzagLeftOffset = 400f;
    [Tooltip("���ʿ��� ƨ�� �� ������ ��")]
    [SerializeField] 
    private float zigzagRightOffset = 200f;

    [Space(30)]

    [Header("=== Stamp ���� ===")]
    [Header("Show ����")]
    [SerializeField] 
    private Vector2 stampStartScale = new Vector2(10f, 10f); 
    [SerializeField] 
    private float stampAnimDuration = 0.5f;
    [SerializeField] 
    private Ease stampEase = Ease.OutBack;
    
    private readonly Dictionary<RectTransform, Vector2> popHomePos = new();


    protected override void Awake()
    {
        base.Awake();

        DOTween.Init();
    }
    public void ShowPop(RectTransform ui, UnityAction onStart = null, UnityAction onComplete = null)
    {
        if (ui == null) return;

        // 1. ���� Ʈ�� ����
        ui.DOKill();

        CanvasGroup cg = ui.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = ui.gameObject.AddComponent<CanvasGroup>();

        cg.DOKill();

        cg.blocksRaycasts = false;

        ui.localScale = Vector3.one * this.popStartScale;

        cg.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        // ������ ��
        seq.OnStart(() => {
            onStart?.Invoke();
        });

        seq.Append(ui.DOScale(Vector3.one, this.popShowDuration).SetEase(this.popShowEase));
        seq.Join(cg.DOFade(1f, this.popShowDuration).SetEase(Ease.Linear));

        // ���� ��
        seq.OnComplete(() => {
            cg.blocksRaycasts = true;
            onComplete?.Invoke();
        });
    }
    public void HidePop(RectTransform ui, UnityAction onStart = null, UnityAction onComplete = null)
    {
        if (ui == null) return;

        ui.DOKill();

        CanvasGroup cg = ui.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = ui.gameObject.AddComponent<CanvasGroup>();

        cg.DOKill();

        cg.blocksRaycasts = false;

        Sequence seq = DOTween.Sequence();
        // ������ ��
        seq.OnStart(() => {
            onStart?.Invoke();
        });

        seq.Append(ui.DOScale(Vector3.one * this.popEndScale, this.popHideDuration).SetEase(this.popHideEase));
        seq.Join(cg.DOFade(0f, this.popHideDuration).SetEase(Ease.Linear));

        // ���� ��
        seq.OnComplete(() => {
            onComplete?.Invoke();
        });
    }
    public void ShowOrder(RectTransform ui, Direction direction = Direction.Right, UnityAction onStart = null, UnityAction onComplete = null)
    {
        if (ui == null) return;

        ui.DOKill();

        CanvasGroup cg = ui.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = ui.gameObject.AddComponent<CanvasGroup>();

        cg.DOKill();
        cg.blocksRaycasts = false;

        bool isVertical = (direction == Direction.Top || direction == Direction.Bottom);

        ui.localScale = isVertical ? new Vector3(1f, 0f, 1f) : new Vector3(0f, 1f, 1f);

        cg.alpha = 0f;

        Sequence seq = DOTween.Sequence();

        Vector2 startPivot = new Vector2(0.5f, 0.5f);
        switch (direction)
        {
            case Direction.Top: startPivot = new Vector2(0.5f, 1f); break; // ���� ����
            case Direction.Bottom: startPivot = new Vector2(0.5f, 0f); break; // �Ʒ��� ����
            case Direction.Left: startPivot = new Vector2(0f, 0.5f); break; // ���� ����
            case Direction.Right: startPivot = new Vector2(1f, 0.5f); break; // ������ ����
        }

        // ������ ��
        seq.OnStart(() => {
            SetPivot(ui, startPivot);
            onStart?.Invoke();
        });

        if (isVertical)
        {
            seq.Append(ui.DOScaleY(1f, this.orderShowDuration).SetEase(this.orderShowEase));
        }
        else
        {
            seq.Append(ui.DOScaleX(1f, this.orderShowDuration).SetEase(this.orderShowEase));
        }

        seq.Join(cg.DOFade(1f, this.orderShowDuration).SetEase(Ease.Linear));

        // ���� ��
        seq.OnComplete(() => {
            SetPivot(ui, new Vector2(0.5f, 0.5f));
            cg.blocksRaycasts = true;
            onComplete?.Invoke();
        });
    }
    public void HideOrder(RectTransform ui, Direction direction = Direction.Right, UnityAction onStart = null, UnityAction onComplete = null)
    {
        if (ui == null) return;

        ui.DOKill();

        CanvasGroup cg = ui.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = ui.gameObject.AddComponent<CanvasGroup>();

        cg.DOKill();
        cg.blocksRaycasts = false;

        bool isVertical = (direction == Direction.Top || direction == Direction.Bottom);

        Sequence seq = DOTween.Sequence();

        Vector2 startPivot = new Vector2(0.5f, 0.5f);
        switch (direction)
        {
            case Direction.Top: startPivot = new Vector2(0.5f, 1f); break;
            case Direction.Bottom: startPivot = new Vector2(0.5f, 0f); break;
            case Direction.Left: startPivot = new Vector2(0f, 0.5f); break;
            case Direction.Right: startPivot = new Vector2(1f, 0.5f); break;
        }

        // ������ ��
        seq.OnStart(() => {
            SetPivot(ui, startPivot);
            onStart?.Invoke();
        });

        if (isVertical)
        {
            seq.Append(ui.DOScaleY(0f, this.orderHideDuration).SetEase(this.orderHideEase));
        }
        else
        {
            seq.Append(ui.DOScaleX(0f, this.orderHideDuration).SetEase(this.orderHideEase));
        }

        seq.Join(cg.DOFade(0f, this.orderHideDuration).SetEase(Ease.Linear));

        seq.OnComplete(() => {
            SetPivot(ui, new Vector2(0.5f, 0.5f));
            onComplete?.Invoke();
        });
    }
    public void ShowVerticalUnfold(RectTransform ui, UnityAction onStart = null, UnityAction onComplete = null)
    {
        if (ui == null) return;

        ui.DOKill();

        CanvasGroup cg = ui.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = ui.gameObject.AddComponent<CanvasGroup>();

        cg.DOKill();

        cg.blocksRaycasts = false;

        ui.localScale = new Vector3(1f, 0f, 1f);
        cg.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        // ������ ��
        seq.OnStart(() => {
            onStart?.Invoke();
        });

        seq.Append(ui.DOScaleY(1f, this.vUnfoldShowDuration).SetEase(this.vUnfoldShowEase));
        seq.Join(cg.DOFade(1f, this.vUnfoldShowDuration).SetEase(Ease.Linear));

        // ���� ��
        seq.OnComplete(() => {
            cg.blocksRaycasts = true;
            onComplete?.Invoke();
        });
    }
    public void HideVerticalUnfold(RectTransform ui, UnityAction onStart = null, UnityAction onComplete = null)
    {
        if (ui == null) return;

        ui.DOKill();

        CanvasGroup cg = ui.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = ui.gameObject.AddComponent<CanvasGroup>();

        cg.DOKill();

        cg.blocksRaycasts = false;

        Sequence seq = DOTween.Sequence();
        // ������ ��
        seq.OnStart(() => {
            onStart?.Invoke();
        });

        seq.Append(ui.DOScaleY(0f, this.vUnfoldHideDuration).SetEase(this.vUnfoldHideEase));
        seq.Join(cg.DOFade(0f, this.vUnfoldHideDuration).SetEase(Ease.Linear));

        // ���� ��
        seq.OnComplete(() => {
            onComplete?.Invoke();
        });
    }
    public void ShowHorizontalUnfold(RectTransform ui, UnityAction onStart = null, UnityAction onComplete = null)
    {
        if (ui == null) return;

        ui.DOKill();

        CanvasGroup cg = ui.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = ui.gameObject.AddComponent<CanvasGroup>();
        cg.DOKill();

        cg.blocksRaycasts = false;

        ui.localScale = new Vector3(0f, 1f, 1f);
        cg.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        // ������ ��
        seq.OnStart(() => {
            onStart?.Invoke();
        });

        seq.Append(ui.DOScaleX(1f, this.hUnfoldShowDuration).SetEase(this.hUnfoldShowEase));
        seq.Join(cg.DOFade(1f, this.hUnfoldShowDuration).SetEase(Ease.Linear));

        // ���� ��
        seq.OnComplete(() => {
            cg.blocksRaycasts = true;
            onComplete?.Invoke();
        });
    }
    public void HideHorizontalUnfold(RectTransform ui, UnityAction onStart = null, UnityAction onComplete = null)
    {
        if (ui == null) return;

        ui.DOKill();

        CanvasGroup cg = ui.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = ui.gameObject.AddComponent<CanvasGroup>();

        cg.DOKill();

        cg.blocksRaycasts = false;

        Sequence seq = DOTween.Sequence();
        // ������ ��
        seq.OnStart(() => {
            onStart?.Invoke();
        });

        seq.Append(ui.DOScaleX(0f, this.hUnfoldHideDuration).SetEase(this.hUnfoldHideEase));
        seq.Join(cg.DOFade(0f, this.hUnfoldHideDuration).SetEase(Ease.Linear));

        // ���� ��
        seq.OnComplete(() => {
            onComplete?.Invoke();
        });
    }
    public void ShowPaper(RectTransform ui, PaperDirection direction = PaperDirection.TopRight, UnityAction onStart = null, UnityAction onComplete = null)
    {
        if (ui == null) return;

        ui.DOKill();

        CanvasGroup cg = ui.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = ui.gameObject.AddComponent<CanvasGroup>();

        cg.DOKill();
        cg.blocksRaycasts = false;

        ui.localScale = Vector3.zero;
        cg.alpha = 0f;

        Sequence seq = DOTween.Sequence();

        Vector2 startPivot = GetPivotFromDirection(direction);

        // ������ ��
        seq.OnStart(() => {
            SetPivot(ui, startPivot); 
            onStart?.Invoke();
        });

        seq.Append(ui.DOScale(Vector3.one, this.paperShowDuration).SetEase(this.paperShowEase));
        seq.Join(cg.DOFade(1f, this.paperShowDuration).SetEase(Ease.Linear));

        seq.OnComplete(() => {
            SetPivot(ui, new Vector2(0.5f, 0.5f)); 
            cg.blocksRaycasts = true;
            onComplete?.Invoke();
        });
    }
    public void HidePaper(RectTransform ui, PaperDirection direction = PaperDirection.TopRight, UnityAction onStart = null, UnityAction onComplete = null)
    {
        if (ui == null) return;

        ui.DOKill();

        CanvasGroup cg = ui.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = ui.gameObject.AddComponent<CanvasGroup>();

        cg.DOKill();
        cg.blocksRaycasts = false;

        Sequence seq = DOTween.Sequence();

        Vector2 startPivot = GetPivotFromDirection(direction);

        // ������ ��
        seq.OnStart(() => {
            SetPivot(ui, startPivot);
            onStart?.Invoke();
        });

        seq.Append(ui.DOScale(Vector3.zero, this.paperHideDuration).SetEase(this.paperHideEase));
        seq.Join(cg.DOFade(0f, this.paperHideDuration).SetEase(Ease.Linear));

        // ���� ��
        seq.OnComplete(() => {
            SetPivot(ui, new Vector2(0.5f, 0.5f)); 
            onComplete?.Invoke();
        });
    }
    public void ShowBounce(RectTransform ui, UnityAction onStart = null, UnityAction onComplete = null)
    {
        if (ui == null) return;

        ui.DOKill();

        CanvasGroup cg = ui.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = ui.gameObject.AddComponent<CanvasGroup>();

        cg.DOKill();

        cg.blocksRaycasts = false;

        Vector2 targetPos = ui.anchoredPosition;

        ui.anchoredPosition = new Vector2(targetPos.x, targetPos.y + this.bouncePopStartOffset);

        ui.localScale = Vector3.one;

        cg.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        // ������ ��
        seq.OnStart(() => {
            onStart?.Invoke();
        });

        seq.Append(ui.DOAnchorPos(targetPos, this.bouncePopShowDuration).SetEase(this.bouncePopShowEase));
        seq.Join(cg.DOFade(1f, this.bouncePopShowDuration * 0.5f).SetEase(Ease.Linear));

        seq.OnComplete(() => {
            cg.blocksRaycasts = true;
            onComplete?.Invoke();
        });
    }
    public void HideBounce(RectTransform ui, UnityAction onStart = null, UnityAction onComplete = null)
    {
        if (ui == null) return;

        ui.DOKill();

        CanvasGroup cg = ui.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = ui.gameObject.AddComponent<CanvasGroup>();

        cg.DOKill();

        cg.blocksRaycasts = false;

        Vector2 originalPos = ui.anchoredPosition;

        Vector2 endPos = new Vector2(ui.anchoredPosition.x, ui.anchoredPosition.y + this.zigzagDropHeight);

        Sequence seq = DOTween.Sequence();
        // ������ ��
        seq.OnStart(() => {
            onStart?.Invoke();
        });

        seq.Append(ui.DOAnchorPosY(endPos.y, this.zigzagHideDuration).SetEase(this.bounceHideEase));
        seq.Join(cg.DOFade(0f, this.zigzagHideDuration).SetEase(Ease.Linear));

        // ���� ��
        seq.OnComplete(() => {
            ui.anchoredPosition = originalPos;
            onComplete?.Invoke();
        });
    }
    public void ShowZigzag(RectTransform ui, UnityAction onStart = null, UnityAction onComplete = null)
    {
        if (ui == null) return;

        ui.DOKill();

        CanvasGroup cg = ui.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = ui.gameObject.AddComponent<CanvasGroup>();

        cg.DOKill();

        cg.blocksRaycasts = false;

        Vector2 targetPos = ui.anchoredPosition;

        ui.anchoredPosition = new Vector2(targetPos.x - this.zigzagLeftOffset, targetPos.y + this.zigzagDropHeight);
        ui.localScale = Vector3.one;
        cg.alpha = 0f;

        ui.localEulerAngles = Vector3.zero;

        Sequence seq = DOTween.Sequence();
        // ������ ��
        seq.OnStart(() => {
            onStart?.Invoke();
        });

        float dropTime = this.zigzagShowDuration * 0.25f;
        float bounceTime1 = this.zigzagShowDuration * 0.35f;
        float bounceTime2 = this.zigzagShowDuration * 0.20f;
        float settleTime = this.zigzagShowDuration * 0.20f;

        seq.Join(cg.DOFade(1f, dropTime).SetEase(Ease.Linear));


        seq.Append(ui.DOAnchorPos(new Vector2(targetPos.x - this.zigzagLeftOffset, targetPos.y),
                                    dropTime).SetEase(Ease.InQuad));
        seq.Join(ui.DOLocalRotate(new Vector3(0, 0, 15f), dropTime).SetEase(Ease.InOutQuad));

        float jumpHeight1 = this.zigzagDropHeight * 0.25f;
        seq.Append(ui.DOJumpAnchorPos(new Vector2(targetPos.x + this.zigzagRightOffset, targetPos.y),
                                        jumpHeight1, 1, bounceTime1).SetEase(Ease.Linear));
        seq.Join(ui.DOLocalRotate(new Vector3(0, 0, -15f), bounceTime1).SetEase(Ease.InOutQuad));

        float jumpHeight2 = this.zigzagDropHeight * 0.08f;
        seq.Append(ui.DOJumpAnchorPos(targetPos, jumpHeight2, 1, bounceTime2).SetEase(Ease.Linear));

        seq.Join(ui.DOLocalRotate(new Vector3(0, 0, 15f), bounceTime2).SetEase(Ease.InOutQuad));

        seq.Append(ui.DOLocalRotate(Vector3.zero, settleTime).SetEase(Ease.OutQuad));

        // ���� ��
        seq.OnComplete(() => {
            cg.blocksRaycasts = true;
            onComplete?.Invoke();
        });
    }
    public void HideZigzag(RectTransform ui, UnityAction onStart = null, UnityAction onComplete = null)
    {
        if (ui == null) return;

        ui.DOKill();

        CanvasGroup cg = ui.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = ui.gameObject.AddComponent<CanvasGroup>();

        cg.DOKill();

        cg.blocksRaycasts = false;

        Sequence seq = DOTween.Sequence();
        // ������ ��
        seq.OnStart(() =>
        {
            onStart?.Invoke();
        });

        seq.Append(ui.DOScale(Vector3.one * this.bouncePopStartOffset, this.zigzagHideDuration).SetEase(this.zigzagHideEase));
        seq.Join(cg.DOFade(0f, this.zigzagHideDuration).SetEase(Ease.Linear));

        seq.OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }
    public void ShowStamp(RectTransform ui, UnityAction onStart = null, UnityAction onComplete = null)
    {
        if (ui == null) return;

        ui.DOKill();

        CanvasGroup cg = ui.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = ui.gameObject.AddComponent<CanvasGroup>();

        cg.DOKill();
        cg.blocksRaycasts = false;

        ui.localScale = new Vector3(this.stampStartScale.x, this.stampStartScale.y, 1f);
        cg.alpha = 0f;

        Sequence seq = DOTween.Sequence();

        // ������ ��
        seq.OnStart(() => {
            onStart?.Invoke();
        });

        seq.Append(ui.DOScale(Vector3.one, this.stampAnimDuration).SetEase(this.stampEase));
        seq.Join(cg.DOFade(1f, this.stampAnimDuration * 0.5f).SetEase(Ease.Linear)); 

        // ���� ��
        seq.OnComplete(() => {
            cg.blocksRaycasts = true;
            onComplete?.Invoke();
        });
    }
    public void ShowSlide(
        RectTransform ui,
        Vector2 basePos,
        Direction direction = Direction.Bottom, 
        float offsetDistance = 800f, 
        float duration = 0.4f, 
        UnityAction onStart = null, 
        UnityAction onComplete = null)
    {
        if (ui == null) return;

        ui.DOKill(true);

        CanvasGroup cg = ui.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = ui.gameObject.AddComponent<CanvasGroup>();

        cg.DOKill();
        cg.blocksRaycasts = false;

        Vector2 startPos = basePos;
        switch (direction)
        {
            case Direction.Top: startPos.y += offsetDistance; break;
            case Direction.Bottom: startPos.y -= offsetDistance; break;
            case Direction.Left: startPos.x -= offsetDistance; break;
            case Direction.Right: startPos.x += offsetDistance; break;
        }

        ui.anchoredPosition = startPos;
        ui.localScale = Vector3.one;
        cg.alpha = 0f;

        Sequence seq = DOTween.Sequence();

        seq.OnStart(() => {
            onStart?.Invoke();
        });

        seq.Append(ui.DOAnchorPos(basePos, duration).SetEase(Ease.OutQuart));
        seq.Join(cg.DOFade(1f, duration).SetEase(Ease.Linear));

        seq.OnComplete(() => {
            cg.blocksRaycasts = true;
            onComplete?.Invoke();
        });
    }

    public void HideSlide(
        RectTransform ui,
        Vector2 basePos,
        Direction direction = Direction.Bottom, 
        float offsetDistance = 800f, 
        float duration = 0.3f, 
        UnityAction onStart = null, 
        UnityAction onComplete = null)
    {
        if (ui == null) return;

        ui.DOKill(true);

        CanvasGroup cg = ui.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = ui.gameObject.AddComponent<CanvasGroup>();

        cg.DOKill();
        cg.blocksRaycasts = false;
        
        Vector2 endPos = basePos;
        switch (direction)
        {
            case Direction.Top: endPos.y += offsetDistance; break;
            case Direction.Bottom: endPos.y -= offsetDistance; break;
            case Direction.Left: endPos.x -= offsetDistance; break;
            case Direction.Right: endPos.x += offsetDistance; break;
        }

        ui.anchoredPosition = basePos;
        
        Sequence seq = DOTween.Sequence();

        seq.OnStart(() => {
            onStart?.Invoke();
        });

        seq.Append(ui.DOAnchorPos(endPos, duration).SetEase(Ease.InQuart));
        seq.Join(cg.DOFade(0f, duration).SetEase(Ease.Linear));

        seq.OnComplete(() => {
            ui.anchoredPosition = basePos;
            onComplete?.Invoke();
        });
    }
    public void ShowGroupSlide(
        RectTransform mainRect, 
        RectTransform subRect,
        Vector2 mainBasePos,
        Vector2 subBasePos,
        Direction direction = Direction.Right, 
        float offsetDistance = 800f, 
        float mainDuration = 0.4f, 
        float subDuration = 0.2f, 
        UnityAction onStart = null, 
        UnityAction onComplete = null)
    {
        if (mainRect == null || subRect == null) return;

        mainRect.DOKill(true);
        subRect.DOKill(true);

        CanvasGroup mainCG = mainRect.GetComponent<CanvasGroup>();
        if (mainCG == null) mainCG = mainRect.gameObject.AddComponent<CanvasGroup>();

        CanvasGroup subCG = subRect.GetComponent<CanvasGroup>();
        if (subCG == null) subCG = subRect.gameObject.AddComponent<CanvasGroup>();

        mainCG.DOKill(true);
        subCG.DOKill(true);

        mainCG.blocksRaycasts = false;
        subCG.blocksRaycasts = false;

        Vector2 mainStartPos = mainBasePos;
        Vector2 subOverlapPos = subBasePos;

        switch (direction)
        {
            case Direction.Top:
            case Direction.Bottom:
                mainStartPos.y += (direction == Direction.Top) ? offsetDistance : -offsetDistance;
                subOverlapPos.y = mainBasePos.y; 
                break;
            case Direction.Left:
            case Direction.Right:
                mainStartPos.x += (direction == Direction.Right) ? offsetDistance : -offsetDistance;
                subOverlapPos.x = mainBasePos.x; 
                break;
        }

        Vector2 subStartPos = subOverlapPos;
        
        if (direction == Direction.Top || direction == Direction.Bottom) subStartPos.y = mainStartPos.y;
        else subStartPos.x = mainStartPos.x;

        mainRect.anchoredPosition = mainStartPos;
        subRect.anchoredPosition = subStartPos;
        
        mainCG.alpha = 0f;
        subCG.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        seq.OnStart(() => { onStart?.Invoke(); });

        seq.Append(mainRect.DOAnchorPos(mainBasePos, mainDuration).SetEase(Ease.OutQuart));
        seq.Join(subRect.DOAnchorPos(subOverlapPos, mainDuration).SetEase(Ease.OutQuart));
        seq.Join(mainCG.DOFade(1f, mainDuration));
        seq.Join(subCG.DOFade(1f, mainDuration));

        seq.Append(subRect.DOAnchorPos(subBasePos, subDuration).SetEase(Ease.OutBack));

        seq.OnComplete(() => {
            mainCG.blocksRaycasts = true;
            subCG.blocksRaycasts = true;
            onComplete?.Invoke();
        });
    }
    public void HideGroupSlide(
        RectTransform mainRect, 
        RectTransform subRect, 
        Vector2 mainBasePos,
        Vector2 subBasePos,
        Direction direction = Direction.Right, 
        float offsetDistance = 800f, 
        float mainDuration = 0.3f, 
        float subDuration = 0.2f, 
        UnityAction onStart = null, 
        UnityAction onComplete = null)
    {
        if (mainRect == null || subRect == null) return;

        mainRect.DOKill(true);
        subRect.DOKill(true);

        CanvasGroup mainCG = mainRect.GetComponent<CanvasGroup>();
        if (mainCG == null) mainCG = mainRect.gameObject.AddComponent<CanvasGroup>();

        CanvasGroup subCG = subRect.GetComponent<CanvasGroup>();
        if (subCG == null) subCG = subRect.gameObject.AddComponent<CanvasGroup>();

        mainCG.DOKill();
        subCG.DOKill();

        mainCG.blocksRaycasts = false;
        subCG.blocksRaycasts = false;
        
        Vector2 subOverlapPos = subBasePos;
        Vector2 mainEndPos = mainBasePos;

        switch (direction)
        {
            case Direction.Top:
            case Direction.Bottom:
                mainEndPos.y += (direction == Direction.Top) ? offsetDistance : -offsetDistance;
                subOverlapPos.y = mainBasePos.y;
                break;
            case Direction.Left:
            case Direction.Right:
                mainEndPos.x += (direction == Direction.Right) ? offsetDistance : -offsetDistance;
                subOverlapPos.x = mainBasePos.x;
                break;
        }

        Vector2 subEndPos = subOverlapPos;
        
        if (direction == Direction.Top || direction == Direction.Bottom) subEndPos.y = mainEndPos.y;
        else subEndPos.x = mainEndPos.x;

        Sequence seq = DOTween.Sequence();
        seq.OnStart(() => { onStart?.Invoke(); });

        seq.Append(subRect.DOAnchorPos(subOverlapPos, subDuration).SetEase(Ease.InBack));

        seq.Append(mainRect.DOAnchorPos(mainEndPos, mainDuration).SetEase(Ease.InQuart));
        seq.Join(subRect.DOAnchorPos(subEndPos, mainDuration).SetEase(Ease.InQuart));
        seq.Join(mainCG.DOFade(0f, mainDuration));
        seq.Join(subCG.DOFade(0f, mainDuration));

        seq.OnComplete(() => {
            mainRect.anchoredPosition = mainBasePos;
            subRect.anchoredPosition = subBasePos;
            onComplete?.Invoke();
        });
    }
    
    public void ToggleGroupSlide(
        RectTransform mainButton,
        List<RectTransform> subButtons,
        bool show,
        Direction direction = Direction.Left,
        float spacing = 80f,
        float duration = 0.25f)
    {
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < subButtons.Count; i++)
        {
            RectTransform button = subButtons[i];

            Vector2 targetPos;

            if (show)
            {
                targetPos = mainButton.anchoredPosition;

                switch (direction)
                {
                    case Direction.Left:
                        targetPos.x -= spacing * (i + 1);
                        break;

                    case Direction.Right:
                        targetPos.x += spacing * (i + 1);
                        break;

                    case Direction.Top:
                        targetPos.y += spacing * (i + 1);
                        break;

                    case Direction.Bottom:
                        targetPos.y -= spacing * (i + 1);
                        break;
                }
            }
            else
            {
                targetPos = mainButton.anchoredPosition;
            }

            if (show)
                button.anchoredPosition = mainButton.anchoredPosition;

            seq.Join(
                button.DOAnchorPos(targetPos, duration)
                    .SetEase(show ? Ease.OutBack : Ease.InBack)
                    .SetDelay(i * 0.03f)
            );
        }
    }
    
    
    public void ShowPopFromWorld(RectTransform ui, Transform worldOrigin, float duration = 0.4f, UnityAction onComplete = null)
    {
        if (ui == null || worldOrigin == null) return;

        ui.DOKill();
        CanvasGroup cg = ui.GetComponent<CanvasGroup>();
        if (cg == null) cg = ui.gameObject.AddComponent<CanvasGroup>();
        cg.DOKill();

        Canvas canvas = ui.GetComponentInParent<Canvas>();
        RectTransform parentRect = ui.parent.GetComponent<RectTransform>();

        // 1회 원래 자리를 기억 이후엔 항상 캐싱된 값 사용
        if (!popHomePos.TryGetValue(ui, out Vector2 finalPos))
        {
            finalPos = ui.anchoredPosition;
            popHomePos[ui] = finalPos;
        }

        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldOrigin.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, cam, out Vector2 startPos);

        ui.anchoredPosition = startPos;
        ui.localScale = Vector3.zero;
        cg.alpha = 0f;
        cg.blocksRaycasts = false;

        Sequence seq = DOTween.Sequence();
        seq.SetTarget(ui); // ★ 이제 ui.DOKill()이 이 시퀀스를 죽일 수 있음

        seq.Append(ui.DOAnchorPos(finalPos, duration).SetEase(Ease.OutBack));
        seq.Join(ui.DOScale(Vector3.one, duration).SetEase(Ease.OutBack));
        seq.Join(cg.DOFade(1f, duration * 0.5f));

        seq.OnComplete(() => {
            cg.blocksRaycasts = true;
            onComplete?.Invoke();
        });
    }

    /// <summary>
    /// UI 원래 자리에서 3D 오브젝트 위치로 빨려 들어가는 연출
    /// </summary>
    public void HidePopToWorld(RectTransform ui, Transform worldOrigin, float duration = 0.3f, UnityAction onComplete = null)
    {
        if (ui == null || worldOrigin == null) return;

        ui.DOKill();
        CanvasGroup cg = ui.GetComponent<CanvasGroup>();
        if (cg == null) cg = ui.gameObject.AddComponent<CanvasGroup>();
        cg.DOKill();

        Canvas canvas = ui.GetComponentInParent<Canvas>();
        RectTransform parentRect = ui.parent.GetComponent<RectTransform>();

        // 캐싱된 원래 자리로 복원
        Vector2 originalPos = popHomePos.TryGetValue(ui, out var home) ? home : ui.anchoredPosition;

        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldOrigin.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, cam, out Vector2 endPos);

        cg.blocksRaycasts = false;

        Sequence seq = DOTween.Sequence();
        seq.SetTarget(ui); // ★

        seq.Append(ui.DOAnchorPos(endPos, duration).SetEase(Ease.InBack));
        seq.Join(ui.DOScale(Vector3.zero, duration).SetEase(Ease.InBack));
        seq.Join(cg.DOFade(0f, duration));

        seq.OnComplete(() => {
            ui.anchoredPosition = originalPos;
            onComplete?.Invoke();
        });
    }
    private void SetPivot(RectTransform rt, Vector2 newPivot)
    {
        if (rt == null) return;

        Vector2 size = rt.rect.size;
        Vector2 deltaPivot = rt.pivot - newPivot;

        Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);

        rt.pivot = newPivot;
        rt.localPosition -= deltaPosition;
    }
    private Vector2 GetPivotFromDirection(PaperDirection direction)
    {
        switch (direction)
        {
            case PaperDirection.TopLeft: return new Vector2(0f, 1f);
            case PaperDirection.TopRight: return new Vector2(1f, 1f);
            case PaperDirection.BottomLeft: return new Vector2(0f, 0f);
            case PaperDirection.BottomRight: return new Vector2(1f, 0f);
            default: return new Vector2(0.5f, 0.5f); // �⺻�� �߾�
        }
    }
}
