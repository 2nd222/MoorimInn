using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Constants;

// TODO: 인벤토리랑 소통해서 슬롯에서 클릭 시 조리 화면에 아이템 개수 전송

/// <summary>
/// UI상에서 슬롯에 있는 아이템 시각적으로 표시하고 인벤토리에 어떤 아이템이 담겨져있는지 알려주기 위한 클래스, 인벤토리하고만 소통
/// </summary>
public class Slot : MonoBehaviour, IBeginDragHandler, IDragHandler, IDropHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Inventory Owner { get; private set; }
    
    public IItem Item { get; private set; }
    public int Count {get; private set;}
    
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image slotIcon;
    [SerializeField] private Button slotButton;
    
    private static Slot dragSlot;             
    // dragSlot은 옮겨지는 슬롯을 참조한다. class, monobehavior, GameObject, Transform 등등은 참조타입
    
    public bool IsEmpty { get; private set; } = true;
    
    public Image SlotIcon => slotIcon;

    public event Action<Slot, Slot> OnSlotDrop; // 드래그 중인 아이템을 슬롯에서 놓을 때 호출 용
    
    public event Action<Slot> OnSlotClick;

    public int Index { get; private set; }
    
    // 디버그용
    [SerializeField] private ItemData debugItemData;
    [SerializeField] private int debugCount;
    private void Update()
    {
        if (Item != null)
        {
            debugItemData = Item.Data;
            debugCount = Count;
        }
        else
        {
            debugItemData = null;
            debugCount = 0;
        }
    }
    
    
    private void OnEnable()
    {
        slotIcon.gameObject.SetActive(!IsEmpty);
        
        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(OnClickSlotAction);
    }

    public void SetOwner(Inventory owner)
    {
        Owner = owner;
    }

    public void SetIndex(int index)
    {
        Index = index;
    }
    
    public void AddItem(IItem item, int count)
    {
        this.Item = item;
        IsEmpty = false;
        slotIcon.sprite = item.Icon;
        Count = count;
        countText.text = Count.ToString();
        
        slotIcon.gameObject.SetActive(true);
    }

    private void OnClickSlotAction()
    {
        OnSlotClick?.Invoke(this);
    }

    public void OnBeginDrag(PointerEventData eventData)  // 드래그 시작
    {
        if (Owner.IsReadOnly)
            return;
        
        if (IsEmpty)
            return;

        dragSlot = this;  // 참조 설정 dragslot을 통해 원래 슬롯에 접근
        DragUI.Instance.StartDrag(Item.Icon);
    }

    public void OnDrag(PointerEventData eventData) // 드래그 중
    {
        if (Owner.IsReadOnly)
            return;
        
        DragUI.Instance.UpdatePosition(eventData.position);
    }
    
    public void OnDrop(PointerEventData eventData) // 놓은 경우
    {
        if (Owner.IsReadOnly)
            return;
        
        if (dragSlot == null || dragSlot == this)
            return;
        
        OnSlotDrop?.Invoke(this, dragSlot);
        
        EndDrag();
    }
    
    public void SetItem(IItem newItem, int newCount)
    {  
        this.Item = newItem;
        Count = newCount;
        
        if (slotIcon == null)
            return;

        if (slotIcon.gameObject == null)
            return;
        
        if (newItem == null)
        {
            IsEmpty = true;
            Count = 0;
            countText.text = Count.ToString();
            slotIcon.sprite = null;
            slotIcon.gameObject.SetActive(false);
        }
        else
        {
            IsEmpty = false;
            slotIcon.sprite = newItem.Icon;
            countText.text = Count.ToString();
            slotIcon.gameObject.SetActive(true);
        }
        
        if (Owner.Type == Constants.InventoryType.BondItem)
        {
            countText.gameObject.SetActive(false);
        }
    }

    public void AddCount(int add)
    {
        Count += add;
        countText.text = Count.ToString();
    }
    
    public void Clear()
    {
        SetItem(null, 0);
    }
    
    private void EndDrag()
    {
        DragUI.Instance.EndDrag();
        dragSlot = null;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EndDrag();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (dragSlot != null)
            return;
        
        if (IsEmpty)
            return;

        if (Owner.Type != InventoryType.Fridge 
            && Owner.Type != InventoryType.MealTable 
            && Owner.Type != InventoryType.Chef 
            && Owner.Type != InventoryType.Server)
            return;

        SlotItemInfoUI.Instance?.Show(Item, Owner.Type, (RectTransform)transform);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        SlotItemInfoUI.Instance?.Hide();
    }
}
