using UnityEngine;

public class MainStoryQuestController : Singleton<MainStoryQuestController>
{
    [SerializeField] private QuestData[] storyQuests;

    private int currentIndex = 0;

    /// <summary>
    /// 다음 퀘스트 생성
    /// </summary>
    /// <param name="inventory"></param>
    public void StartNextQuest(Inventory inventory)
    {
        if (currentIndex >= storyQuests.Length)
            return;

        var quest = new Quest(storyQuests[currentIndex], inventory);

        QuestManager.Instance.OfferQuest(quest);

        currentIndex++;
    }
}
