using UnityEngine;
using static Constants;
public enum BgmMood
{
    Lyrical,    // 서정적인 피아노와 무림풍
    Exciting,   // 신나는 무림풍
    Sad,        // 슬픈 무림풍
    Moderate    // 신나는 무림풍
}

[CreateAssetMenu(fileName = "BGM", menuName = "SoundSO/BGM Data")]
public class BGMData : SoundData
{
    [Header("BGM 설정")]
    public BgmMood mood;
}
