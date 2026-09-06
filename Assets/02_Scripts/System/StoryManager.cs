using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using static Constants;

[Serializable]
public class CharacterName
{
    public int id;
    public string name;
}

[Serializable]
public class TriggerGroup
{
    // public int storytriggerID; // 만약 한 스토리를 접근하는 방향이 여러 방향(트리거)에서 접근 가능한 구조이면 id 필요
    public StoryTriggerSO so;
    public bool isActive;
}

[Serializable]
public class StorySequence
{
    public string sequenceName;

    [Tooltip("위에서부터 순서대로 재생됩니다.")]
    public List<int> storyIds = new();
}

public enum StoryPlayMode
{
    Normal,
    Recall
}

public class StoryManager : Singleton<StoryManager>, IInitializable
{
    private DayManager dayManager;
    [SerializeField] private Receipt receipt;
    
    private RecipeUnlockManager recipeUnlockManager;
    private IngredientUnlockManager ingredientUnlockManager;
    
    [SerializeField] private StorySO storySO;

    [SerializeField] private List<TriggerGroup> triggerGroups;
    
    [Header("스토리 재생 순서")]
    [SerializeField] private StorySequence storySequence;
    
    [SerializeField] private RestaurantEconomy economy;
    
    [Header("캐릭터 데이터")]
    [SerializeField] private List<Character> characterDatas;
    
    [Header("TooltipUI")]
    [SerializeField] private ToolTipUI tooltipUI;
    
    private StoryUI storyUI;
    public StoryUI StoryUI => storyUI;
    private CharacterUI characterUI;
    public CharacterUI CharacterUI => characterUI;
    private StoryFadePanel fadePanel;
    private ScreenEffectUI screenEffectUI;
    public ScreenEffectUI ScreenEffectUI => screenEffectUI;

    [SerializeField] private List<CharacterName> characterNames;
    private Dictionary<int, string> nameDict;
    private Dictionary<int, string> runtimeNameOverrides = new();

    [SerializeField] private float fadeDuration;
    
    private Tween transitionTween;
    private bool isSkipLocked;
    
    /// <summary>
    /// flag 내용들을 저장데이터로 넣어놓으면 스토리 이어서
    /// </summary>
    private Story currentStory; // 스토리 뭉치 어느건지
    private int currentIndex; // 한 스토리 뭉치 내에서 대사 인덱스
    private int currentSceneIndex;
    private StoryScene currentScene;
    public StoryScene CurrentScene => currentScene;
    private StoryTrigger currentTrigger; // 현재 트리거
    
    private StoryPlayMode playMode = StoryPlayMode.Normal;

    private bool isPlaying;
    public bool IsPlaying => isPlaying && playMode == StoryPlayMode.Normal;
    private bool isTransitioning;
    private bool isInputLocked;
    public bool IsInputLocked => isInputLocked;
    private bool canAdvance = true;

    private Queue<StoryTrigger> triggerQueue = new();
    
    private HashSet<int> playedStoryIds = new(); // 순서가 없는 집합이라 검색에 빠름
    private List<int> playedStoryHistory = new(); // 순서가 있어서 다시보기 용으로 사용
    public List<int> PlayedStoryHistory => playedStoryHistory;
    private HashSet<string> flags = new();
    
    [SerializeField] StoryReplayManager replayManager;
    public StoryReplayManager StoryReplayManager => replayManager;
    public bool IsRecallPlaying => isPlaying && playMode == StoryPlayMode.Recall;
    
    private StoryRewardPopupData rewardPopupData;
    
    public StorySequence StorySequence => storySequence;
    
    public void Init()
    {
        dayManager = DayManager.Instance;
        recipeUnlockManager = RecipeUnlockManager.Instance;
        ingredientUnlockManager = IngredientUnlockManager.Instance;
    }
    
    private void Start()
    {
        nameDict = new Dictionary<int, string>();
        foreach (var c in characterNames)
        {
            nameDict[c.id] = c.name;
        }
    }
    
    public void HandleStoryStart()
    {
        GameManager.Instance.SetState(GameFlowState.Story);
        
        CheckTriggers();
    }

