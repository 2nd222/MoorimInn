using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Story/Character")]
public class Character : ScriptableObject
{
    public int characterID;
    public string characterName;
    
    public Sprite characterImage;
    
    public List<FaceSpriteData> faces;
    public List<EyebrowSpriteData> eyebrows;
    public Sprite shadow;
    
    public List<EmotionIcon> emotionIcons;
    
    [Header("비주얼 보정")]
    public CharacterVisualSetting visual;
    
    [Header("이름표")]
    public Sprite namePlateSprite;
}

[Serializable]
public class EmotionIcon
{
    public Emotion emotion;
    public Sprite icon;
    public SFXData sound;
}

[Serializable]
public class CharacterVisualSetting
{
    [Header("캐릭터 기본 크기 보정")]
    public float baseScale = 1f;

    [Header("캐릭터 위치 보정")]
    public Vector2 offset;

    [Header("감정 아이콘 위치")] 
    public Vector2 emotionIconPosition;
    
    [Header("얼굴 음영 색")]
    public Color shadowColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
}

[Serializable]
public class FaceSpriteData
{
    public FaceType type;
    public Sprite sprite;
}

[Serializable]
public class EyebrowSpriteData
{
    public EyebrowType type;
    public Sprite sprite;
}