using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

[CreateAssetMenu(fileName = "StoryScenes", menuName = "Story/StoryScenes")]
public class Story:ScriptableObject
{
    public int id;
    public string storyName;
    
    [TextArea(3, 10)]
    public string description;

    public string mainSpeaker;

    public List<int> characterIDs;
    public List<StoryScene> scenes;
    
    public QuestData rewardQuest;
    
    [Header("스토리 보상")]
    public int rewardMoney;
    public int rewardFame;

    public List<ItemReward> rewardItems;
}

[Serializable]
public class StoryScene
{
    public SceneVisualType visualType;
    public List<CharacterInScene> characters;
    public List<StoryLine> lines;
    public Sprite background; 
    public Sprite illustration;

    public bool isRecallScene;
    
    [Header("추가 오버레이 일러스트")]
    public Sprite illustrationOverlay;
    
    [Header("씬 종료 연출")]
    public ScreenEffectData endEffect;
    
    [Header("씬 전환")]
    public SceneTransitionType transitionType = SceneTransitionType.Fade;
    public bool startWithBlack = false;
    
    [Header("사운드")]
    public BGMData sceneBGM;
    public bool changeBGM = false;
}

[Serializable]
public class CharacterInScene
{
    public int characterID;
    public int slotIndex;
    public bool startVisible;
    
    [Header("캐릭터 슬롯 UI에는 등장 안함")]
    public bool useSlotUI = true;
    
    public bool startFlip;
    
    [Header("씬 전환 시 상태")]
    public bool resetExpression = true;
}

[Serializable]
public enum SceneVisualType
{
    CharacterSlots, // 기존 방식
    Illustration    // CG 컷씬 방식
}

public enum SceneTransitionType
{
    None,
    Fade
}

[Serializable]
public class ItemReward
{
    public ItemData item;
    public int count = 1;
    public Constants.InventoryType inventoryType;
}