    private void CheckTriggers()
    {
        var allTriggers = new List<StoryTrigger>();

        foreach (var group in triggerGroups)
        {
            if (!group.isActive) continue;

            allTriggers.AddRange(group.so.triggers);
        }

        // 1. 현재 조건에서 재생 가능한 Trigger만 찾는다.
        var availableTriggers = allTriggers.Where(trigger =>
            {
                if (trigger.playOnce && playedStoryIds.Contains(trigger.storyId))
                    return false;

                return CheckCondition(trigger);
            }).ToList();
        
        // 2. StorySequence에 설정된 순서대로 정렬한다.
        var sortedTriggers = availableTriggers
            .OrderBy(trigger => GetStorySequenceOrder(trigger.storyId))
            .ThenByDescending(trigger => trigger.priority);

        // 3. Queue에 넣는다.
        foreach (var trigger in sortedTriggers)
        {
            triggerQueue.Enqueue(trigger);

            if (trigger.playOnce)
            {
                playedStoryIds.Add(trigger.storyId);

                if (!playedStoryHistory.Contains(trigger.storyId))
                    playedStoryHistory.Add(trigger.storyId);
            }
        }

        Debug.Log($"{triggerQueue.Count} stories played");
        
        StartCoroutine(PlaySceneTransition(PlayQueuedStory));
    }
    
    private int GetStorySequenceOrder(int storyId)
    {
        if (storySequence == null || storySequence.storyIds == null)
            return int.MaxValue;

        int index = storySequence.storyIds.IndexOf(storyId);

        if (index < 0)
            return int.MaxValue;

        return index;
    }
    
    private IEnumerator PlayQueuedStory()
    {
        if (triggerQueue.Count == 0)
        {
            Debug.Log("스토리 없음 → NPC로 넘김");
            SoundManager.Instance.PlayRandomBGM();
            GameManager.Instance.CompleteState(GameFlowState.Story);
            yield break;
        }

        SoundManager.Instance.StopBGM();

        currentTrigger = triggerQueue.Dequeue();

        currentStory = storySO.stories.Find(s => s.id == currentTrigger.storyId);

        yield return StartCoroutine(BeginStory(currentStory, StoryPlayMode.Normal));
    }
    
    private IEnumerator BeginStory(Story story, StoryPlayMode mode)
    {
        playMode = mode;

        currentStory = story;

        runtimeNameOverrides.Clear();
        storyUI.ClearLogs();

        storyUI.AddLogHeader(
            mode == StoryPlayMode.Normal
                ? DayManager.Instance.DayData.day
                : 0,
            currentStory.storyName
        );

        currentSceneIndex = 0;
        currentIndex = 0;
        isPlaying = true;

        storyUI.Show(story);

        if (HasAnyCharacter(currentStory))
            characterUI.Show();
        else
            characterUI.Hide();

        yield return StartCoroutine(SetupSceneInternal(false));
    }
    
