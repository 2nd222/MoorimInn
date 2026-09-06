using System;
using DG.Tweening;
using UnityEngine;
using static Constants; 

public class Gate : MonoBehaviour, IInteractable 
{
    // 씬 단위 싱글톤 (씬 로드마다 새로 등록됨)
    public static Gate Instance { get; private set; }

    [Header("Door Settings")] 
    public GameObject doorHandle0;
    public GameObject doorHandle1;
    public float openDoorTime = 1f;
    
    private Tween doorTween;
    
    public bool IsOpen { get; private set; } = true; // 인스턴스 변수: 씬 로드 시 자동 초기화

    public InteractMode Mode => InteractMode.Immediate; 
    
    public event Action<bool> OnGateStateChanged; // 인스턴스 이벤트

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        ForceOpen();
        DayManager.Instance.OnDayEnd += ForceOpen;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (DayManager.Instance != null)
            DayManager.Instance.OnDayEnd -= ForceOpen;

        doorTween?.Kill();
    }

    // 트윈 없이 즉시 열림 (하루 종료 / 씬 시작용)
    private void ForceOpen()
    {
        doorTween?.Kill();

        IsOpen = true;
        doorHandle0.transform.localRotation = Quaternion.Euler(0, 90, 0);
        doorHandle1.transform.localRotation = Quaternion.Euler(0, -90, 0);

        OnGateStateChanged?.Invoke(true);
    }

    public bool CanInteract(PlayerType playerType) => true;

    public void Interact(PlayerController player)
    {
        if (doorTween != null && doorTween.IsActive() && doorTween.IsPlaying()) 
            return;

        if (IsOpen)
            CloseDoor();
        else
            OpenDoor();
    }

    public void OnInteractEnd(PlayerController player) { }

    public Vector3 GetInteractPosition() => transform.position;
    
    private void OpenDoor()
    {
        IsOpen = true; 
        doorTween?.Kill();
        
        Sequence s = DOTween.Sequence();
        s.Join(doorHandle0.transform.DOLocalRotate(new Vector3(0, 90, 0), openDoorTime).SetEase(Ease.OutQuad));
        s.Join(doorHandle1.transform.DOLocalRotate(new Vector3(0, -90, 0), openDoorTime).SetEase(Ease.OutQuad));
        s.OnComplete(() => OnGateStateChanged?.Invoke(IsOpen));
        
        doorTween = s;
    }

    private void CloseDoor()
    {
        IsOpen = false; 
        doorTween?.Kill();
        
        Sequence s = DOTween.Sequence();
        s.Join(doorHandle0.transform.DOLocalRotate(Vector3.zero, openDoorTime).SetEase(Ease.OutQuad));
        s.Join(doorHandle1.transform.DOLocalRotate(Vector3.zero, openDoorTime).SetEase(Ease.OutQuad));
        s.OnComplete(() => OnGateStateChanged?.Invoke(IsOpen));
        
        doorTween = s;
    }
}