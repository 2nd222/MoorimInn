using System;
using System.Collections.Generic;
using UnityEngine;

public class SubStoryQuestController : Singleton<SubStoryQuestController>
{
    /// <summary>
    /// 서브 스토리 시작 (NPC 대화 등에서 호출)
    /// </summary>
    public void StartSubStory(QuestData startQuestData, Inventory inventory)
    {
        if (startQuestData == null)
            return;

        // 중복 방지
        if (QuestManager.Instance.HasQuest(startQuestData.id))
            return;

        var quest = new Quest(startQuestData, inventory);
        QuestManager.Instance.OfferQuest(quest);
    }
    
    /// <summary>
    /// 서브 퀘스트 완료 시 다음 퀘스트 연결
    /// </summary>
    public void OnQuestCompleted(Quest quest, Inventory inventory)
    {
        if (quest.QuestType != QuestType.SubStory)
            return;

        var next = quest.Data.nextQuest;

        if (next == null)
            return;

        // 중복 방지
        if (QuestManager.Instance.HasQuest(next.id))
            return;

        var nextQuest = new Quest(next, inventory);
        QuestManager.Instance.OfferQuest(nextQuest);
    }
}