    private IEnumerator SetupSceneInternal(bool useCrossFadeTransition) // 암전하는 fade를 사용 안하면 true
    {
        currentScene = currentStory.scenes[currentSceneIndex];
        StoryScene beforeScene = null;
        if(currentIndex != 0 ) 
            beforeScene = currentStory.scenes[currentSceneIndex-1];
        currentIndex = 0;
            
        storyUI.SetRecallDim(currentScene.isRecallScene);
        
        if (currentScene.sceneBGM != null)
        {
            SoundManager.Instance.PlayBGM(currentScene.sceneBGM);
        }
        else
        {
            SoundManager.Instance.StopBGM();
        }
        
        if (currentScene.startWithBlack)
        {
            screenEffectUI.SetBlackImmediate(true);
        }
        else
        {
            screenEffectUI.SetBlackImmediate(false);
        }
        
        Coroutine illustRoutine = null;
        Coroutine bgRoutine = null;
        Coroutine characterFade = null;
        
        float duration = GetTransitionDuration(TransitionSpeed.Dramatic);
        
        if (currentScene.visualType == SceneVisualType.Illustration) // 세팅하려는 씬이 일러스트 타입 일 때
        {
            if (useCrossFadeTransition)
            {
                if(currentScene.illustrationOverlay != null)
                {
                    illustRoutine = StartCoroutine(
                    storyUI.ChangeIllustrationRoutine(currentScene.illustration, currentScene.illustrationOverlay, false, false, duration)
                    );
                }
                else
                    illustRoutine = StartCoroutine(
                        storyUI.ChangeIllustrationRoutine(currentScene.illustration, duration)
                    );
            }
            else
            {
                characterUI.Hide();
                
                if(currentScene.illustrationOverlay != null)
                    storyUI.ShowIllustration(currentScene.illustration, currentScene.illustrationOverlay);
                else
                    storyUI.ShowIllustration(currentScene.illustration);
            }
        }
        else
        {
            storyUI.HideIllustration();
            
            if (currentScene.characters == null || currentScene.characters.Count == 0)
                characterUI.Hide();
            else
                characterUI.Show();

            characterUI.ResetFade();
            characterUI.SetupScene(currentScene.characters);

            yield return null;
        }
            
        if (useCrossFadeTransition)
        {
            if(beforeScene != null && beforeScene.visualType == SceneVisualType.CharacterSlots && currentScene.visualType == SceneVisualType.Illustration)
                characterFade = StartCoroutine(CharacterUI.FadeOut(duration));
                
            bgRoutine = StartCoroutine(storyUI.ChangeBackgroundRoutine(currentScene.background, duration));
            yield return new WaitForSeconds(duration);
        }
        else
            storyUI.SetBackground(currentScene.background);
        
        // Debug.Log($"{currentStory.id}");
        // foreach(var currentCharacter in currentScene.characters)
        //     Debug.Log(currentCharacter.characterID);
        
        if (illustRoutine != null)
            yield return illustRoutine;

        if (bgRoutine != null)
            yield return bgRoutine;
        
        if (characterFade != null)
            yield return characterFade;
    }

    public void ShowNextLine()
    {
        if (!canAdvance)
            return;
        
        canAdvance = false;

        StartCoroutine(ShowNextLineRoutine());
    }
    
    private IEnumerator ShowNextLineRoutine()
    {
        if (!isPlaying || currentScene == null)
        {
            canAdvance = true;
            yield break;
        }

        if (isTransitioning)
        {
            canAdvance = true;
            yield break;
        }

        if (currentIndex >= currentScene.lines.Count)
        {
            yield return StartCoroutine(HandleSceneEndRoutine());
            canAdvance = true;
            yield break;
        }

        var line = currentScene.lines[currentIndex++];

        yield return StartCoroutine(ShowLineRoutine(line));

        yield return null; // ← 중요

        canAdvance = true;
    }
    
    private IEnumerator ShowLineRoutine(StoryLine line)
    {
        float transitionDuration = GetTransitionDuration(line.transitionSpeed);
     
        if (!string.IsNullOrEmpty(line.changeDisplayName))
        {
            OverrideCharacterName(line.characterID, line.changeDisplayName);
        }

        if (line.resetDisplayName)
        {
            ResetCharacterName(line.characterID);
        }
        
        if(line.stopBGM)
            SoundManager.Instance.StopBGM();

        if(line.stopAllSFX)
            SoundManager.Instance.StopSFX();
        
        if (line.playSFX != null)
        {
            SoundManager.Instance.PlaySFX(line.playSFX);
        }
        
        // 캐릭터 씬에서 일러스트 사용
        if (line.useIllustInCharacterScene)
        {
            if (line.resetIllustration && line.resetOverlay)
            {
                yield return StartCoroutine(StoryUI.FadeOutIllustration(GetTransitionDuration(line.transitionSpeed)));
            }
            else
            {
                StoryUI.ShowCharacterSceneIllustration(line);
            }
        }
        else if (line.illustrationSequence != null && line.illustrationSequence.Count > 0)
            storyUI.PlayIllustrationSequence(line.illustrationSequence, line.illustrationSequence[0].useFlashTransition);
        else if (line.illustrationOverride != null || line.illustrationOverlay != null || line.resetIllustration || line.resetOverlay)
        {
            yield return StartCoroutine(
                storyUI.ChangeIllustrationRoutine(
                    line.illustrationOverride,
                    line.illustrationOverlay,
                    line.resetIllustration,
                    line.resetOverlay,
                    transitionDuration));
        }
        
        storyUI.ShowLine(line);
        ApplyScreenEffect(line.screenEffect);
        ApplyCharacterFocusEffect(line);
        ApplyFlip(line);
        
        
        if (line.lineType == LineType.Dialogue)
        {
            storyUI.ShowCharacterName();
            
            if (line.isUnknown)
            {
                characterUI.Focus(-1); // 강조 없음
            }
            else
            {
                characterUI.Focus(line.characterID);
            }
        }
        else // ⭐ 나레이션
        {
            storyUI.HideCharacterName();
            characterUI.Focus(-1); // 아무도 강조 안함
        }
        
        characterUI.SetUnknown(line.characterID, line.isUnknown);
        characterUI.SetExpression(line.characterID, line.expressionData);
        
        foreach(var exp in line.extraExpressions)
        {
            characterUI.SetExpression(exp.characterID, exp.expressionData);
        }
        
        if (line.motions != null && line.motions.Count > 0)
        {
            if (line.playTogether)
            {
                yield return StartCoroutine(characterUI.PlayMotionsParallel(line.motions, line.isUnknown));
            }
            else
            {
                foreach (var cmd in line.motions)
                {
                    yield return StartCoroutine(characterUI.PlayMotionRoutine(cmd, line.isUnknown));
                }
            }
        }
        else if (line.motion != null && line.motion.type != MotionType.None)
        {
            yield return StartCoroutine(characterUI.PlayMotionRoutine(line.characterID, line.motion, line.isUnknown));
        }
            
        if(line.showEmotionIcon)
            characterUI.ShowEmotionIcon(line.characterID, line.emotion);
    }

