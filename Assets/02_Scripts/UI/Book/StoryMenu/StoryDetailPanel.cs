using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryDetailPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtTitle;
    [SerializeField] private TextMeshProUGUI txtDescription;
    
    [SerializeField] private Transform characterParent;
    [SerializeField] private StoryDetailCharacterSlot characterSlotPrefab;

    [SerializeField] private Button startButton;

    private Story currentStory;

    private void Awake()
    {
        startButton.onClick.AddListener(OnClickStart);

        gameObject.SetActive(false);
    }

    public void SetStory(Story story)
    {
        currentStory = story;

        txtTitle.text = story.storyName;
        txtDescription.text = story.description;

        RefreshCharacters(story);

        gameObject.SetActive(true);
    }

    private void RefreshCharacters(Story story)
    {
        foreach (Transform child in characterParent)
            Destroy(child.gameObject);

        foreach (int characterID in story.characterIDs)
        {
            Character data = StoryManager.Instance.GetCharacter(characterID);

            if (data != null)
            {
                StoryDetailCharacterSlot slot = Instantiate(characterSlotPrefab, characterParent);
                slot.Setup(data);
            }
        }
    }
    
    private void OnClickStart()
    {
        if (currentStory == null)
            return;

        string msg = $"{currentStory.storyName}을 다시 보시겠습니까?";

        UIManager.Instance.CreateOkPopup(
            msg,
            () =>
            {
                Time.timeScale = 1f;
                DayManager.Instance.PauseTime();
                StoryManager.Instance.StoryReplayManager.PlayStory(currentStory.id);
            },
            () => { },
            true);
    }
}
