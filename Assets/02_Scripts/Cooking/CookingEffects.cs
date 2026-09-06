using System;
using UnityEngine;
using UnityEngine.UI;

public class CookingEffects : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private CookingStation station;

    [Header("Animator")]
    private Animator anim;

    [Header("VFX")]
    [SerializeField] private GameObject vfxPrefabs;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip cookingClip; // 조리 중 루프 사운드
    [SerializeField] private AudioClip doneClip; // 조리 완료 효과음

    [Header("Bubble")]
    [SerializeField] private GameObject bubbleObj;
    [SerializeField] private Image bubbleImage;

    private static readonly int COOKING_TRIGGER = Animator.StringToHash("Cook");
    private static readonly int DONE_TRIGGER = Animator.StringToHash("Done");
    private static readonly int IDLE_TRIGGER = Animator.StringToHash("Idle");

    private bool wasPaused;
    
    void Start()
    {
        if (bubbleObj != null)
            bubbleObj.SetActive(false);
    }

    void OnEnable()
    {
        if (station != null)
            station.OnStateChanged += StateChanged;
    }

    void OnDisable()
    {
        if (station != null)
            station.OnStateChanged -= StateChanged;
    }
    
    private void Update()
    {
        if (audioSource == null)
            return;

        bool isPaused = DayManager.Instance != null && DayManager.Instance.IsPaused;

        if (isPaused == wasPaused)
            return;

        wasPaused = isPaused;

        if (isPaused)
        {
            if (audioSource.isPlaying)
                audioSource.Pause();
        }
        else
        {
            if (audioSource.clip != null)
                audioSource.UnPause();
        }
    }
    
    private void StateChanged(CookingState state, Sprite foodIcon)
    {
       switch (state)
        {
             case CookingState.Cooking:
                vfxPrefabs.SetActive(true);
                PlayAnimation(COOKING_TRIGGER);
                PlayLoopSound(cookingClip);
                HideBubble();
                break;
 
            case CookingState.Done:
                vfxPrefabs.SetActive(false);
                PlayAnimation(DONE_TRIGGER);
                StopLoopSound();
                PlayOneShotSound(doneClip);
                ShowBubble(foodIcon);
                break;
 
            case CookingState.Idle:
                vfxPrefabs.SetActive(false);
                PlayAnimation(IDLE_TRIGGER);
                StopLoopSound();
                HideBubble();
                break;
        } 
    }

    private void ShowBubble (Sprite foodIcon)
    {
        if (bubbleObj == null || bubbleImage == null) return;

        bubbleImage.sprite = foodIcon;
        bubbleObj.SetActive(true);
    }

    private void HideBubble ()
    {
        if (bubbleObj != null)
            bubbleObj.SetActive(false);
    }

    private void PlayAnimation(int triggerHash)
    {
        if (anim != null)
            anim.SetTrigger(triggerHash);
    }

    private void PlayLoopSound(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void PlayOneShotSound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    private void StopLoopSound()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }
}
