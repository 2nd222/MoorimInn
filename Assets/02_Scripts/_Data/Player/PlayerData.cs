using UnityEngine;
using static Constants;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Player/PlayerData")]
public class PlayerData : ScriptableObject
{
    public Sprite icon;
    public PlayerType type;
    public string like;
    public string dislike;
    public string skill;
}
