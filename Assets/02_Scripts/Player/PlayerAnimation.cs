using DG.Tweening;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("컴포넌트 연결")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Animator animator;

    private readonly int hashSpeed = Animator.StringToHash("Speed");
    private readonly int hashPickup = Animator.StringToHash("PickUp");
    private readonly int hashGive = Animator.StringToHash("Give");
    private readonly int hashInteract = Animator.StringToHash("Interact");

    void Awake()
    {
        this.animator = GetComponent<Animator>();
        this.player.OnStateChanged += HandleStateChanged;
    }
    
    private void HandleStateChanged(PlayerState state)
    {
        if (state == PlayerState.Idle || state == PlayerState.Walk || state == PlayerState.Run)
        {
            DOTween.Kill(this.gameObject.GetInstanceID());
        }
        switch (state)
        {
            case PlayerState.Idle: 
                PlayIdle(); 
                break;
            case PlayerState.Walk: 
                PlayWalk(); 
                break;
            case PlayerState.Run: 
                PlayRun(); 
                break;
            case PlayerState.PickUp: 
                PlayPickUp(); 
                break;
            case PlayerState.Give: 
                PlayGive(); 
                break;
            case PlayerState.Interact: 
                PlayInteract();
                break;
        }
    }
    private void PlayIdle()
    {
        float currentSpeed = this.animator.GetFloat(this.hashSpeed);
        DOVirtual.Float(currentSpeed, 0f, 0.1f, (value) =>
        {
            this.animator.SetFloat(this.hashSpeed, value);
        }).SetId(this.gameObject.GetInstanceID());
    }

    private void PlayWalk()
    {
        this.animator.SetFloat(this.hashSpeed, 0.5f);
    }

    private void PlayRun()
    { 
        this.animator.SetFloat(this.hashSpeed, 1f);
    }

    private void PlayPickUp()
    {
        this.animator.SetTrigger(this.hashPickup);
    }

    private void PlayGive()
    {
        this.animator.SetTrigger(this.hashGive);
    }

    private void PlayInteract()
    {
        this.animator.SetTrigger(this.hashInteract);
    }
}
