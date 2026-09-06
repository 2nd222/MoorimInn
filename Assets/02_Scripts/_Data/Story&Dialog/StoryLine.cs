using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

[Serializable]
public class StoryLine
{
    public int id;
    
    [Header("대화인지 나레이션인지")]
    public LineType lineType;

    [Header("일러스트 단일 그림 화면인 경우")] 
    public bool useIllustInCharacterScene = false; // 캐릭터 슬롯 씬에서 일러스트 호출할 때
    public Sprite illustrationOverride;
    
    [Header("추가 오버레이 일러스트")]
    public Sprite illustrationOverlay;
    
    [Header("일러스트 리셋 여부")]
    public bool resetIllustration;
    public bool resetOverlay;
    
    [Header("일러스트 다중 그림 시퀀스의 경우")]
    public List<IllustrationFrame> illustrationSequence;
    
    [Header("캐릭터 슬롯의 경우 캐릭터 현재 캐릭터에 대한 정보 및 행동 설정")]
    public int characterID;
    public bool isUnknown;
    public ExpressionData expressionData;
    public MotionData motion;
    public List<MotionCommand> motions;
    public bool playTogether;
    public string changeDisplayName;
    public bool resetDisplayName;
    public List<FlipCommand> flips;
    
    [Tooltip("이 대사 진행 시 해당 캐릭터 슬롯을 가장 앞으로 가져옵니다.")]
    public bool bringCharacterToFront;
    
    [Header("현재 화자 외에 추가 표정 변경")]
    public List<ExpressionChange> extraExpressions;
    
    [Header("스크린 이펙트 관련")]
    public ScreenEffectData screenEffect;
    
    
    [Header("대사 본문")]
    [TextArea(3, 10)]
    public string line;
    
    [Header("사운드")]
    public SFXData playSFX;
    public bool stopBGM;
    public bool stopAllSFX;
    
    [Header("연출")]
    public bool useTypingSound = true;
    
    [Header("생각인지 발화인지")]
    public bool isInnerThought;
    
    [Header("감정 이모지 사용하는지")]
    public bool showEmotionIcon;
    public Emotion emotion;
    
    [Header("강조 대사인지")]
    public bool useAccentFontSize;
    public bool useSmallFontSize;
    
    [Header("대사 창을 내릴 지(대사 없이 그림만 나올 때 사용 가능)")]
    public bool hideDialogueUI;
    
    [Header("대사 자동 진행")]
    public bool autoNext;
    public float autoDelay = 1.5f;
    
    [Header("로그 창에 추가 안하는 경우")]
    public bool hideFromLog;
    
    [Header("장면 씬 전환 속도")]
    public TransitionSpeed transitionSpeed;
}

public enum LineType
{
    Dialogue,   // 일반 대화
    Narration   // 나레이션
}

public enum Emotion
{
    Neutral,
    Happy,
    Anger,
    Sad,
    Embarrassed,
    Surprised,
    Complicated,
    Disappointed,
    Silence,
    Lovely,
    Exclamation
}

public enum FaceType
{
    None,       // 변경 안함
    Neutral,
    Neutral2,
    Neutral3,
    Happy1,
    Happy2,
    Happy3,
    Anger1,
    Anger2,
    Anger3,
    Wink1,
    Wink2,
    Wink3,
    Wink4,
    Surprised1,
    Surprised2,
    Surprised3,
    Embarrassed1,
    Embarrassed2,
    Embarrassed3,
    Sad1,
    Sad2,
    Sad3,
    Disappointment1,
    Disappointment2,
    Disappointment3,
    Conceited1,
    Conceited2,
    Conceited3,
    Serious1,
    Serious2,
    Serious3,
    Anger4
}

public enum EyebrowType
{
    None,       // 변경 안함
    Default,
    Angry,
    Sad,
    Surprised,
    Embarrassed,
    Disappointment,
    HalfDown,
    StrongAnger,
}

public enum ShadowType
{
    None,   // 변경 안 함
    On,
    Off
}

[Serializable]
public class ExpressionData
{
    public FaceType face = FaceType.None;
    public EyebrowType eyebrow = EyebrowType.None;
    public ShadowType shadow = ShadowType.None;
    public bool useDOScale;
}

[Serializable]
public class MotionCommand
{
    public int characterID;
    public int targetID;
    public MotionData motion;
}

[Serializable]
public class MotionData
{
    public MotionType type;
    public float duration;
    public float value; // 이동 거리 or 점프 높이
    public int targetSlotIndex = -1;
    public float stopDistance = 100f;
    public float targetValue;
    
    [Header("Flip")]
    public bool useFlip;
    public bool flip;
}

[Serializable]
public class FlipCommand
{
    public int characterID;
    public bool flip;
}

[Serializable]
public class IllustrationFrame
{
    [Header("Visual")]
    public Sprite illustration;
    public Sprite background;
    
    [Header("Overlay")]
    public Sprite overlayIllustration;
    
    [Header("Timing")]
    public float duration = 1f;
    
    [Header("Transition")]
    public TransitionSpeed transitionSpeed;
    public IllustrationTransitionType transitionType;

    [Header("Option")] 
    public bool keepPreviousBG;
    public bool useFlashTransition = false;
}

[Serializable]
public class ExpressionChange
{
    public int characterID;
    public ExpressionData expressionData;
    public bool isUnknown;
}

[Serializable]
public class ScreenEffectData
{
    public ScreenEffectType effectType = ScreenEffectType.None;

    public float effectDuration = 0.3f;
    public float effectPower = 30f;
}

public enum MotionType
{
    None,
    MoveLeft,
    MoveRight,
    MoveUp,
    MoveDown,
    ExitLeft,
    ExitRight,
    EnterLeft,
    EnterRight,
    Jump,
    Shake,
    Attack,   // 추가
    Hit,      // 추가
    Emphasis,    // 강조
    Nervous,  // 떨림
    MoveToTarget,
    ReturnSlot,
    Sit,
    Stand,
    EnterMove,
    Greet,
}

public enum ScreenEffectType
{
    None,

    FadeBlack,
    FadeFromBlack,

    FlashWhite,
    Shake,
    FlashWithShake,

    ZoomIn,
    ZoomOut,

    VignetteDark,
    VignetteClear,
    
    FlashBlack,
    
    ShakeAndSound,
    
    FadeBlur,
    FadeFromBlur
}

public static class DialogueStyle
{
    public static string Angry(string text)
    {
        return $"<color=#FF5555><b>{text}</b></color>";
    }

    public static string Whisper(string text)
    {
        return $"<alpha=#88><i>{text}</i>";
    }
}

public enum TransitionSpeed
{
    Default,
    Fast,
    Slow,
    Dramatic,
    NoTerm
}

public enum IllustrationTransitionType { Fade, Flash, }