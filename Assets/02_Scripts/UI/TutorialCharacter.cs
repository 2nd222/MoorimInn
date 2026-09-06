using UnityEngine;

public class TutorialCharacter : MonoBehaviour
{
    [SerializeField] private CharacterSlot characterSlot;
    [SerializeField] private TutorialDialogueUI dialogueUI;

    public void Setup(Character character)
    {
        characterSlot.SetCharacter(character, character.characterImage);
    }

    public void Say(string text)
    {
        //dialogueUI.Show(characterSlot.CurrentCharacter.characterName, text);
        
        Debug.Log($"[TutorialCharacter] Say 호출됨. 전달받은 대사: {text}");

        if (dialogueUI == null) 
        { 
            Debug.LogError("[TutorialCharacter] 🚨 dialogueUI가 연결되지 않았습니다!"); 
            return; 
        }

        if (characterSlot == null || characterSlot.CurrentCharacter == null) 
        { 
            Debug.LogError("[TutorialCharacter] 🚨 characterSlot에 CurrentCharacter가 없습니다! Setup()이 제대로 실행되지 않았을 수 있습니다."); 
            return; 
        }

        Debug.Log($"[TutorialCharacter] dialogueUI.Show 호출. 화자 이름: {characterSlot.CurrentCharacter.characterName}");
        dialogueUI.Show(characterSlot.CurrentCharacter.characterName, text);
    }

    public void SetExpression(FaceType face, EyebrowType eyebrow = EyebrowType.None, bool useScale = true)
    {
        ExpressionData data = new ExpressionData()
        {
            face = face,
            eyebrow = eyebrow,
            useDOScale = useScale
        };

        characterSlot.SetExpression(data);
    }

    public void PlayEmotion(Emotion emotion)
    {
        Sprite icon = characterSlot.CurrentCharacter.emotionIcons.Find(x => x.emotion == emotion)?.icon;

        if (icon != null)
            characterSlot.PlayEmotionIcon(emotion);
    }
}
