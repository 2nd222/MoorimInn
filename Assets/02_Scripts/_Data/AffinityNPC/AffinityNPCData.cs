using UnityEngine;


[CreateAssetMenu(fileName = "AffinityNPC", menuName = "Scriptable Objects/AffinityNPC")]
public class AffinityNPCData : ScriptableObject
{
    public int id;
    public Sprite icon;
    public Sprite profile;
    public string myName;
    [TextArea(3, 10)]
    public string description;
    public string[] likes;
    public string[] dislikes;
    
    public StoryTriggerSO storyTriggers; // 캐릭터 스토리 체크용
}
