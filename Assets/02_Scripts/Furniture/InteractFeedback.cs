using DG.Tweening;
using UnityEngine;

public class InteractFeedback : MonoBehaviour
{
    [Header("흔들림 연출 설정")]
    [Range(0.05f, 1f)]
    [SerializeField] private float shakeDuration = 0.2f;

    [Range(0.01f, 0.5f)]
    [SerializeField] private float shakeStrength = 0.1f;

    [Range(1, 30)]
    [SerializeField] private int shakeVibrato = 10;

    [Range(0f, 180f)]
    [SerializeField] private float shakeRandomness = 90f;

    /// <summary>
    /// 외부(로직 스크립트)에서 연출이 필요할 때 호출하는 함수
    /// </summary>
    public void PlayFeedback()
    {
        this.transform.DOKill();
        this.transform.DOShakePosition(this.shakeDuration, this.shakeStrength, this.shakeVibrato,
                                            this.shakeRandomness);
    }
}
