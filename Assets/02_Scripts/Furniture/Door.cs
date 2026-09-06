using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public enum DoorSide
{
    LeftDoor,
    RightDoor
}

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] private DoorSide mySide;
    [SerializeField] private Transform leftDoorPart;
    [SerializeField] private Transform rightDoorPart;
    [SerializeField] private float smooth = 5f;

    public UnityAction onDoorOpened;
    public UnityAction onDoorClosed;

    private Transform movingPart;
    private Transform otherDoorPart;

    private bool isLocked = false;
    private bool isOpen = false;
    private bool isMoving = false;

    private Vector3 closePosition;
    private Vector3 openPosition;
    private Vector3 targetPosition;
    public InteractMode Mode => InteractMode.Immediate;

    void Start()
    {
        Init();
    }

    void Update()
    {
        HandleMovement();
    }

    public void Init()
    {
        if (this.mySide == DoorSide.LeftDoor)
        {
            this.movingPart = this.leftDoorPart;
            this.otherDoorPart = this.rightDoorPart;
        }
        else
        {
            this.movingPart = this.rightDoorPart;
            this.otherDoorPart = this.leftDoorPart;
        }

        this.closePosition = this.movingPart.localPosition;

        this.openPosition = new Vector3(
            this.otherDoorPart.localPosition.x,
            this.closePosition.y,
            this.closePosition.z
        );

        this.targetPosition = this.isOpen ? this.openPosition : this.closePosition;
    }

    private void HandleMovement()
    {
        if (!this.isMoving) return;

        this.movingPart.localPosition = Vector3.Lerp(
            this.movingPart.localPosition,
            this.targetPosition,
            Time.deltaTime * this.smooth
        );

        if (Vector3.Distance(this.movingPart.localPosition, this.targetPosition) < 0.01f)
        {
            this.movingPart.localPosition = this.targetPosition;
            this.isMoving = false;
        }
    }

    public bool CanInteract(Constants.PlayerType playerType)
    {
        return true;
    }

    public void Interact(PlayerController Player)
    {
        if (this.isLocked) return;

        if (this.isMoving) return;

        if (this.isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }
    /// <summary>
    /// 문 열기
    /// </summary>
    public void OpenDoor()
    {
        if (this.isLocked || this.isOpen || this.isMoving) return;

        this.isOpen = true;
        this.isMoving = true;
        this.targetPosition = this.openPosition;

        this.onDoorOpened?.Invoke();
    }
    /// <summary>
    /// 문 닫기
    /// </summary>
    public void CloseDoor()
    {
        if (this.isLocked || !this.isOpen || this.isMoving) return;

        this.isOpen = false;
        this.isMoving = true;
        this.targetPosition = this.closePosition;

        this.onDoorClosed?.Invoke();
    }
    /// <summary>
    /// 문의 잠금 상태를 변경
    /// </summary>
    public void SetLock(bool locked)
    {
        this.isLocked = locked;
    }

    public void OnInteractEnd(PlayerController player) { }

    public Vector3 GetInteractPosition()
    {
        return this.transform.position;
    }
}