    private IEnumerator HandleSceneEndRoutine()
    {
        yield return ApplyScreenEffectRoutine(currentScene.endEffect);
            
        SceneTransitionType transitionType = currentScene.transitionType;
            
        currentSceneIndex++;

        if (currentSceneIndex >= currentStory.scenes.Count)
        {
            EndStory();
            yield break;
        }

        switch (transitionType)
        {
            case SceneTransitionType.Fade:
                yield return StartCoroutine(PlaySceneTransition(()=>SetupSceneInternal(false)));
                canAdvance = true;
                ShowNextLine();
                break;

            case SceneTransitionType.None:
                storyUI.ClearCurrentLine();
                yield return StartCoroutine(SetupSceneInternal(true));
                canAdvance = true;
                ShowNextLine();
                break;
        }
    }
    
    private void EndStory(int storyId = -1)
    {
        StartCoroutine(PlaySceneTransition(EndStoryRoutine));
    }

    private IEnumerator EndStoryRoutine()
    {
        if (playMode == StoryPlayMode.Recall)
        {
            EndRecallStory();
            yield break;
        }
        
        ApplyStoryRewardsAndFlags();
        yield return StartCoroutine(ShowStoryRewardPopupRoutine());
        yield return StartCoroutine(PlayQueuedStory());
    }

