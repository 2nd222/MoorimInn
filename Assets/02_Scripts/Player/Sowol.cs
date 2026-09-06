using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static Constants;

public class Sowol : PlayerController
{
    private Inventory inventory;
    
    public override void OnSelected()
    {
        base.OnSelected();
    }
    public override void OnDeselected()
    {
        base.OnDeselected();
    }

    // 빈 땅 클릭은 무시, IInteractable 클릭만 이동 허용
    // 조리대 완료 등의 상태 변화는 조리대가 EndInteract()를 콜백으로 호출
    //   → 소월이 CookingStation을 폴링할 필요 없음

    private IEnumerator Start()
    {
        yield return null;
        yield return null;
        
        inventory = InventoryManager.Instance.GetInventory(InventoryType.Chef);
        DayManager.Instance.OnDayEnd += ClearInventorySlots;
    }
    
    private void OnDestroy()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayEnd -= ClearInventorySlots;
    }
    
    public override void OnSlotClicked(Slot slot)
    {
        SlotItemInfoUI.Instance?.Hide();
        
        if(slot.Owner.Type is InventoryType.IngredientsButMeatOnly or InventoryType.Ingredients)
            ClickIngredientInvenSlot(slot);
        else if(slot.Owner.Type is InventoryType.Chef)
            ClickChefInvenSlot(slot);
        else if (slot.Owner.Type is InventoryType.MealTable)
            ClickMealTableSlot(slot);
        else if (slot.Owner.Type is InventoryType.Fridge)
            ClickFridgeSlot(slot);
    }

    private void ClickIngredientInvenSlot(Slot slot)
    {
        if (slot.Item == null)
            return;
        
        // 팝업창 나와서 개수 골라서 개수 받는 기능
        
        // 조리 시스템에 개수 넘겨주기
        CookingController.Instance.OnIngredientClicked(slot);
        
        if (slot.Count <= 0)
        {
            slot.SetItem(null, 0);
        }
    }
    
    private void ClickChefInvenSlot(Slot slot)
    {
        if (slot.Item == null)
            return;

        var interactable = PlayerManager.Instance.ActiveInteractable;
        if (interactable is ServingSlot servingSlot)
        {
            if (slot.Owner.Type is InventoryType.Chef)
            {
                servingSlot.PlaceFood(inventory, slot);

                // 애니메이션
                ChangeState(PlayerState.Give);
            }
        }
        else if (interactable is Fridge fridge)
        {
            if (slot.Owner.Type is InventoryType.Chef)
            {
                fridge.PlaceFood(inventory, slot);
                ChangeState(PlayerState.Give);
            }
        }
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

            // 애니메이션
            ChangeState(PlayerState.PickUp);
        }
    }
    
    private void ClickFridgeSlot(Slot slot)
    {
        if (slot.Item == null)
            return;
        
        var interactable = PlayerManager.Instance.ActiveInteractable;
        if (interactable is Fridge fridge)
        {
            IItem foodItem = fridge.TakeFood(slot.Index, inventory);
            if (foodItem == null)
            {
                Debug.Log("가져올 음식 없음");
            }

            // 애니메이션 실행
            ChangeState(PlayerState.PickUp);
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
