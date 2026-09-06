using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryDetailCharacterSlot : MonoBehaviour
{
    [SerializeField] private Image portrait;
    [SerializeField] private TextMeshProUGUI txtName;

    public void Setup(Character character)
    {
        txtName.text = character.characterName;
        portrait.sprite = character.characterImage;
    }
}
