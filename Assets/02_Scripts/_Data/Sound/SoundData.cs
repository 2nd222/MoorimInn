using UnityEngine;
using static Constants;

public abstract class SoundData : ScriptableObject
{
    public string soundName;                   // 이름(SoundManager key값)
    public AudioClip clip;                     // 사운드 파일
    [Range(0f, 1f)] public float volume = 1f;  // 개별 볼륨
}
