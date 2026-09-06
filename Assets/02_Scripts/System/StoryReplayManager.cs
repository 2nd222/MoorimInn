using System.Collections.Generic;
using UnityEngine;

public class StoryReplayManager : MonoBehaviour
{
    [SerializeField] private StorySO storySO;
    
    public IReadOnlyList<int> PlayedStoryHistory => StoryManager.Instance.PlayedStoryHistory;

    public Story GetStory(int storyID)
    {
        return storySO.stories.Find(s => s.id == storyID);
    }

    public void PlayStory(int storyID)
    {
        StoryManager.Instance.PlayRecallStory(storyID);
    }
}