    private void ApplyStoryRewardsAndFlags()
    {
        runtimeNameOverrides.Clear();

        rewardPopupData = new StoryRewardPopupData();
        
        if (currentTrigger != null && !string.IsNullOrEmpty(currentTrigger.addFlagOnComplete))
        {
            flags.Add(currentTrigger.addFlagOnComplete);
        }

        isPlaying = false;
        storyUI.Hide();
        characterUI.Hide();
            
        currentScene = null;

        if (currentStory.rewardQuest != null)
        {
            var quest = new Quest(currentStory.rewardQuest, InventoryManager.Instance.GetInventory(InventoryType.Fridge));
            QuestManager.Instance.AcceptQuest(quest);
        }
            
        // 돈 보상
        if (currentStory.rewardMoney > 0)
        {
            rewardPopupData.money = currentStory.rewardMoney;
            EconomyManager.Instance.AddMoney(currentStory.rewardMoney);
        }

        // 명성 보상
        if (currentStory.rewardFame > 0)
        {
            rewardPopupData.fame = currentStory.rewardFame;
            EconomyManager.Instance.AddFame(currentStory.rewardFame);
        }

        // 아이템 보상
        if (currentStory.rewardItems != null)
        {
            foreach (var reward in currentStory.rewardItems)
            {
                if (reward.item == null)
                    continue;

                if(reward.item is BondItemData bondItemData)
                    BondManager.Instance.AcquireBondItem(bondItemData);
                else
                {
                    Inventory inventory = InventoryManager.Instance.GetInventory(reward.inventoryType);
                    IItem item = ItemFactory.Instance.CreateItem(reward.item.id);
                    inventory.GetItem(item, reward.count);
                }
                
                rewardPopupData.itemRewardNames.Add(reward.item.itemName);
            }
        }
        
        if (currentTrigger != null && currentTrigger.unlockRecipeIds != null)
        {
            foreach (var recipeId in currentTrigger.unlockRecipeIds)
            {
                // 이미 해금되지 않은 경우만 팝업에 추가
                if (!recipeUnlockManager.IsUnlocked(recipeId))
                {
                    RecipeMastery mastery = DataManager.Instance.GetRecipeMasteryData(recipeId);
                    if (mastery != null)
                    {
                        rewardPopupData.unlockedRecipes.Add(mastery.data);
                    }
                }
                recipeUnlockManager.Unlock(recipeId);
            }
        }
        if (currentTrigger != null && currentTrigger.unlockIngredientIds != null)
        {
            foreach (var ingredientId in currentTrigger.unlockIngredientIds)
            {
                // 이미 해금되지 않은 경우만 팝업에 추가
                if (!ingredientUnlockManager.IsUnlocked(ingredientId))
                {
                    IngredientData ingredient = DataManager.Instance.GetIngredientData(ingredientId);
                    if (ingredient != null)
                    {
                        rewardPopupData.unlockedIngredients.Add(ingredient);
                    }
                }
                ingredientUnlockManager.Unlock(ingredientId);
            }
        }
        if (currentTrigger != null && currentTrigger.upgradeTypes != null)
        {
            foreach (var upgradeType in currentTrigger.upgradeTypes)
            {
                if (UpgradeManager.Instance.Upgrade(upgradeType))
                {
                    UpgradeData upgradeData = UpgradeManager.Instance.GetData(upgradeType);

                    if (upgradeData != null)
                    {
                        rewardPopupData.unlockedUpgrades.Add(upgradeData);
                    }
                }
            }
        }
    }
    
    private IEnumerator ShowStoryRewardPopupRoutine()
    {
        if (rewardPopupData == null || !rewardPopupData.HasReward)
        {
            yield break;
        }

        bool confirmed = false;

        string message = "";

        if (rewardPopupData.money > 0)
        {
            message += $"돈 +{rewardPopupData.money}\n";
        }
        if (rewardPopupData.fame > 0)
        {
            message += $"명성 +{rewardPopupData.fame}\n";
        }
        if (rewardPopupData.itemRewardNames.Count > 0)
        {
            message += "\n[획득한 물건]\n";

            foreach (var itemName in rewardPopupData.itemRewardNames)
            {
                message += $"{itemName}\n";
            }
        }
        if (rewardPopupData.unlockedRecipes.Count > 0)
        {
            message += "\n[해금된 음식]\n";

            foreach (var recipe in rewardPopupData.unlockedRecipes)
            {
                message += $"{recipe.recipeName}\n";
            }
        }
        if (rewardPopupData.unlockedIngredients.Count > 0)
        {
            message += "\n[해금된 재료]\n";

            foreach (var ingredient in rewardPopupData.unlockedIngredients)
            {
                message += $"{ingredient.itemName}\n";
            }
        }
        if (rewardPopupData.unlockedUpgrades.Count > 0)
        {
            message += "\n[해금된 업그레이드]\n";

            foreach (var upgrade in rewardPopupData.unlockedUpgrades)
            {
                int level = UpgradeManager.Instance.GetLevel(upgrade.type);

                message += $"{upgrade.upgradeName}\n";
                message += $"{level} 단계\n";
                message += $"{upgrade.levels[level - 1].effectDescription}\n";
            }
        }

        UIManager.Instance.CreateOkPopup(
            message,
            () => confirmed = true,
            null,
            false
        );

        yield return new WaitUntil(() => confirmed);

        rewardPopupData = null;
    }
    
