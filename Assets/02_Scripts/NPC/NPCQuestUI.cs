using System.Collections.Generic;
using UnityEngine;

public class NPCQuestUI : MonoBehaviour
{
    [Header("왼쪽 리스트")]
    [SerializeField] private Transform listContent;
    [SerializeField] private GameObject slotPrefab;

    [Header("오른쪽 상세")]
    [SerializeField] private NPCQuestDetailPanel detailPanel;
    
    private RecipeUnlockManager recipeUnlockManager;
    private IngredientUnlockManager ingredientUnlockManager;
    
    private List<Quest> quests = new List<Quest>();
    private Inventory inventory;
    
    private Quest selectedQuest;
    private NPCQuestSlot currentSelectedSlot;
    
    private QuestType currentQuestType;
    private NPCType currentNPCType;
    public void Init(QuestType questType, NPCType npcType)
    {
        inventory = InventoryManager.Instance.GetInventory(Constants.InventoryType.Fridge);
        
        currentQuestType = questType;
        currentNPCType = npcType;
        
        recipeUnlockManager = RecipeUnlockManager.Instance;
        ingredientUnlockManager = IngredientUnlockManager.Instance;
        
        selectedQuest = null;
        currentSelectedSlot = null;
        quests.Clear();

        // 수락 가능
        quests.AddRange(QuestManager.Instance.GetWaitingQuests(questType));
        // 진행중 + 클리어 대상
        quests.AddRange(QuestManager.Instance.GetQuestsByNPC(npcType));
        // 메인, 서브 퀘스트
        quests.AddRange(QuestManager.Instance.GetQuests(QuestType.MainStory));
        quests.AddRange(QuestManager.Instance.GetQuests(QuestType.SubStory));

        RefreshList();
        detailPanel.Clear();
    }

    private void RefreshList()
    {
        foreach (Transform child in listContent)
            Destroy(child.gameObject);

        foreach (var quest in quests)
        {
            Debug.Log($"{quest.QuestName} / Accepted : {quest.IsAccepted}");
            
            var obj = Instantiate(slotPrefab, listContent);
            var slot = obj.GetComponent<NPCQuestSlot>();

            slot.Init(quest, OnClickQuest);
        }
    }
    
    private void OnClickQuest(Quest quest, NPCQuestSlot slot)
    {
        // 이전 선택 해제
        if (currentSelectedSlot != null)
            currentSelectedSlot.SetSelected(false);

        // 현재 선택
        currentSelectedSlot = slot;
        currentSelectedSlot.SetSelected(true);

        selectedQuest = quest;
        detailPanel.Show(quest, this);
    }

    public void AcceptQuest()
    {
        if (selectedQuest == null) return;

        QuestManager.Instance.AcceptQuest(selectedQuest);

        Init(currentQuestType, currentNPCType);
    }

    public void ClearQuest()
    {
        if (selectedQuest == null) return;

        if (!selectedQuest.CanClear())
            return;
        
        if (!selectedQuest.CanRemoveItems(inventory))
        {
            Debug.Log("아이템 부족");
            return;
        }
        selectedQuest.CostForQuestClear(inventory);
        
        // 완료 처리
        selectedQuest.Complete();;

        if (selectedQuest.RewardRecipeIds != null)
        {
            foreach (var recipeId in selectedQuest.RewardRecipeIds)
            {
                recipeUnlockManager.Unlock(recipeId);
            }
        }

        if (selectedQuest.RewardIngredientIds != null)
        {
            foreach (var ingredientId in selectedQuest.RewardIngredientIds)
            {
                ingredientUnlockManager.Unlock(ingredientId);
            }
        }
        
        
        Init(currentQuestType, currentNPCType);
    }
}
