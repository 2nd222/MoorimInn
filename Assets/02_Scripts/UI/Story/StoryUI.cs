using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum LogType
{
    Dialogue,
    Header
}

[Serializable]
public class DialogueLog
{
    public LogType logType;
    
    public int day;
    public string title;
    
    public string speaker;
    public string content;

    public bool isNarration;
    public bool isInnerThought;
}

public class StoryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private Image namePlateImage;
    [SerializeField] private Sprite unknownCharacterNamePlate;
    [SerializeField] private TextMeshProUGUI contentText;

    [SerializeField] private GameObject root;
    [SerializeField] private GameObject screenRoot;
    [SerializeField] private GameObject canvasBG;
    [SerializeField] private GameObject characterNameRoot;
    [SerializeField] private GameObject darkBackGround;
    [SerializeField] private CanvasGroup darkBackgroundGroup;
    [SerializeField] private GameObject recallSceneDim;
    [SerializeField] private DialogueLogUI dialogueLogUI;
    [SerializeField] private GameObject defaultIllustBG;
    
    [SerializeField] private Image bgImageA;
    [SerializeField] private Image bgImageB;
    [SerializeField] private Image illustrationA;
    [SerializeField] private Image illustrationB;
    [SerializeField] private Image overlayIllustA;
    [SerializeField] private Image overlayIllustB;
    
    [SerializeField] private StorySettingMenu storySettingMenu;
    [SerializeField] private Image nextIndicator;
    
    [SerializeField] private float autoPlayDelay = 1.5f;
    private readonly WaitForSeconds typingWait = new(0.03f);
    
    [SerializeField] private ScreenEffectUI screenEffectUI;
    
    [Header("Dialogue Motion")]
    [SerializeField] private RectTransform dialogueRoot;
    [SerializeField] private CanvasGroup dialogueCanvasGroup;
    
    [Header("Focus Mode")]
    [SerializeField] private float focusFade = 0.85f;
    [SerializeField] private float focusDuration = 0.25f;
    
    private Coroutine typingCoroutine;
    private Coroutine autoCoroutine;
    private Coroutine autoPlayCoroutine;
    
    private Coroutine illustrationCoroutine;
    private Coroutine illustrationSequenceCoroutine;
    private Coroutine illustrationZoomCoroutine;

    private bool isTyping; // 현재 글자 나오고 있는 상황인지
    private bool isLogOpen;
    private bool isTransitioning; // 일러스트 전환 중인지
    private bool isCutSceneAuto; // 컷 씬용 대사 자동 재생 on인지
    private bool isAutoPlayEnabled; // 자동 재생 on off인지
    private bool isPointerBlocking; // 다른 버튼 눌렀을 때 대사 안 넘어가도록 bool 값 설정
    
    private List<StoryLine> history = new();
    private int historyIndex = -1;
    
    private List<DialogueLog> dialogueLogs = new();
    public IReadOnlyList<DialogueLog> DialogueLogs => dialogueLogs;
    
    private StoryLine currentLine;
    private string currentFullText;
    
    private bool isFirstBackground = true;
    private Coroutine bgCoroutine;
    private Coroutine blinkCoroutine;
    
    private float defaultContentFontSize;
    private float accentFontSize;
    private float smallFontSize;
    
    private Image currentBg;
    private Image nextBg;

    private CanvasGroup currentBgGroup;
    private CanvasGroup nextBgGroup;
    
    private Image currentIllust;
    private Image nextIllust;

    private CanvasGroup currentGroup;
    private CanvasGroup nextGroup;

    private Image currentOverlay;
    private Image nextOverlay;

    private CanvasGroup currentOverlayGroup;
    private CanvasGroup nextOverlayGroup;
    
    public void Init()
    {
        defaultContentFontSize = UIManager.Instance.GetTextScale();
        accentFontSize = UIManager.Instance.GetTextScale() * 1.3f;
        smallFontSize = UIManager.Instance.GetTextScale() * 0.7f;
        UIManager.Instance.OnFontSizeChanged += SetFontSize;

        InitBackGround();
        InitIllustration();
    }

    private void OnEnable()
    {
        StoryManager.Instance.SetStoryUI(this);
        storySettingMenu.Init(this);
    }
    
    private void OnDisable()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.OnFontSizeChanged -= SetFontSize;
    }
    
    public void Show(Story story)
    {
        root.SetActive(true);
        canvasBG.SetActive(true);
        screenRoot.SetActive(true);
        defaultIllustBG.SetActive(true);
        currentBg.gameObject.SetActive(true);
        
        string title = $"[{story.mainSpeaker}] {story.storyName}";
        titleText.text = title;
        ResetBackgroundState();
        ResetIllustrationState();
        
        storySettingMenu.RefreshAutoButtonSprite();
        
        darkBackgroundGroup.alpha = 0f;
        darkBackGround.SetActive(false);
    }

    public void Hide()
    {
        StopAllAutoPlay();
        
        titleText.text = "";
        
        root.SetActive(false);
        canvasBG.SetActive(false);
        screenRoot.SetActive(false);
        defaultIllustBG.SetActive(false);
        currentBg.gameObject.SetActive(false);
        SetRecallDim(false);
    }
    
    private void SetDialogueUIVisible(bool visible)
    {
        root.SetActive(visible);
    }
    
    public void ShowCharacterName()
    {
        characterNameRoot.SetActive(true);
    }

    public void HideCharacterName()
    {
        characterNameRoot.SetActive(false);
    }

    private void ShowDialogue()
    {
        dialogueRoot.gameObject.SetActive(true);
    }

    private void HideDialogue()
    {
        dialogueRoot.gameObject.SetActive(false);
    }
    
    private void Update()
    {
        if (!root.activeSelf) return;

        HandleKeyboardInput();
        HandleMouseInput();
    }
    
    #region 대사 관련
    
    public void ShowLine(StoryLine line)
    {
        StopCutSceneAuto();
        
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        currentLine = line;
        
        HideIndicator();
        
        defaultContentFontSize = UIManager.Instance.GetTextScale();
        accentFontSize = UIManager.Instance.GetTextScale() * 1.3f;
        smallFontSize = UIManager.Instance.GetTextScale() * 0.7f;
        if (line.useAccentFontSize)
            SetFontSize(accentFontSize);
        else if (line.useSmallFontSize)
            SetFontSize(smallFontSize);
        else
            SetFontSize(defaultContentFontSize);
        
        ApplyLineUI(line);
        
        if (line.bringCharacterToFront)
        {
            StoryManager.Instance.CharacterUI.BringCharacterToFront(line.characterID);
        }
        
        if (!line.hideFromLog)
        {
            history.Add(line);
            historyIndex = history.Count - 1;

            AddDialogueLog(line);
        }
        
        SetDialogueUIVisible(!line.hideDialogueUI);
        currentFullText = line.line;

        typingCoroutine = StartCoroutine(TypeLine(line.line));
        if (line.autoNext)
        {
            isCutSceneAuto = true;
            
            if (autoCoroutine != null)
                StopCoroutine(autoCoroutine);

            autoCoroutine = StartCoroutine(AutoNextRoutine(line));
        }
    }

    private void ApplyLineUI(StoryLine line)
    {
        ShowDialogue();
        
        Character character = StoryManager.Instance.CharacterUI.GetCharacterData(line.characterID);
        
        if (line.lineType == LineType.Narration)
        {
            speakerText.text = "";
            contentText.alignment = TextAlignmentOptions.Top;
            HideCharacterName();
        }
        else
        {
            speakerText.text = line.isUnknown ? "???" : StoryManager.Instance.GetCharacterName(line.characterID);
            if (line.isUnknown || character is null)
                namePlateImage.sprite = unknownCharacterNamePlate;
            else
                namePlateImage.sprite = character.namePlateSprite;
            contentText.alignment = TextAlignmentOptions.TopLeft;
            ShowCharacterName();
        }

        ApplyInnerThought(line.isInnerThought, StoryManager.Instance.CurrentScene.visualType, line.characterID);
    }
    
    IEnumerator TypeLine(string text)
    {
        isTyping = true;
        contentText.text = "";
        
        HideIndicator();

        int visibleCount = 0;
        int visibleCharCount = 0;

        while (visibleCount < text.Length)
        {
            if (text[visibleCount] == '<')
            {
                int closeIndex = text.IndexOf('>', visibleCount);

                if (closeIndex != -1)
                {
                    visibleCount = closeIndex + 1;
                }
            }
            else
            {
                char c = text[visibleCount];
                
                visibleCount++;
                visibleCharCount++;

                contentText.text = text[..visibleCount]; 
                
                if (currentLine.useTypingSound &&
                    !char.IsPunctuation(c) &&
                    !char.IsWhiteSpace(c) &&
                    visibleCharCount % 2 == 0)
                {
                    SoundManager.Instance.PlayTypeSound();
                }
                
                yield return typingWait;
                continue;
            }
            contentText.text = text[..visibleCount]; // 0 ~ visibleCount-1 까지
        }

        isTyping = false;
        ShowIndicator();
    }
    
    private void CompleteTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        contentText.text = currentFullText; // ⭐ 핵심
        isTyping = false;
        ShowIndicator();
    }

    public void ClearCurrentLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        StopAllAutoPlay();

        contentText.text = "";
        speakerText.text = "";

        HideCharacterName();
        HideDialogue();
        HideIndicator();
    }

    public void ApplyInnerThought(bool isInner, SceneVisualType visualType, int thinkerID)
    {
        darkBackgroundGroup.DOKill();
        
        bool useIllustrationDim = visualType == SceneVisualType.Illustration;

        if (useIllustrationDim)
        {
            if (isInner)
            {
                contentText.color = new Color(0.83f,0.83f,0.83f);
                darkBackGround.SetActive(true);
                darkBackgroundGroup.DOFade(0.96f, 0.5f).SetEase(Ease.OutQuad);
            }
            else
            {
                contentText.color = new Color(1f,1f,0.85f);
                darkBackgroundGroup.DOFade(0f, 0.5f).SetEase(Ease.InQuad)
                    .OnComplete(() => { darkBackGround.SetActive(false); });
            }
            StoryManager.Instance.CharacterUI.ApplyInnerThoughtFocus(-1,false);
        }
        else
        {
            darkBackGround.SetActive(false);
            contentText.color = isInner ? new Color(0.83f,0.83f,0.83f) : new Color(1f,1f,0.85f);
            StoryManager.Instance.CharacterUI.ApplyInnerThoughtFocus(thinkerID, isInner);
        }
    }

    private void SetFontSize(float size)
    {
        float targetSize = size;

        contentText.fontSize = targetSize;
    }
    
    #endregion
    
    #region 뒷 배경 관련

    private void InitBackGround()
    {
        currentBg = bgImageA;
        nextBg = bgImageB;
        currentBgGroup = currentBg.GetComponent<CanvasGroup>();
        nextBgGroup = nextBg.GetComponent<CanvasGroup>();
        currentBgGroup.alpha = 1f;
        nextBgGroup.alpha = 0f;
        
        currentBg.gameObject.SetActive(false);
        nextBg.gameObject.SetActive(false);
    }

    public void SetBackground(Sprite bg)
    {
        currentBg.gameObject.SetActive(true);
            
        currentBg.sprite = bg;
        currentBgGroup.alpha = 1f;

        nextBg.gameObject.SetActive(false);
        nextBgGroup.alpha = 0f;
            
        isFirstBackground = false;
    }
    
    public IEnumerator ChangeBackgroundRoutine(Sprite bg, float duration)
    {
        if (isFirstBackground)
        {
            SetBackground(bg);
            yield break;
        }
        
        yield return CrossFadeBackground(bg, duration);
    }
    
    private IEnumerator CrossFadeBackground(Sprite newBg, float duration)
    {
        nextBg.sprite = newBg;
        nextBg.gameObject.SetActive(true);

        nextBgGroup.alpha = 0f;
        
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            
            float t = Mathf.SmoothStep(0f, 1f, time / duration);
            
            currentBgGroup.alpha = 1f - t;
            nextBgGroup.alpha = t;
            
            yield return null;
        }

        currentBgGroup.alpha = 0f;
        nextBgGroup.alpha = 1f;
        
        currentBg.gameObject.SetActive(false);
        
        // swap
        (currentBg, nextBg) = (nextBg, currentBg);
        (currentBgGroup, nextBgGroup) = (nextBgGroup, currentBgGroup);
    }
    
    private void ResetBackgroundState()
    {
        isFirstBackground = true;

        currentBg.sprite = null;
        nextBg.sprite = null;

        currentBgGroup.alpha = 1f;
        nextBgGroup.alpha = 0f;
        
        currentBg.rectTransform.localScale = Vector3.one;
        nextBg.rectTransform.localScale = Vector3.one;
    }

    public void SetRecallDim(bool isRecallScene)
    {
        if (isRecallScene)
            recallSceneDim.gameObject.SetActive(true);
        else
            recallSceneDim.gameObject.SetActive(false);
    }
    
    #endregion
    
    #region 일러스트 스토리 진행 화면

    private void InitIllustration()
    {
        currentIllust = illustrationA;
        nextIllust = illustrationB;
        currentGroup = currentIllust.GetComponent<CanvasGroup>();
        nextGroup = nextIllust.GetComponent<CanvasGroup>();
        currentGroup.alpha = 1f;
        nextGroup.alpha = 0f;
        currentIllust.gameObject.SetActive(false);
        nextIllust.gameObject.SetActive(false);
        
        currentOverlay = overlayIllustA;
        nextOverlay = overlayIllustB;
        currentOverlayGroup = currentOverlay.GetComponent<CanvasGroup>();
        nextOverlayGroup = nextOverlay.GetComponent<CanvasGroup>();
        currentOverlayGroup.alpha = 1f;
        nextOverlayGroup.alpha = 0f;
        currentOverlay.gameObject.SetActive(false);
        nextOverlay.gameObject.SetActive(false);
    }
    
    public void ShowIllustration(Sprite sprite)
    {
        if (sprite != null)
        {
            currentIllust.gameObject.SetActive(true);
            currentIllust.sprite = sprite;
            currentGroup.alpha = 1f;
        }
        else
        {
            currentIllust.gameObject.SetActive(false);
        }
    }
    
    public void ShowIllustration(Sprite sprite, Sprite overlay)
    {
        if (sprite != null)
        {
            currentIllust.gameObject.SetActive(true);
            currentIllust.sprite = sprite;
            currentGroup.alpha = 1f;
        }
        else
        {
            currentIllust.gameObject.SetActive(false);
        }

        if (overlay != null)
        {
            currentOverlay.gameObject.SetActive(true);
            currentOverlay.sprite = overlay;
            currentOverlayGroup.alpha = 1f;
        }
        else
        {
            currentOverlay.gameObject.SetActive(false);
        }
    }

    public IEnumerator ChangeIllustrationRoutine(Sprite sprite, float duration)
    {
        isTransitioning = true;

        yield return CrossFadeIllustration(sprite, null, false, true , duration);

        isTransitioning = false;
    }
    
    public IEnumerator ChangeIllustrationRoutine(
        Sprite sprite,
        Sprite overlay,
        bool resetBase,
        bool resetOverlay,
        float duration)
    {
        isTransitioning = true;

        yield return CrossFadeIllustration(sprite, overlay, resetBase, resetOverlay, duration);

        isTransitioning = false;
    }
    
    private IEnumerator CrossFadeIllustration(
        Sprite sprite,
        Sprite overlay,
        bool resetBase,
        bool resetOverlay,
        float duration)
    {
        bool changeBase = !resetBase && sprite != null;
        bool changeOverlay = !resetOverlay && overlay != null;
        
        if (resetBase)
        {
            currentIllust.gameObject.SetActive(false);
            currentGroup.alpha = 0f;

            nextIllust.gameObject.SetActive(false);
            nextGroup.alpha = 0f;
        }
        else if (changeBase)
        {
            nextIllust.sprite = sprite;
            nextIllust.gameObject.SetActive(true);
            nextGroup.alpha = 0f;
        }
        
        if (resetOverlay)
        {
            currentOverlay.gameObject.SetActive(false);
            currentOverlayGroup.alpha = 0f;

            nextOverlay.gameObject.SetActive(false);
            nextOverlayGroup.alpha = 0f;
        }
        else if (changeOverlay)
        {
            nextOverlay.sprite = overlay;
            nextOverlay.gameObject.SetActive(true);
            nextOverlayGroup.alpha = 0f;
        }
        
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = time / duration;

            if(changeBase)
            {
                currentGroup.alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 1.1f));
                nextGroup.alpha = Mathf.SmoothStep(0f, 1f, t);
            }
            if (changeOverlay)
            {
                currentOverlayGroup.alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 1.1f));
                nextOverlayGroup.alpha = Mathf.SmoothStep(0f, 1f, t);
            }

            yield return null;
        }
        
        if (changeBase)
        {
            nextGroup.alpha = 1f;

            currentGroup.alpha = 0f;
            currentIllust.gameObject.SetActive(false);

            (currentIllust, nextIllust) = (nextIllust, currentIllust);
            (currentGroup, nextGroup) = (nextGroup, currentGroup);
        }
        
        if (changeOverlay)
        {
            nextOverlayGroup.alpha = 1f;
            currentOverlayGroup.alpha = 0f;
            currentOverlay.gameObject.SetActive(false);

            (currentOverlay, nextOverlay) = (nextOverlay, currentOverlay);
            (currentOverlayGroup, nextOverlayGroup) = (nextOverlayGroup, currentOverlayGroup);
        }
    }
    
    public void PlayIllustrationSequence(List<IllustrationFrame> frames, bool useFlashTransition)
    {
        if (illustrationSequenceCoroutine != null)
            StopCoroutine(illustrationSequenceCoroutine);

        illustrationSequenceCoroutine =
            StartCoroutine(PlayIllustrationSequenceRoutine(frames, useFlashTransition));
    }
    
    private IEnumerator PlayIllustrationSequenceRoutine(List<IllustrationFrame> frames, bool useFlashTransition)
    {
        if (frames == null || frames.Count == 0)
            yield break;

        isTransitioning = true;
        
        currentIllust.gameObject.SetActive(false);
        nextIllust.gameObject.SetActive(false);
        
        bool isFirstFrame = true;
        
        foreach (var frame in frames)
        {
            float duration = StoryManager.Instance.GetTransitionDuration(frame.transitionSpeed);

            if (isFirstFrame && useFlashTransition)
            {
                yield return StartCoroutine(screenEffectUI.FlashWhiteRoutine(2f, 1f, () =>
                {
                    ShowIllustration(frame.illustration, frame.overlayIllustration);
                    SetBackground(frame.background);
                }));

                isFirstFrame = false;
            }
            else
            {
                Coroutine bgRoutine = null;
                Coroutine illustRoutine = null;

                // 두 번째 컷부터 transition
                if (!frame.keepPreviousBG && frame.background != null)
                {
                    bgRoutine = StartCoroutine(ChangeBackgroundRoutine(frame.background, duration));
                }

                illustRoutine =
                    StartCoroutine(PlayIllustrationTransitionRoutine(frame.illustration, frame.overlayIllustration, duration, frame.transitionType));
                
                if (bgRoutine != null)
                    yield return bgRoutine;

                if (illustRoutine != null)
                    yield return illustRoutine;
            }
            
            yield return new WaitForSeconds(frame.duration);
        }
        
        isTransitioning = false;
    }
    
    private IEnumerator PlayIllustrationTransitionRoutine(Sprite sprite, Sprite overlay, float duration, IllustrationTransitionType type)
    {
        switch (type)
        {
            case IllustrationTransitionType.Fade:
                yield return CrossFadeIllustration(
                    sprite,
                    overlay,
                    false,
                    false,
                    duration
                );
                break;

            case IllustrationTransitionType.Flash:
                yield return FlashIllustration(sprite, overlay, duration);
                break;
        }
    }
    
    private IEnumerator FlashIllustration(Sprite sprite, Sprite overlay, float duration)
    {
        yield return StartCoroutine(
            screenEffectUI.FlashWhiteRoutine(duration, duration/2, () =>
            {
                currentIllust.gameObject.SetActive(true);
                currentIllust.sprite = sprite;
                currentGroup.alpha = 1f;
                
                if (overlay != null)
                {
                    currentOverlay.gameObject.SetActive(true);
                    currentOverlay.sprite = overlay;
                    currentOverlayGroup.alpha = 1f;
                }
                else
                {
                    currentOverlay.gameObject.SetActive(false);
                }
            }));
    }
    
    public void ShowCharacterSceneIllustration(StoryLine line)
    {
        if (illustrationCoroutine != null)
        {
            StopCoroutine(illustrationCoroutine);
        }

        float duration = StoryManager.Instance.GetTransitionDuration(line.transitionSpeed);

        illustrationCoroutine = StartCoroutine(
            ChangeIllustrationRoutine(
                line.illustrationOverride,
                line.illustrationOverlay,
                false,
                false,
                duration
            )
        );
    }
    
    public IEnumerator FadeOutIllustration(float duration)
    {
        // 기존 일러스트 관련 코루틴 정지
        if (illustrationCoroutine != null)
        {
            StopCoroutine(illustrationCoroutine);
            illustrationCoroutine = null;
        }

        if (illustrationSequenceCoroutine != null)
        {
            StopCoroutine(illustrationSequenceCoroutine);
            illustrationSequenceCoroutine = null;
        }
        
        // 기존 DOTween 제거
        currentGroup.DOKill();
        nextGroup.DOKill();
        currentOverlayGroup.DOKill();
        nextOverlayGroup.DOKill();

        isTransitioning = true;
        
        currentIllust.gameObject.SetActive(true);
        currentOverlay.gameObject.SetActive(currentOverlay.sprite != null);
        
        Sequence sequence = DOTween.Sequence();

        sequence.Join(currentGroup.DOFade(0f, duration).SetEase(Ease.InQuad));

        if (currentOverlay.gameObject.activeSelf)
        {
            sequence.Join(currentOverlayGroup.DOFade(0f, duration).SetEase(Ease.InQuad));
        }

        yield return sequence.WaitForCompletion();

        currentGroup.alpha = 0f;
        currentOverlayGroup.alpha = 0f;

        currentIllust.gameObject.SetActive(false);
        nextIllust.gameObject.SetActive(false);

        currentOverlay.gameObject.SetActive(false);
        nextOverlay.gameObject.SetActive(false);

        currentIllust.sprite = null;
        nextIllust.sprite = null;

        currentOverlay.sprite = null;
        nextOverlay.sprite = null;

        isTransitioning = false;
    }

    public void HideIllustration()
    {
        currentIllust.gameObject.SetActive(false);
        nextIllust.gameObject.SetActive(false);

        currentOverlay.gameObject.SetActive(false);
        nextOverlay.gameObject.SetActive(false);
    }
    
    public void ResetIllustrationState()
    {
        currentIllust = illustrationA;
        nextIllust = illustrationB;

        currentGroup = currentIllust.GetComponent<CanvasGroup>();
        nextGroup = nextIllust.GetComponent<CanvasGroup>();

        currentOverlay = overlayIllustA;
        nextOverlay = overlayIllustB;

        currentOverlayGroup = currentOverlay.GetComponent<CanvasGroup>();
        nextOverlayGroup = nextOverlay.GetComponent<CanvasGroup>();
        
        // 메인
        currentIllust.sprite = null;
        nextIllust.sprite = null;

        currentGroup.alpha = 1f;
        nextGroup.alpha = 0f;

        currentIllust.gameObject.SetActive(false);
        nextIllust.gameObject.SetActive(false);

        // 오버레이
        currentOverlay.sprite = null;
        nextOverlay.sprite = null;

        currentOverlayGroup.alpha = 0f;
        nextOverlayGroup.alpha = 0f;

        currentOverlay.gameObject.SetActive(false);
        nextOverlay.gameObject.SetActive(false);

        // scale 초기화
        currentIllust.rectTransform.localScale = Vector3.one;
        nextIllust.rectTransform.localScale = Vector3.one;
        
        // 코루틴 초기화
        if (illustrationSequenceCoroutine != null)
            StopCoroutine(illustrationSequenceCoroutine);

        if (illustrationCoroutine != null)
            StopCoroutine(illustrationCoroutine);

        if (illustrationZoomCoroutine != null)
            StopCoroutine(illustrationZoomCoroutine);
    }

    #endregion
    
    #region Indicator 관련
    private void ShowIndicator()
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        nextIndicator.enabled = true;
        
        blinkCoroutine = StartCoroutine(BlinkIndicator());
    }

    private void HideIndicator()
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        nextIndicator.enabled = false;
    }
    
    private IEnumerator BlinkIndicator()
    {
        float time = 0;

        while (true)
        {
            time += Time.deltaTime * 1.5f;
            float alpha = Mathf.PingPong(time, 1f);

            var color = nextIndicator.color;
            color.a = alpha;
            nextIndicator.color = color;

            yield return null;
        }
    }
    #endregion

    #region 로그창 관련 함수
    public void AddLogHeader(int day, string storyTitle)
    {
        dialogueLogs.Add(new DialogueLog
        {
            logType = LogType.Header,
            day = day,
            title = storyTitle,
        });
    }
    
    public void AddDialogueLog(StoryLine line)
    {
        dialogueLogs.Add(new DialogueLog
        {
            logType = LogType.Dialogue,
            speaker = line.lineType == LineType.Narration ? "" : (line.isUnknown ? "???" : StoryManager.Instance.GetCharacterName(line.characterID)),
            content = line.line,
            isNarration = line.lineType == LineType.Narration,
            isInnerThought = line.isInnerThought
        });
    }
    
    public void ClearLogs()
    {
        dialogueLogs.Clear();

        history.Clear();
        historyIndex = -1;
    }
    
    public void ToggleLogUI()
    {
        storySettingMenu.RefreshAutoButtonSprite();
        
        if (isLogOpen)
        {
            CloseLogUI();
        }
        else
        {
            OpenLogUI();
        }
    }
    
    private void OpenLogUI()
    {
        isLogOpen = true;

        dialogueLogUI.Open(this);

        HideIndicator();
    }
    
    public void CloseLogUI()
    {
        //Debug.Log($"닫기 전 isTyping = {isTyping}");
        isLogOpen = false;
        dialogueLogUI.Close();
        //Debug.Log($"닫기 후 isTyping = {isTyping}");

        ShowIndicator();
        
        SetPointerBlock(true);
        StartReleasePointerBlock();
    }
    #endregion

    #region 자동재생 관련 내용
    
    /// <summary>
    /// 컷 씬용 자동 재생 함수
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    private IEnumerator AutoNextRoutine(StoryLine line)
    {
        while (isTyping)
            yield return null;

        float timer = 0f;

        while (timer < line.autoDelay)
        {
            // 로그 열려있으면 시간 멈춤
            if (!dialogueLogUI.gameObject.activeSelf && !isTransitioning)
            {
                timer += Time.deltaTime;
            }

            yield return null;
        }
        
        while (isTransitioning)
            yield return null;
        
        StoryManager.Instance.ShowNextLine();
        isCutSceneAuto = false;
    }
    
    private bool CurrentLineIsAuto()
    {
        return currentLine != null && currentLine.autoNext && currentLine.hideDialogueUI;
    }

    /// <summary>
    /// 수동으로 자동 재생 on할 때 사용하는 함수
    /// </summary>
    private void StartAutoPlay()
    {
        if (autoPlayCoroutine != null)
            StopCoroutine(autoPlayCoroutine);

        autoPlayCoroutine = StartCoroutine(AutoPlayRoutine());
    }

    /// <summary>
    /// 수동으로 자동 재생 off
    /// </summary>
    private void StopAutoPlay()
    {
        if (autoPlayCoroutine != null)
            StopCoroutine(autoPlayCoroutine);

        autoPlayCoroutine = null;
    }
    
    private IEnumerator AutoPlayRoutine()
    {
        while (isAutoPlayEnabled)
        {
            // 현재 타이핑 중이면 대기
            while (isTyping)
                yield return null;

            // 로그창 / 전환 중이면 대기
            while (!CanAdvance())
                yield return null;

            float timer = 0f;

            while (timer < autoPlayDelay)
            {
                // 중간에 막히면 시간 멈춤
                if (CanAdvance())
                {
                    timer += Time.deltaTime;
                }

                yield return null;
            }

            // 다시 한번 체크
            if (!CanAdvance())
                continue;

            // 현재 줄 자체 autoNext면 스킵
            if (CurrentLineIsAuto())
                continue;

            StoryManager.Instance.ShowNextLine();
        }
    }
    
    public bool ToggleAutoPlay()
    {
        isAutoPlayEnabled = !isAutoPlayEnabled;

        if (isAutoPlayEnabled)
        {
            StartAutoPlay();
        }
        else
        {
            StopAutoPlay();
        }
        
        return isAutoPlayEnabled;
    }
    
    public void StopAllAutoPlay()
    {
        if (autoCoroutine != null)
        {
            StopCoroutine(autoCoroutine);
            autoCoroutine = null;
        }

        if (autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }

        storySettingMenu.RefreshAutoButtonSprite();
        isCutSceneAuto = false;
        isAutoPlayEnabled = false;
    }
    
    private void StopCutSceneAuto()
    {
        if (autoCoroutine != null)
        {
            StopCoroutine(autoCoroutine);
            autoCoroutine = null;
        }

        isCutSceneAuto = false;
    }
    #endregion

    #region 대사창에 효과

    public void FocusMode(bool enable)
    {
        dialogueRoot.DOKill();
        dialogueCanvasGroup.DOKill();

        if (enable)
        {
            dialogueCanvasGroup.DOFade(focusFade, focusDuration).SetEase(Ease.OutQuad);
        }
        else
        {
            dialogueCanvasGroup.DOFade(1f, focusDuration).SetEase(Ease.InQuad);
        }
    }

    #endregion
    
    #region 행동 가능 여부 확인 함수
    private bool CanAdvance()
    {
        if (UIManager.Instance != null && UIManager.Instance.IsSettingActive)
            return false;
        
        return !IsBlocked();
    }

    private bool IsBlocked()
    {
        return
            isLogOpen ||
            isTransitioning ||
            isCutSceneAuto ||
            isPointerBlocking;
    }
    
    private bool CanScrollHistory()
    {
        return
            !isTyping &&
            !isTransitioning &&
            !isCutSceneAuto;
    }
    
    public bool CanSkip()
    {
        Debug.Log(StoryManager.Instance.CharacterUI.DuringMotion);
        
        if (StoryManager.Instance.CharacterUI == null)
            return false;

        return
            !isTransitioning &&
            !StoryManager.Instance.CharacterUI.DuringMotion;
    }
    #endregion

    #region 입력 관련 함수
    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isLogOpen)
            {
                CloseLogUI();
                return;
            }
        }
        
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.LeftControl))
        {
            Debug.Log("스토리 스킵");
            if (CanSkip())
            {
                StoryManager.Instance.SkipCurrentStory();
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Debug.Log("씬 스킵");
            if (CanSkip())
            {
                StoryManager.Instance.SkipCurrentScene();
                return;
            }
        }
    }
    
    private void HandleMouseInput()
    {
        if (!Input.GetMouseButtonDown(0))
            return;
        
        if (StoryManager.Instance.IsInputLocked)
            return;
        
        if (isPointerBlocking)
            return;
        
        if (isAutoPlayEnabled)
        {
            isAutoPlayEnabled = false;
            storySettingMenu.RefreshAutoButtonSprite();
            StopAutoPlay();
        }

        if (!CanAdvance())
            return;

        if (isTyping)
        {
            CompleteTyping();
            return;
        }

        StoryManager.Instance.ShowNextLine();
    }
    
    public void SetPointerBlock(bool value)
    {
        isPointerBlocking = value;
    }
    
    public void StartReleasePointerBlock()
    {
        StartCoroutine(ReleasePointerBlockRoutine());
    }

    private IEnumerator ReleasePointerBlockRoutine()
    {
        yield return null;

        isPointerBlocking = false;
    }
    #endregion
}