    public void SkipCurrentScene()
    {
        if (!isPlaying || currentScene == null)
            return;
        
        if (isTransitioning || isSkipLocked)
            return;
        
        isSkipLocked = true;
        
        AddSkippedLinesToLog();

        // 현재 씬 끝으로 이동
        currentIndex = currentScene.lines.Count;

        // 다음 ShowNextLine 호출 시 씬 종료 처리됨
        ShowNextLine();

        if(tooltipUI != null)
            tooltipUI.Hide();
        
        DOVirtual.DelayedCall(0.3f, () =>
        {
            isSkipLocked = false;
        });
    }
    
    public void SkipCurrentStory()
    {
        if (!isPlaying || currentStory == null)
            return;

        if (isTransitioning || isSkipLocked)
            return;

        if(tooltipUI != null)
            tooltipUI.Hide();
        
        isSkipLocked = true;
        StartCoroutine(SkipCurrentStoryRoutine());
    }
    
    private IEnumerator SkipCurrentStoryRoutine()
    {
        storyUI.ClearCurrentLine();

        yield return StartCoroutine(
            PlaySceneTransition(EndStoryRoutine)
        );
        
        isSkipLocked = false;
    }
    
    private void AddSkippedLinesToLog()
    {
        for (int i = currentIndex; i < currentScene.lines.Count; i++)
        {
            var line = currentScene.lines[i];

            if (line.hideFromLog)
                continue;

            storyUI.AddDialogueLog(line);
        }
    }
    
    public string GetCharacterName(int id)
    {
        if (runtimeNameOverrides.TryGetValue(id, out var overrideName))
            return overrideName;

        if (nameDict.TryGetValue(id, out var name))
            return name;

        return "???";
    }
    
    public void OverrideCharacterName(int id, string newName)
    {
        runtimeNameOverrides[id] = newName;
    }
    
    public void ResetCharacterName(int id)
    {
        runtimeNameOverrides.Remove(id);
    }
    
    private bool HasAnyCharacter(Story story)
    {
        foreach (var scene in story.scenes)
        {
            if (scene.characters != null && scene.characters.Count > 0)
                return true;
        }

        return false;
    }
    
    private bool CheckCondition(StoryTrigger t)
    {
        var dayData = dayManager.DayData;

        if (dayData.day < t.requiredDay)
            return false;

        if (t.requiredDayOfWeek is { Count: > 0 } && !t.requiredDayOfWeek.Contains(dayData.dayOfWeek))
            return false;

        if (economy.Money < t.requiredMoney)
            return false;

        if (economy.Fame < t.requiredFame)
            return false;

        if (economy.Debt < t.requiredDebt)
            return false;
        
        if (t.recipeCondition != null && !CheckRecipeCondition(t.recipeCondition))
            return false;

        if (!CheckFlagCondition(t))
            return false;
        
        return true;
    }
    
    private bool CheckRecipeCondition(RecipeCondition cond)
    {
        if (!cond.useCondition)
            return true;

        // 아무 레시피
        if (cond.anyRecipe)
        {
            return DataManager.Instance.HasAnyRecipeProficiency(cond.requiredProficiency);
        }

        // 특정 레시피
        var mastery = DataManager.Instance.GetRecipeMasteryData(cond.recipeId);

        if (mastery == null)
            return false;

        return mastery.proficiency >= cond.requiredProficiency;
    }
    
    private bool CheckFlagCondition(StoryTrigger t)
    {
        // 필요 플래그
        if (!string.IsNullOrEmpty(t.requiredFlag))
        {
            if (!flags.Contains(t.requiredFlag))
                return false;
        }

        // 막는 플래그
        if (!string.IsNullOrEmpty(t.blockedFlag))
        {
            if (flags.Contains(t.blockedFlag))
                return false;
        }

        return true;
    }
    
    public float GetTransitionDuration(TransitionSpeed speed)
    {
        return speed switch
        {
            TransitionSpeed.Fast => 0.5f,
            TransitionSpeed.Slow => 1.5f,
            TransitionSpeed.Dramatic => 2f,
            TransitionSpeed.Default => fadeDuration,
            TransitionSpeed.NoTerm => 0.05f,
            _ => fadeDuration
        };
    }
    
