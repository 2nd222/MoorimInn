using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSlot : MonoBehaviour
{
    public int slotIndex; // 0, 1, 2, 3

    [SerializeField] private RectTransform motionRootX;
    [SerializeField] private RectTransform motionRootY;
    [SerializeField] private RectTransform visualRoot;
    
    [SerializeField] private Image characterImage;
    
    [SerializeField] private Image faceImage;
    [SerializeField] private Image eyebrowImage;
    
    [SerializeField] private Image shadowImage;
    
    [Header("감정 아이콘")]
    [SerializeField] private Image emotionIcon;
    [SerializeField] private Image iconBubble;
    
    private int currentCharacterID = -1;
    public int CurrentCharacterID => currentCharacterID;
    private Character currentCharacter;
    public Character CurrentCharacter => currentCharacter;

    private bool isFlipped; 
    private float currentBaseScale = 1f;
    private float focusScale = 1f;
    private float expressionScale = 1f;
    
    private RectTransform slotRoot;
    public RectTransform SlotRoot
    {
        get
        {
            if (slotRoot == null)
                slotRoot = GetComponent<RectTransform>();

            return slotRoot;
        }
    }
    
    public Vector2 SlotPos => SlotRoot.anchoredPosition;
    private Vector2 defaultEmotionIconPos;
    
    private int motionLock = 0;
    public bool IsPlayingMotion => motionLock > 0;

    private bool isUnknown;
    private bool isFocused;
    
    private FaceType currentFace = FaceType.Neutral;
    private EyebrowType currentEyebrow = EyebrowType.Default;
    private bool currentShadow = false;
    
    private bool HasFaceLayer => currentCharacter != null && currentCharacter.faces != null && currentCharacter.faces.Count > 0;
    private bool HasEyebrowLayer => currentCharacter != null && currentCharacter.eyebrows != null && currentCharacter.eyebrows.Count > 0;
    
    private void Awake()
    {
        slotRoot = GetComponent<RectTransform>();
        
        characterImage.color = Color.white;
        defaultEmotionIconPos = iconBubble.rectTransform.anchoredPosition;
    }
    
    public void SetCharacter(Character character, Sprite bodySprite, bool resetExpression = true)
    {
        HideEmotionIcon();
        
        // 기존 연출 완전히 종료
        ResetMotionRoot();
        visualRoot.DOKill();
        
        StopAllCoroutines();

        currentCharacter = character;
        currentCharacterID = character.characterID;
        characterImage.sprite = bodySprite;
        
        characterImage.color = Color.white;

        faceImage.color = Color.white;
        eyebrowImage.color = Color.white;

        // --------------------------------------------------
        // 표정 초기화 여부
        // --------------------------------------------------
        if (resetExpression)
        {
            currentFace = FaceType.Neutral;
            currentEyebrow = EyebrowType.Default;
            currentShadow = false;
        }
        
        // 그림자 Sprite는 캐릭터 기준으로 다시 설정
        if (character.shadow != null)
        {
            shadowImage.sprite = character.shadow;
        }
        else
        {
            shadowImage.sprite = null;
            currentShadow = false;
        }

        if (character.visual != null)
        {
            iconBubble.rectTransform.anchoredPosition = character.visual.emotionIconPosition;
        }
        else
        {
            iconBubble.rectTransform.anchoredPosition = defaultEmotionIconPos;
        }
        
        RefreshShadow();
        RefreshExpression();
        
        isUnknown = false;
        isFocused = false;
        RefreshScale();
        
        gameObject.SetActive(true);
    }
    
    public void SetHidden(Character character, Sprite bodySprite, bool resetExpression = true)
    {
        currentCharacter = character;
        currentCharacterID = character.characterID;
        characterImage.sprite = bodySprite;
        
        visualRoot.DOKill();
        ResetMotionRoot();
        
        StopAllCoroutines();
        
        if (resetExpression)
        {
            currentFace = FaceType.Neutral;
            currentEyebrow = EyebrowType.Default;
            currentShadow = false;
        }

        if (character.shadow != null)
        {
            shadowImage.sprite = character.shadow;
        }
        else
        {
            shadowImage.sprite = null;
            currentShadow = false;
        }
        
        if (character.visual != null)
        {
            iconBubble.rectTransform.anchoredPosition = character.visual.emotionIconPosition;
        }
        else
        {
            iconBubble.rectTransform.anchoredPosition = defaultEmotionIconPos;
        }
        
        RefreshShadow();
        RefreshExpression();
        RefreshScale();
        gameObject.SetActive(false);
    }

    public void SetFlip(bool flip)
    {
        //Debug.Log($"Flip {currentCharacterID} -> {flip}");
        
        visualRoot.DOKill();
        
        isFlipped = flip;
        RefreshScale();
    }
    
    public void SetBaseScale(float scale)
    {
        currentBaseScale = scale;
        RefreshScale();
    }
    
    private void RefreshScale()
    {
        float scale = currentBaseScale * focusScale * expressionScale;

        float x = isFlipped ? -scale : scale;

        visualRoot.localScale = new Vector3(x, scale, 1f);
    }
    
    private void Clear()
    {
        currentCharacter = null;
        currentCharacterID = -1;
        characterImage.sprite = null;
        
        faceImage.sprite = null;
        eyebrowImage.sprite = null;
        
        faceImage.color = Color.white;
        eyebrowImage.color = Color.white;

        currentShadow = false;
        shadowImage.sprite = null;
        RefreshShadow();
        
        HideEmotionIcon();
    }
    
    public void Appear(Character character, Sprite bodySprite, Vector2 targetPos)
    {
        HideEmotionIcon();
        SetCharacter(character, bodySprite);
        
        motionRootX.DOKill();
        motionRootY.DOKill();
        
        StopAllCoroutines();
        StartCoroutine(AppearRoutine(targetPos));
    }

    public void Disappear(Vector2 startPos)
    {
        if (!gameObject.activeInHierarchy)
            return;
        
        Clear();
        
        motionRootX.DOKill();
        motionRootY.DOKill();
        
        StopAllCoroutines();
        StartCoroutine(DisappearRoutine(startPos));
    }
    
    public void SetFocusScale(float target, float duration = 0.25f)
    {
        visualRoot.DOKill();
        
        DOTween.To(() => focusScale, x => { focusScale = x; RefreshScale(); }, target, duration);
    }
    
    private void ChangeExpression(Sprite faceSprite, Sprite eyebrowSprite, bool useDOScale, float duration = 0.2f)
    {
        bool changeFace = HasFaceLayer && faceSprite != faceImage.sprite;
        bool changeEyebrow = HasEyebrowLayer && eyebrowSprite != eyebrowImage.sprite;

        if (!changeFace && !changeEyebrow)
            return;

        if (changeFace)
            faceImage.sprite = faceSprite;

        if (changeEyebrow)
            eyebrowImage.sprite = eyebrowSprite;

        RefreshExpression();
        RefreshColor();
        
        if (useDOScale)
            PlayExpressionScale();

        if(HasFaceLayer || HasEyebrowLayer)
            Debug.Log($"표정 변경 {faceSprite?.name} 눈썹 변경 {eyebrowSprite?.name}");
    }
    
    private void PlayExpressionScale()
    {
        visualRoot.DOComplete();
        
        expressionScale = 1f;

        DOTween.To(
                () => expressionScale,
                x =>
                {
                    expressionScale = x;
                    RefreshScale();
                },
                1.05f,
                0.25f
            )
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                DOTween.To(
                    () => expressionScale,
                    x =>
                    {
                        expressionScale = x;
                        RefreshScale();
                    },
                    1f,
                    0.25f
                ).SetEase(Ease.InQuad);
            });
    }
    
    public void PlayEmotionIcon(Emotion emotion)
    {
        emotionIcon.DOKill();
        
        EmotionIcon data = currentCharacter.emotionIcons.Find(x => x.emotion == emotion);
        
        iconBubble.gameObject.SetActive(true);
        emotionIcon.sprite = data.icon;
        emotionIcon.gameObject.SetActive(true);

        // 감정 전용 효과음
        if (data.sound != null)
        {
            SoundManager.Instance.PlaySFX(data.sound);
        }
        
        RectTransform rt = iconBubble.rectTransform;

        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = startPos + Vector2.up * 50f;

        Color c1 = emotionIcon.color;
        c1.a = 0;
        emotionIcon.color = c1;
        
        Color c2 = iconBubble.color;
        c2.a = 0;
        emotionIcon.color = c2;
        
        Sequence seq = DOTween.Sequence();
        
        // 등장
        seq.Append(iconBubble.DOFade(1f, 0.1f));
        seq.Join(emotionIcon.DOFade(1f, 0.3f));
        seq.Join(rt.DOAnchorPos(endPos, 0.5f).SetEase(Ease.OutQuad));

        // 유지
        seq.AppendInterval(1f);
        
        // 퇴장
        seq.Append(emotionIcon.DOFade(0f, 0.3f));
        seq.Join(rt.DOAnchorPos(endPos + Vector2.down * 20f, 0.5f));
        seq.Append(iconBubble.DOFade(0f, 0.1f));
        
        seq.OnComplete(() =>
        {
            rt.anchoredPosition = startPos;
            HideEmotionIcon();
        });
    }

    // 흔들기 (캐릭터)
    public IEnumerator Shake(float duration = 0.3f, float power = 0.1f)
    {
        BeginMotion();
        
        Vector2 originalPos = motionRootY.anchoredPosition;
        
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;

            float x = Random.Range(-power, power);
            float y = Random.Range(-power, power);

            motionRootY.anchoredPosition = originalPos + new Vector2(x, y);

            yield return null;
        }

        motionRootY.anchoredPosition = originalPos;
        
        EndMotion();
    }
    
    private void HideEmotionIcon()
    {
        emotionIcon.gameObject.SetActive(false);
        iconBubble.gameObject.SetActive(false);
    }

    public void SetUnknown(bool value)
    {
        isUnknown = value;
        RefreshColor();
    }
    
    public void SetFocused(bool value)
    {
        isFocused = value;
        RefreshColor();
    }

    private void RefreshColor()
    {
        Color shadowBase = GetShadowColor();
        Color target;
        Color targetForShadow;
        
        if (isUnknown)
        {
            target = new Color(0.1f, 0.1f, 0.1f, 1f);
            targetForShadow = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        }
        else if (!isFocused)
        {
            target = new Color(0.7f, 0.7f, 0.7f, 1f);
            targetForShadow = new Color(shadowBase.r * 0.5f, shadowBase.g * 0.5f, shadowBase.b * 0.5f, shadowBase.a);
        }
        else
        {
            target = Color.white;
            targetForShadow = shadowBase;
        }

        characterImage.DOColor(target, 0.2f);
        
        if(faceImage.gameObject.activeSelf)
            faceImage.DOColor(target,0.2f);
        
        if(eyebrowImage.gameObject.activeSelf)
            eyebrowImage.DOColor(target,0.2f);
        
        if (shadowImage.gameObject.activeSelf)
            shadowImage.DOColor(targetForShadow, 0.2f);
    }
    
    IEnumerator AppearRoutine(Vector2 targetPos)
    {
        BeginMotion();
        
        Vector2 startOffset = Vector2.left * 200f;
        motionRootX.anchoredPosition = startOffset;

        float time = 0;

        while (time < 0.3f)
        {
            time += Time.deltaTime;
            float t = time / 0.3f;

            motionRootX.anchoredPosition = Vector2.Lerp(startOffset, Vector2.zero, t);
            yield return null;
        }

        motionRootX.anchoredPosition = Vector2.zero;
        EndMotion();
    }
    
    IEnumerator DisappearRoutine(Vector2 startPos)
    {
        BeginMotion();
        
        motionRootX.anchoredPosition = Vector2.zero;
        Vector2 endOffset = Vector2.right * 200f;

        float time = 0;

        while (time < 0.3f)
        {
            time += Time.deltaTime;
            float t = time / 0.3f;

            motionRootX.anchoredPosition = Vector2.Lerp(startPos, endOffset, t);
            yield return null;
        }
        
        EndMotion();
        gameObject.SetActive(false);
    }
    
    public IEnumerator MoveRoutine(Vector2 dir, float distance, float duration)
    {
        BeginMotion();

        Vector2 target = dir * distance;

        Tween tween = motionRootX
            .DOAnchorPos(target, duration)
            .SetEase(Ease.OutQuad);

        yield return tween.WaitForCompletion();

        slotRoot.anchoredPosition += target; 
        motionRootX.anchoredPosition = Vector2.zero;
        EndMotion();
    }
    
    public IEnumerator MoveToPosition(Vector2 targetPos, float duration)
    {
        gameObject.SetActive(true);
        BeginMotion();
        motionRootX.DOKill();

        Debug.Log($"{slotIndex}번 슬롯 {targetPos}로 이동");
        Tween tween = slotRoot.DOAnchorPos(targetPos, duration).SetEase(Ease.InOutQuad);

        yield return tween.WaitForCompletion();

        EndMotion();
    }
    
    
    
    public IEnumerator ExitRoutine(Vector2 direction, float distance, float duration)
    {
        BeginMotion();
        
        motionRootX.DOKill();
        
        Vector2 targetOffset = direction * distance;

        Tween tween = motionRootX.DOAnchorPos(targetOffset, duration).SetEase(Ease.InOutQuad);
        
        yield return tween.WaitForCompletion();
        
        EndMotion();
        gameObject.SetActive(false);
    }
    
    public IEnumerator EnterRoutine(Vector2 targetPos, Vector2 direction, float distance, float duration, bool isUnknown)
    {
        gameObject.SetActive(true);
        BeginMotion();
        motionRootX.DOKill();
        
        slotRoot.anchoredPosition = targetPos;
        // 시작 위치 (화면 밖)
        Vector2 startOffset = direction * distance;
        motionRootX.anchoredPosition = startOffset;

        SetUnknown(isUnknown);
        
        Sequence seq = DOTween.Sequence();

        seq.Append(motionRootX.DOAnchorPos(Vector2.zero, duration).SetEase(Ease.OutCubic));
        //seq.Join(currentImage.DOFade(1f, duration));

        yield return seq.WaitForCompletion();
        
        motionRootX.anchoredPosition = Vector2.zero;
        EndMotion();
    }
    
    public IEnumerator EnterMoveRoutine(float startOffset, Vector2 targetPos, float duration, bool isUnknown)
    {
        gameObject.SetActive(true);
        BeginMotion();
        slotRoot.DOKill();
        motionRootX.DOKill();
        
        slotRoot.anchoredPosition = targetPos;
        Vector2 startPos = new Vector2(targetPos.x + startOffset, motionRootX.anchoredPosition.y);
        motionRootX.anchoredPosition = startPos;

        Color c = characterImage.color;
        c.a = 0f;
        characterImage.color = c;

        SetUnknown(isUnknown);
        
        Sequence seq = DOTween.Sequence();

        seq.Append(motionRootX.DOAnchorPos(Vector2.zero, duration).SetEase(Ease.OutCubic));
        //seq.Join(currentImage.DOFade(1f, duration));

        yield return seq.WaitForCompletion();
        EndMotion();
    }
    
    public IEnumerator PlayAttack(Vector2 dir, float power = 80f, float duration = 0.15f)
    {
        BeginMotion();
        
        Vector2 forward = dir * power;

        Sequence seq = DOTween.Sequence();
        seq.Append(motionRootX.DOAnchorPos(forward, duration).SetEase(Ease.OutQuad));
        seq.Append(motionRootX.DOAnchorPos(Vector2.zero, duration).SetEase(Ease.InQuad));
        
        yield return seq.WaitForCompletion();
        
        motionRootX.anchoredPosition = Vector2.zero;
        EndMotion();
    }
    
    public IEnumerator PlayHit(Vector2 dir, float power = 30f, float duration = 0.2f)
    {
        BeginMotion();
        
        Debug.Log("Hit");
        Sequence seq = DOTween.Sequence();

        seq.Append(motionRootX.DOAnchorPos(dir * power, duration * 0.3f));
        seq.Append(motionRootX.DOAnchorPos(Vector2.zero, duration * 0.7f));

        // 흔들림
        motionRootX.DOShakeAnchorPos(duration, 10f, 20, 90, false, true);
        
        yield return seq.WaitForCompletion();
        
        motionRootX.anchoredPosition = Vector2.zero;
        EndMotion();
    }
    
    public IEnumerator JumpRoutine(float height, float duration)
    {
        BeginMotion();

        float baseY = motionRootY.anchoredPosition.y;
        
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = time / duration;

            float y = Mathf.Sin(t * Mathf.PI) * height;

            motionRootY.anchoredPosition = new Vector2(0, baseY + y);

            yield return null;
        }

        motionRootY.anchoredPosition = new Vector2(0, baseY);

        EndMotion();
    }
    
    public IEnumerator EmphasisRoutine(float strength = 0.2f)
    {
        BeginMotion();
        Tween tween = visualRoot.DOPunchScale(Vector3.one * strength, 0.3f, 10, 1);
        
        yield return tween.WaitForCompletion();
        RefreshScale();
        EndMotion();
    }
    
    public IEnumerator NervousRoutine()
    {
        BeginMotion();
        
        Vector2 originalPos = motionRootY.anchoredPosition;
        
        Tween tween = motionRootY.DOShakeAnchorPos(0.5f, 5f, 30);
        
        yield return tween.WaitForCompletion();
        
        motionRootY.anchoredPosition = originalPos;
        EndMotion();
    }
    
    public IEnumerator SitRoutine(float distance = 80f, float duration = 0.25f)
    {
        BeginMotion();
        motionRootY.DOKill();

        Tween tween = motionRootY.DOAnchorPosY(-distance, duration).SetEase(Ease.OutQuad);
        yield return tween.WaitForCompletion();

        EndMotion();
    }

    public IEnumerator StandRoutine(float duration = 0.25f)
    {
        BeginMotion();
        motionRootY.DOKill();

        Tween tween = motionRootY.DOAnchorPosY(0f, duration).SetEase(Ease.OutQuad);
        yield return tween.WaitForCompletion();

        EndMotion();
    }
    
    public IEnumerator GreetRoutine(float forward = 70f, float bow = 20f, float duration = 0.45f)
    {
        BeginMotion();

        motionRootX.DOKill();
        motionRootY.DOKill();

        float baseX = motionRootX.anchoredPosition.x;
        float baseY = motionRootY.anchoredPosition.y;
        float dir = isFlipped ? -1f : 1f;

        // 1. 앞으로 이동
        yield return motionRootX
            .DOAnchorPosX(baseX + dir * forward, duration * 0.3f)
            .SetEase(Ease.OutQuad)
            .WaitForCompletion();

        // 2. 숙이기
        yield return motionRootY
            .DOAnchorPosY(baseY - bow, duration * 0.4f)
            .SetEase(Ease.OutQuad)
            .WaitForCompletion();

        // 3. 뒤로 가면서 일어나기
        Sequence seq = DOTween.Sequence();
        seq.Join(
            motionRootX
                .DOAnchorPosX(baseX, duration * 0.3f)
                .SetEase(Ease.InOutQuad));

        seq.Join(
            motionRootY
                .DOAnchorPosY(baseY, duration * 0.3f)
                .SetEase(Ease.OutQuad));

        yield return seq.WaitForCompletion();

        EndMotion();
    }
    
    public void ClearImmediate()
    {
        motionRootX.anchoredPosition = Vector2.zero;
        motionRootY.anchoredPosition = Vector2.zero;

        motionRootX.DOKill();
        motionRootY.DOKill();
        
        StopAllCoroutines();
        gameObject.SetActive(false);
        
        currentCharacter = null;
        currentCharacterID = -1;
        
        characterImage.sprite = null;
        faceImage.sprite = null;
        eyebrowImage.sprite = null;
        
        faceImage.color = Color.white;
        eyebrowImage.color = Color.white;

        characterImage.color = Color.white;
        
        currentShadow = false;
        shadowImage.sprite = null;
        RefreshShadow();
        
        HideEmotionIcon();
        
        isUnknown = false;
        isFocused = false;
        
        focusScale = 1f;
        expressionScale = 1f;
        currentBaseScale = 1f;
        isFlipped = false;

        RefreshScale();
    }
    
    public void SetExpression(ExpressionData expression)
    {
        if (expression == null || currentCharacter == null)
            return;
        
        FaceType nextFace = currentFace;
        EyebrowType nextEyebrow = currentEyebrow;
        bool nextShadow = currentShadow;
        
        if (expression.face != FaceType.None)
            nextFace = expression.face;

        if (expression.eyebrow != EyebrowType.None)
            nextEyebrow = expression.eyebrow;

        switch (expression.shadow)
        {
            case ShadowType.On:
                nextShadow = true;
                break;

            case ShadowType.Off:
                nextShadow = false;
                break;

            // None이면 그대로 유지
        }
        
        if (nextFace == currentFace && nextEyebrow == currentEyebrow && nextShadow == currentShadow)
            return;

        bool shadowChanged = nextShadow != currentShadow;
        
        currentFace = nextFace;
        currentEyebrow = nextEyebrow;
        currentShadow = nextShadow;
        
        if (shadowChanged)
            ChangeShadow(currentShadow);
        
        Sprite faceSprite = null;
        Sprite eyebrowSprite = null;

        if (HasFaceLayer)
            faceSprite = currentCharacter.faces.Find(x => x.type == currentFace)?.sprite;

        if (HasEyebrowLayer)
            eyebrowSprite = currentCharacter.eyebrows.Find(x => x.type == currentEyebrow)?.sprite;

        // 얼굴, 눈썹 레이어가 하나도 없으면 애니메이션 자체를 안 함
        if (faceSprite == null && eyebrowSprite == null)
        {
            RefreshExpression();
            return;
        }

        ChangeExpression(faceSprite, eyebrowSprite, expression.useDOScale);
    }
    
    private void RefreshExpression()
    {
        Sprite faceSprite = null;
        Sprite eyebrowSprite = null;

        if (HasFaceLayer)
            faceSprite = currentCharacter.faces.Find(x => x.type == currentFace)?.sprite;

        if (HasEyebrowLayer)
            eyebrowSprite = currentCharacter.eyebrows.Find(x => x.type == currentEyebrow)?.sprite;

        // 얼굴
        bool showFace = HasFaceLayer && faceSprite != null;

        faceImage.gameObject.SetActive(showFace);

        if (showFace)
            faceImage.sprite = faceSprite;

        // 눈썹
        bool showEyebrow = HasEyebrowLayer && eyebrowSprite != null;

        eyebrowImage.gameObject.SetActive(showEyebrow);

        if (showEyebrow)
            eyebrowImage.sprite = eyebrowSprite;
    }
    
    private void RefreshShadow()
    {
        shadowImage.DOKill();

        if (currentShadow)
        {
            shadowImage.gameObject.SetActive(true);

            RefreshColor(); // 현재 focus 상태 색 적용
        }
        else
        {
            shadowImage.gameObject.SetActive(false);
        }
    }
    
    private void ChangeShadow(bool showShadow, float duration = 0.1f)
    {
        shadowImage.DOKill();

        if (showShadow)
        {
            shadowImage.gameObject.SetActive(true);

            Color c = GetShadowColor();
            c.a = 0.5f;
            shadowImage.color = c;

            RefreshColor();
        }
        else
        {
            shadowImage.gameObject.SetActive(false);

            RefreshColor();
        }
    }
    
    public void ResetExpression()
    {
        currentFace = FaceType.Neutral;
        currentEyebrow = EyebrowType.Default;
        currentShadow = false;

        RefreshExpression();
        RefreshShadow();
    }
    
    public void ResetMotionRoot()
    {
        motionRootX.DOKill();
        motionRootY.DOKill();

        motionRootX.anchoredPosition = Vector2.zero;
        motionRootY.anchoredPosition = Vector2.zero;
    }
    
    private void BeginMotion()
    {
        motionLock++;
    }

    private void EndMotion()
    {
        motionLock = Mathf.Max(0, motionLock - 1);
    }
    
    private Color GetShadowColor()
    {
        if (currentCharacter != null &&
            currentCharacter.visual != null)
        {
            return currentCharacter.visual.shadowColor;
        }

        return new Color(0.5f, 0.5f, 0.5f, 0.5f);
    }
}
