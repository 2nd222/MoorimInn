using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Constants;

public class PostManUI : MonoBehaviour
{
    [SerializeField] private GameObject questPanel;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private NPCTalkPanel talkPanel;

    [SerializeField] private NPCQuestUI questUI;
    [SerializeField] private UpgradeUI upgradeUI;
    
    [SerializeField] private NPCMenuSlot slotPrefab;
    [SerializeField] private Transform buttonContent;

    [SerializeField] private List<NPCMenuData> menuDatas;
    [SerializeField] private CharacterSlot characterSlot;
    [SerializeField] private Character postmanCharacter;
    
    [SerializeField] private Button exitButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    
    private bool isInitialized = false;
    
    [SerializeField] private NPCDialogueData[] greetingDialogues;
    [SerializeField] private NPCDialogueData[] idleDialogues;
    
    private int lastDialogIndex = -1;

    private Postman postman;
    
    private NPCUIState currentState;
    private void SetState(NPCUIState state)
    {
        currentState = state;
        UpdateDialogue();
    }
    
    private void OnEnable()
    {
        if (postman.IsAvailableDay())
            postman.DayManager.OnDayStart += ResetDialogue;
        
        SetDefaultExpression();
        
        OpenTalk();
    }

    private void Start()
    {
        OpenTalk();
    }
    
    private void OnDisable()
    {
        if (postman.DayManager != null)
            postman.DayManager.OnDayStart -= ResetDialogue;
    }

    public void Init(Postman postman)
    {
        if (isInitialized) return;
        isInitialized = true;   
        
        this.postman = postman;
        
        characterSlot.SetCharacter(postmanCharacter, postmanCharacter.characterImage);
        
        foreach (var data in menuDatas)
        {
            var slot = Instantiate(slotPrefab, buttonContent);
        
            slot.Init(data, OnSelectMenu);
        }
        
        exitButton.onClick.AddListener(Close);
        saveButton.onClick.AddListener(OnClickSave);
        loadButton.onClick.AddListener(OnClickLoad);
        
        currentState = NPCUIState.Talk;
    }
    
    private void OnSelectMenu(NPCMenuData data)
    {
        ChangeExpression(data.expression);
    
        switch (data.state)
        {
            case NPCUIState.Upgrade:
                OpenUpgrade();
                break;
    
            case NPCUIState.Quest:
                OpenQuest();
                break;
    
            case NPCUIState.Talk:
                OpenTalk();
                break;
        }
    }

    private void ResetDialogue()
    {
        lastDialogIndex = -1;
    }
    
    private void OpenQuest()
    {
        talkPanel.gameObject.SetActive(true);
        questPanel.SetActive(true);
        upgradePanel.SetActive(false);
        
        questUI.Init(QuestType.PostMan,NPCType.PostMan);
        
        SetState(NPCUIState.Quest);
    }

    private void OpenTalk()
    {
        talkPanel.gameObject.SetActive(false);
        talkPanel.gameObject.SetActive(true);
        questPanel.SetActive(false);
        upgradePanel.SetActive(false);

        talkPanel.Clear();
        
        SetState(NPCUIState.Talk);
    }
    
    private void OpenUpgrade()
    {
        talkPanel.gameObject.SetActive(true);

        questPanel.SetActive(false);
        upgradePanel.SetActive(true);

        upgradeUI.Refresh();

        SetState(NPCUIState.Upgrade);
    }
    private void UpdateDialogue()
    {
        switch (currentState)
        {
            case NPCUIState.Quest:
                talkPanel.ChangeText("일 좀 부탁드리고 싶다는 것입니다.");
                break;

            case NPCUIState.Talk:
                var dialogueData = GetTalkDialogue();

                if (dialogueData != null)
                    ChangeDialogue(dialogueData);
                break;
            
            case NPCUIState.Upgrade:
                talkPanel.ChangeText("무림객잔 운영에 도움이 될 만한 물건들을 가져온 것입니다.");
                break;

            default:
                talkPanel.ChangeText("");
                break;
        }
    }
    
    private NPCDialogueData GetRandomDialogue(NPCDialogueData[] dialogues)
    {
        if (dialogues == null || dialogues.Length == 0)
            return null;

        int index;

        do
        {
            index = Random.Range(0, dialogues.Length);
        }
        while (dialogues.Length > 1 && index == lastDialogIndex);

        lastDialogIndex = index;

        return dialogues[index];
    }
    
    private NPCDialogueData GetTalkDialogue()
    {
        if (Random.value < 0.3f)
        {
            return GetRandomDialogue(greetingDialogues);
        }

        return GetRandomDialogue(idleDialogues);
    }
    
    private void ChangeDialogue(NPCDialogueData data)
    {
        talkPanel.ChangeText(data.text);

        if (data.expression != null)
            characterSlot.SetExpression(data.expression);
    }
    
    private void ChangeExpression(ExpressionData expression)
    {
        if (characterSlot != null)
            characterSlot.SetExpression(expression);
    }

    private void SetDefaultExpression()
    {
        characterSlot.SetExpression(new ExpressionData
        {
            face = FaceType.Neutral,
            eyebrow = EyebrowType.Default,
            useDOScale = true
        });
    }
    
    private void SetHappyExpression()
    {
        characterSlot.SetExpression(new ExpressionData
        {
            face = FaceType.Happy1,
            eyebrow = EyebrowType.Default,
            useDOScale = true
        });
    }

    private void SetConceitExpression()
    {
        characterSlot.SetExpression(new ExpressionData
        {
            face = FaceType.Conceited1,
            eyebrow = EyebrowType.Angry,
            useDOScale = true
        });
    }
    
    public void ChangeText(string text)
    {
        talkPanel.ChangeText(text);
    }
    
    private void OnClickSave()
    {
        UIManager.Instance.ActiveSaveUI(true);
    }

    private void OnClickLoad()
    {
        UIManager.Instance.ActiveLoadUI(true);
    }
    
    private void Close()
    {
        gameObject.SetActive(false);
        GameManager.Instance.CompleteState(GameFlowState.NPC);
    }
}
