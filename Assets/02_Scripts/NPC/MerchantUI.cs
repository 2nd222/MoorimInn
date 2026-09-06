using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using static Constants;

public class MerchantUI : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject questPanel;
    [SerializeField] private NPCTalkPanel talkPanel;
    [SerializeField] private DebtPanel debtPanel;

    [SerializeField] private NPCQuestUI npcQuestUI;
    
    [SerializeField] private Shop shop;
    
    [SerializeField] private NPCMenuSlot slotPrefab;
    [SerializeField] private Transform buttonContent;

    [SerializeField] private List<NPCMenuData> menuDatas;
    [SerializeField] private CharacterSlot characterSlot;
    [SerializeField] private Character merchantCharacter;
    
    [SerializeField] private Button exitButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    private Button debtButton;
    
    private bool isInitialized = false;
    
    [SerializeField] private NPCDialogueData[] greetingDialogues;
    [SerializeField] private NPCDialogueData[] idleDialogues;
    
    private int lastDialogIndex = -1;
    
    public bool RepayDebtFinished { get; private set; } = false;
    
    private Merchant merchant;
    
    private NPCUIState currentState;
    private void SetState(NPCUIState state)
    {
        currentState = state;
        UpdateDialogue();
    }
    
    private void OnEnable()
    {
        lastDialogIndex = -1;
        
        
        if (merchant.IsAvailableDay())
            merchant.DayManager.OnDayStart += ResetDialogue;
        
        SetDefaultExpression();
        
        debtPanel.FullPayDebt += FullyRepayDebt;

        shop.OnProductClicked -= HandleProductClicked;
        shop.OnProductClicked += HandleProductClicked;
        
        shop.OnRecipeClicked -= HandleRecipeClicked;
        shop.OnRecipeClicked += HandleRecipeClicked;
        
        OpenTalk();
    }

    private void Start()
    {
        OpenTalk();
    }

    private void OnDisable()
    {
        if (merchant.DayManager != null)
            merchant.DayManager.OnDayStart -= ResetDialogue;

        if (debtPanel != null)
            debtPanel.FullPayDebt -= FullyRepayDebt;

        if (shop != null)
        {
            shop.OnProductClicked -= HandleProductClicked;
            shop.OnRecipeClicked -= HandleRecipeClicked;
        }
        
    }

    public void Init(Merchant merchant)
    {
        if (isInitialized) return;
        isInitialized = true;   
        
        this.merchant = merchant;
        
        shop.Init();
        characterSlot.SetCharacter(merchantCharacter, merchantCharacter.characterImage);
        
        foreach (var data in menuDatas)
        {
            if (EconomyManager.Instance.RestaurantEconomy.Debt == 0 && data.state == NPCUIState.Debt)
                return;
            
            var slot = Instantiate(slotPrefab, buttonContent);
        
            slot.Init(data, OnSelectMenu);
            
            if (data.state == NPCUIState.Debt)
                debtButton = slot.Button;
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
            case NPCUIState.Shop:
                OpenShop();
                break;
    
            case NPCUIState.Quest:
                OpenQuest();
                break;
    
            case NPCUIState.Talk:
                OpenTalk();
                break;
    
            case NPCUIState.Debt:
                OpenDebt();
                break;
        }
    }
    
    private void OpenShop()
    {
        shopPanel.SetActive(true);
        questPanel.SetActive(false);
        talkPanel.gameObject.SetActive(true);
        debtPanel.gameObject.SetActive(false);
        
        SetState(NPCUIState.Shop);
    }
    
    private void HandleProductClicked(ItemData itemData)
    {
        // 이미 최대 보유량이면 팝업 자체를 안 띄움
        int maxBuyable = shop.GetMaxBuyable(itemData);
        if (maxBuyable <= 0)
        {
            SetDisappointmentExpression();
            talkPanel.ChangeText("더 이상 들고 갈 수 없을 것 같은데?");
            UIManager.Instance.CreateOkPopup($"최대 {itemData.maxCapacity}개까지만 보유할 수 있습니다.", () => {}, () => {}, false);
            return;
        }

        int owned = shop.GetOwnedCount(itemData);

        talkPanel.ChangeText($"{itemData.itemName}을 사고 싶어?");
        UIManager.Instance.CreateAmountPopup($"{itemData.itemName}을 구매하시겠습니까?", itemData,
            owned, maxBuyable,
            (amount) =>
            {
                int totalPrice = itemData.price * amount;
            
                if (!EconomyManager.Instance.CanSpend(totalPrice))
                {
                    SetDisappointmentExpression();
                    talkPanel.ChangeText("돈이 부족한 것 같은데?");
                    UIManager.Instance.CreateOkPopup("돈이 부족합니다!", () => {}, () => {}, false);
                    return;
                }

                bool success = shop.Buy(itemData.id, amount);

                if (success)
                {
                    SetHappyExpression();
                    talkPanel.ChangeText("좋은 선택야!");
                    UIManager.Instance.CreateOkPopup("구매 완료!", () => {}, () => {}, false);
                    if(itemData is BondItemData bondItemData)
                    {
                        shop.UpdateProductList(IngredientType.Buff);
                    }
                }
            },
            () =>
            {
                SetDefaultExpression();
                talkPanel.ChangeText("천천히 골라도 돼");
                Debug.Log("취소 버튼 눌림");
            });
    }

    private void HandleRecipeClicked(RecipeData recipeData)
    {
        talkPanel.ChangeText($"{recipeData.recipeName} 비급을 사고 싶어?");
        UIManager.Instance.CreateAmountPopup($"{recipeData.recipeName} 조리법을 구매하시겠습니까?", recipeData,
            (amount) =>
            {
                if (!EconomyManager.Instance.CanSpend(recipeData.price))
                {
                    SetDisappointmentExpression();
                    talkPanel.ChangeText("돈이 부족한 것 같은데?");
                    UIManager.Instance.CreateOkPopup("돈이 부족합니다!", () => {}, () => {}, false);
                    return;
                }

                EconomyManager.Instance.SpendMoney(recipeData.price);
                RecipeUnlockManager.Instance.Unlock(recipeData.id);

                SetHappyExpression();
                talkPanel.ChangeText("좋은 선택이야!");
                UIManager.Instance.CreateOkPopup("레시피 해금!", ()=>{}, () => {}, false);
                shop.UpdateProductList(IngredientType.Recipe);
            },
            () =>
            {
                SetDefaultExpression();
                talkPanel.ChangeText("천천히 골라도 돼");
                Debug.Log("취소 버튼 눌림");
            });
    }
    
    private void OpenQuest()
    {
        shopPanel.SetActive(false);
        questPanel.SetActive(true);
        talkPanel.gameObject.SetActive(true);
        debtPanel.gameObject.SetActive(false);
        
        npcQuestUI.Init(QuestType.Merchant, NPCType.Merchant);
        
        SetState(NPCUIState.Quest);
    }

    private void OpenTalk()
    {
        talkPanel.gameObject.SetActive(false);
        shopPanel.SetActive(false);
        questPanel.SetActive(false);
        talkPanel.gameObject.SetActive(true);
        debtPanel.gameObject.SetActive(false);

        talkPanel.Clear();
        
        SetState(NPCUIState.Talk);
    }
    
    private void OpenDebt()
    {
        shopPanel.SetActive(false);
        questPanel.SetActive(false);
        talkPanel.gameObject.SetActive(true);
        debtPanel.gameObject.SetActive(true);
        
        SetState(NPCUIState.Debt);
    }
    
    private void UpdateDialogue()
    {
        switch (currentState)
        {
            case NPCUIState.Shop:
                talkPanel.ChangeText("좋은 물건 많아~ 구경하고 가");
                break;

            case NPCUIState.Quest:
                talkPanel.ChangeText("일 좀 부탁해도 될까? 보상은 할게");
                break;

            case NPCUIState.Talk:
                var dialogueData = GetTalkDialogue();

                if (dialogueData != null)
                    ChangeDialogue(dialogueData);
                break;
            
            case NPCUIState.Debt:
                talkPanel.ChangeText("돈을 갚고 싶어?");
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

    private void FullyRepayDebt()
    {
        RepayDebtFinished = true;
        debtPanel.gameObject.SetActive(false);
        debtButton.gameObject.SetActive(false);
    }
    
    private void ChangeExpression(ExpressionData expression)
    {
        if (characterSlot != null)
            characterSlot.SetExpression(expression);
    }

    public void SetDefaultExpression()
    {
        characterSlot.SetExpression(new ExpressionData
        {
            face = FaceType.Neutral,
            eyebrow = EyebrowType.Default,
            useDOScale = true
        });
    }
    
    public void SetHappyExpression()
    {
        characterSlot.SetExpression(new ExpressionData
        {
            face = FaceType.Happy1,
            eyebrow = EyebrowType.Surprised,
            useDOScale = true
        });
    }

    public void SetSurprisedExpression()
    {
        characterSlot.SetExpression(new ExpressionData
        {
            face = FaceType.Surprised1,
            eyebrow = EyebrowType.Surprised,
            useDOScale = true
        });
    }
    
    public void SetSadExpression()
    {
        characterSlot.SetExpression(new ExpressionData
        {
            face = FaceType.Sad1,
            eyebrow = EyebrowType.Sad,
            useDOScale = true
        });
    }

    public void SetDisappointmentExpression()
    {
        characterSlot.SetExpression(new ExpressionData
        {
            face = FaceType.Disappointment1,
            eyebrow = EyebrowType.Disappointment,
            useDOScale = true
        });
    }

    public void ChangeText(string text)
    {
        talkPanel.ChangeText(text);
    }
    
    private void Close()
    {
        gameObject.SetActive(false);
        GameManager.Instance.CompleteState(GameFlowState.NPC);
    }
    
    private void OnClickSave()
    {
        UIManager.Instance.ActiveSaveUI(true);
    }

    private void OnClickLoad()
    {
        UIManager.Instance.ActiveLoadUI(true);
    }
    
    private void ResetDialogue()
    {
        lastDialogIndex = -1;
    }
}