    private IEnumerator PlaySceneTransition(Func<IEnumerator> onMid)
    {
        isTransitioning = true; // ⭐ 잠금
        isInputLocked = true;
        
        fadePanel.canvasGroup.gameObject.SetActive(true);
        fadePanel.canvasGroup.blocksRaycasts = true;
        
        storyUI.ClearCurrentLine();
        yield return fadePanel.canvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.InOutBack).WaitForCompletion();
        
        SceneVisualType visualType = currentScene?.visualType ?? SceneVisualType.CharacterSlots;
        storyUI.ApplyInnerThought(false, visualType, -1);
        
        storyUI.ResetIllustrationState();
        screenEffectUI.ResetZoom(0.5f);
        
        if (onMid != null)
            yield return StartCoroutine(onMid());
        
        Canvas.ForceUpdateCanvases();

        yield return null;
        

        yield return fadePanel.canvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InQuad).WaitForCompletion();

        fadePanel.canvasGroup.blocksRaycasts = false;
        fadePanel.canvasGroup.gameObject.SetActive(false);
        
        isTransitioning = false;

        yield return null;
        yield return null;
        
        ShowNextLine();

        yield return null;
        yield return null;
        
        isInputLocked = false;
    }
    
    private void ApplyScreenEffect(ScreenEffectData effect)
    {
        if (effect == null)
            return;

        var effectUI = screenEffectUI;

        switch (effect.effectType)
        {
            case ScreenEffectType.None:
                break;

            case ScreenEffectType.FadeBlack:
                effectUI.FadeBlack(effect.effectDuration);
                break;

            case ScreenEffectType.FadeFromBlack:
                effectUI.FadeFromBlack(effect.effectDuration);
                break;

            case ScreenEffectType.FlashWhite:
                effectUI.FlashWhite(effect.effectDuration);
                break;
            
            case ScreenEffectType.FlashBlack:
                effectUI.FlashBlack(effect.effectDuration);
                break;

            case ScreenEffectType.Shake:
                effectUI.ShakeScreen(effect.effectPower, effect.effectDuration);
                break;

            case ScreenEffectType.VignetteDark:
                effectUI.SetVignette(0.45f, effect.effectDuration);
                break;

            case ScreenEffectType.VignetteClear:
                effectUI.SetVignette(0f, effect.effectDuration);
                break;

            case ScreenEffectType.FlashWithShake:
                effectUI.PlayImpact(effect.effectPower, effect.effectDuration, true);
                break;
            
            case ScreenEffectType.ShakeAndSound:
                effectUI.PlayImpact(effect.effectPower, effect.effectDuration, false);
                break;
            
            case ScreenEffectType.FadeBlur:
                screenEffectUI.FadeBlur(effect.effectDuration);
                break;

            case ScreenEffectType.FadeFromBlur:
                screenEffectUI.FadeFromBlur(effect.effectDuration);
                break;
        }
    }
    
    private IEnumerator ApplyScreenEffectRoutine(ScreenEffectData effect)
    {
        if (effect == null)
            yield break;

        var effectUI = screenEffectUI;

        switch (effect.effectType)
        {
            case ScreenEffectType.None:
                yield break;

            case ScreenEffectType.FadeBlack:
                effectUI.FadeBlack(effect.effectDuration);
                break;

            case ScreenEffectType.FadeFromBlack:
                effectUI.FadeFromBlack(effect.effectDuration);
                break;

            case ScreenEffectType.FlashWhite:
                effectUI.FlashWhite(effect.effectDuration);
                break;

            case ScreenEffectType.FlashBlack:
                effectUI.FlashBlack(effect.effectDuration);
                break;
            
            case ScreenEffectType.Shake:
                effectUI.ShakeScreen(effect.effectPower, effect.effectDuration);
                break;

            case ScreenEffectType.VignetteDark:
                effectUI.SetVignette(0.45f, effect.effectDuration);
                break;

            case ScreenEffectType.VignetteClear:
                effectUI.SetVignette(0f, effect.effectDuration);
                break;

            case ScreenEffectType.FlashWithShake:
                effectUI.PlayImpact(effect.effectPower, effect.effectDuration, true);
                break;
            
            case  ScreenEffectType.ShakeAndSound:
                effectUI.PlayImpact(effect.effectPower, effect.effectDuration, false);
                break;
        }
        yield return new WaitForSeconds(effect.effectDuration);
    }
    
    private void ApplyCharacterFocusEffect(StoryLine line)
    {
        if (line.screenEffect == null)
            return;

        var slot = characterUI.FindSlot(line.characterID);
        
        if (currentScene.visualType == SceneVisualType.CharacterSlots)
        {
            if (slot == null)
                return;
        }
        

        switch (line.screenEffect.effectType)
        {
            case ScreenEffectType.ZoomIn:
                if(currentScene.visualType == SceneVisualType.CharacterSlots)
                {
                    screenEffectUI.ZoomTo(slot.SlotPos, 1.1f, line.screenEffect.effectDuration);
                    storyUI.FocusMode(true);
                }
                else if (currentScene.visualType == SceneVisualType.Illustration)
                {
                    screenEffectUI.ZoomTo(new Vector2(0.5f, 0.5f), 1.2f, line.screenEffect.effectDuration);
                    storyUI.FocusMode(true);
                }
                break;

            case ScreenEffectType.ZoomOut:
                screenEffectUI.ResetZoom(line.screenEffect.effectDuration);
                storyUI.FocusMode(false);
                break;
        }
    }
    
    private void ApplyFlip(StoryLine line)
    {
        if (line.flips == null)
            return;

        foreach (var flip in line.flips)
        {
            characterUI.SetFlip(flip.characterID, flip.flip);
        }
    }
    
    public void PlayRecallStory(int storyID)
    {
        Story story = storySO.stories.Find(x => x.id == storyID);

        if (story == null)
            return;
        
        StartCoroutine(PlaySceneTransition(() => BeginStory(story, StoryPlayMode.Recall)));
    }
    
    private void EndRecallStory()
    {
        runtimeNameOverrides.Clear();

        storyUI.Hide();
        characterUI.Hide();

        currentScene = null;
        currentStory = null;

        isPlaying = false;

        playMode = StoryPlayMode.Normal;

        SoundManager.Instance.PlayRandomBGM();
    }
    
    public Story GetStory(int storyId)
    {
        return storySO.stories.Find(s => s.id == storyId);
    }
    
    public bool IsStoryPlayed(int storyId)
    {
        return playedStoryIds.Contains(storyId);
    }

    public void SetTooltipUI(ToolTipUI tooltipUI)
    {
        this.tooltipUI = tooltipUI;
    }
    
    public StorySaveData GetSaveData()
    {
        return new StorySaveData
        {
            playedStoryIDs = playedStoryIds.ToList(),
            playedStoryHistory = playedStoryHistory.ToList(),
            flags = flags.ToList()
        };
    }
    
    public void LoadFromData(StorySaveData data)
    {
        playedStoryIds = new HashSet<int>(data.playedStoryIDs);
        playedStoryHistory = new List<int>(data.playedStoryHistory);
        flags = new HashSet<string>(data.flags);
    }
    
    public void ResetData()
    {
        playedStoryIds.Clear();
        playedStoryHistory.Clear();
        flags.Clear();

        Debug.Log("Story Reset 완료");
    }
    
    public void SetStoryUI(StoryUI ui)
    {
        storyUI = ui;
        storyUI.Init();
    }
    
    public void SetCharacterUI(CharacterUI ui)
    {
        characterUI = ui;
    }
    
    public void SetReceipt(Receipt receipt)
    {
        this.receipt = receipt;
        //receipt.onButtonConfirm += HandleDayEnd; 
    }
    
    public void SetFadePanel(StoryFadePanel panel)
    {
        fadePanel = panel;
        
        fadePanel.canvasGroup.alpha = 0f;
        fadePanel.canvasGroup.gameObject.SetActive(false);
        fadePanel.canvasGroup.blocksRaycasts = false;
    }

    public Character GetCharacter(int characterID)
    {
        foreach (var character in characterDatas)
            if (character.characterID == characterID)
                return character;
        
        return null;
    }
    
    public void SetScreenEffectUI(ScreenEffectUI ui)
    {
        screenEffectUI = ui;
    }
}
