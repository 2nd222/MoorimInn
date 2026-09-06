using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoryReplayTab : BaseTab
{
    [SerializeField] private StoryReplaySlot slotPrefab;
    [SerializeField] private Transform contentParent;
    
    [SerializeField] private StoryDetailPanel detailPanel;
    
    private StoryReplaySlot currentSelectedSlot;
    
    public void Refresh()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        List<int> sortedStoryIds = GetSortedStoryIds();

        foreach (int storyID in sortedStoryIds)
        {
            Story story = StoryManager.Instance.StoryReplayManager.GetStory(storyID);

            if (story == null)
                continue;

            StoryReplaySlot slot =
                Instantiate(slotPrefab, contentParent);

            slot.Setup(story, detailPanel, this);
        }
        
        detailPanel.gameObject.SetActive(false);
        currentSelectedSlot = null;
    }
    
    /// <summary>
    /// StorySequence에 설정된 순서에 따라
    /// 실제 플레이된 Story ID들을 정렬한다.
    /// </summary>
    private List<int> GetSortedStoryIds()
    {
        var replayManager = StoryManager.Instance.StoryReplayManager;

        var storyManager = StoryManager.Instance;

        var playedStoryIds = replayManager.PlayedStoryHistory;

        var sequence = storyManager.StorySequence;

        if (sequence == null || sequence.storyIds == null)
        {
            return new List<int>(playedStoryIds);
        }

        return playedStoryIds
            .OrderBy(id =>
            {
                int index = sequence.storyIds.IndexOf(id);

                // Sequence에 없는 Story는 가장 뒤로
                return index >= 0
                    ? index
                    : int.MaxValue;
            })
            .ToList();
    }
    
    public void SelectSlot(StoryReplaySlot slot)
    {
        if (currentSelectedSlot != null)
            currentSelectedSlot.SetSelected(false);

        currentSelectedSlot = slot;

        if (currentSelectedSlot != null)
            currentSelectedSlot.SetSelected(true);
    }
    
    public override void SetupData()
    {
        
    }

    public override void ResetSetting()
    {
        Refresh();
    }
}
