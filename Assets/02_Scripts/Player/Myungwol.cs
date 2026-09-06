using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using static Constants;

public class Myungwol : PlayerController
{
    private Inventory inventory;
    public override float SpeedMultiplier => 1.5f;
    
    // 인벤토리 (인벤토리 담당자가 구현 시 타입 교체)
    // TODO: 인벤토리 담당자와 협의 후 실제 Inventory 컴포넌트로 교체
    // [SerializeField] private Inventory inventory;
    // public Inventory Inventory => inventory;

    private IEnumerator Start()
    {
        yield return null;
        yield return null;
        
        inventory = InventoryManager.Instance.GetInventory(InventoryType.Server);
        DayManager.Instance.OnDayEnd += ClearInventorySlots;
    }
    
    private void OnDestroy()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayEnd -= ClearInventorySlots;
    }

    public override void OnSelected()
    {
        base.OnSelected();
        // 명월 전용 UI 활성화 등
    }
 
    public override void OnDeselected()
    {
        base.OnDeselected();
        // 명월 전용 UI 비활성화 등
    }

    public override void OnSlotClicked(Slot slot)
    {
        base.OnSlotClicked(slot);
        // 명월: 손님에게 음식 전달 등
        SlotItemInfoUI.Instance?.Hide();
        
        if (slot.Owner.Type is InventoryType.MealTable)
            ClickMealTableSlot(slot);
        else if(slot.Owner.Type is InventoryType.Server)
            ClickServerSlot(slot);
    }

    private void ClickMealTableSlot(Slot slot)
    {
        if (slot.Item == null)
            return;

        var interactable = PlayerManager.Instance.ActiveInteractable;
        if (interactable is ServingSlot servingSlot)
        {
            IItem foodItem = servingSlot.TakeFood(slot.Index, inventory);
            if (foodItem == null)
            {
                Debug.Log("가져올 음식 없음");
                return;
            }

            // 애니메이션 실행
            ChangeState(PlayerState.PickUp);
        }
    }

    public void ClickServerSlot(Slot slot)
    {
        if (slot.Item == null)
            return;

        var interactable = PlayerManager.Instance.ActiveInteractable;
        if (interactable is ServingSlot servingSlot)
        {
            if (slot.Owner.Type is InventoryType.Server)
            {
                servingSlot.PlaceFood(inventory, slot);
                ChangeState(PlayerState.Give);
            }
        }
        else if (interactable is GuestInteractable guest)
        {
            if (slot.Owner.Type is InventoryType.Server)
            {
                // TODO: 팝업 UI 호출
                // "이 음식을 전달하시겠습니까?" 확인 시 아래 실행
                guest.DeliverFood(inventory, slot);
                ChangeState(PlayerState.Give);
            }
        }
    }
    
    private void ClearInventorySlots()
    {
        if (inventory == null) return;
        foreach (var slot in inventory.Slots)
        {
            if (slot == null) continue;
            
            slot.Clear();
        }
    }
}
