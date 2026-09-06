using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Constants;

[RequireComponent(typeof(Collider))]
public class Fridge : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform movePoint;
    [SerializeField] private Inventory inventory;
    [SerializeField] private PopupController ui;

    [SerializeField] private InteractFeedback feedback;

    public InteractMode Mode => InteractMode.AutoOnEnter;

    private void Start()
    {
        //DayManager dayManager = FindFirstObjectByType<DayManager>();
        DayManager dayManager = DayManager.Instance;

        dayManager.OnDayEnd += OnDayEnd;
        dayManager.OnDayStart += OnDayStart;
    }
    
    private void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayEnd -= OnDayEnd;
            DayManager.Instance.OnDayStart -= OnDayStart;
        }

        if (inventory != null)
            inventory.OnCloseRequested -= HandleInventoryCloseRequest;
    }

    public void Interact(PlayerController player)
    {
        Debug.Log("Fridge Interact 호출됨");

        // 상호작용 로직
        InventoryManager.Instance.OpenInventory(this.inventory);
        //this.feedback.PlayFeedback();
        UIAnimationManager.Instance.ShowPopFromWorld(this.ui.PanelTransform, this.transform);
        
        this.inventory.OnCloseRequested -= HandleInventoryCloseRequest;
        this.inventory.OnCloseRequested += HandleInventoryCloseRequest;
    }

    public void OnInteractEnd(PlayerController player)
    {
        // 종료 시 정리 로직
        HandleInventoryCloseRequest();
    }
    private void HandleInventoryCloseRequest()
    {
        this.inventory.OnCloseRequested -= HandleInventoryCloseRequest;

        UIAnimationManager.Instance.HidePopToWorld(this.ui.PanelTransform, this.transform, 0.3f,
            onComplete: () =>
            {
                InventoryManager.Instance.CloseInventory(this.inventory);
            });
    }
    private void OnDayEnd()
    {
        foreach (var slot in inventory.Slots)
        {
            if (slot.Item == null)
                continue;

            slot.Item.FridgeSpoilTimer++;
            slot.Item.FreshState++;
            Debug.Log($"{slot.Item.ItemName} → {slot.Item.FridgeSpoilTimer}");
        }
        
        if (this == null || inventory == null) return;
        
        if (inventory.gameObject.activeInHierarchy)
            HandleInventoryCloseRequest();
    }
    
    private void OnDayStart()
    {
        List<Slot> removeSlots = new();

        foreach (var slot in inventory.Slots)
        {
            if (slot.Item == null)
                continue;

            if (slot.Item.FreshState == FreshState.None)
                removeSlots.Add(slot);
        }

        foreach (var slot in removeSlots)
        {
            Debug.Log($"{slot.Item.ItemName} 완전히 상함 → 제거");
            inventory.RemoveItem(slot.Item.ID, slot.Count);
        }
    }
    
    /// <summary>
    /// 냉장고에 음식을 놓는 상황
    /// </summary>
    /// <param name="foodData"></param>
    public void PlaceFood(Inventory sourceInven, Slot sourceSlot)
    {
        SetFreshness(sourceSlot);
        sourceInven.TryMoveToInventory(inventory, sourceSlot, sourceSlot.Count);
    }
    
    /// <summary>
    /// 냉장고에서 음식을 꺼내는 상황
    /// </summary>
    /// <returns></returns>
    public IItem TakeFood(int slotIndex, Inventory targetInven)
    {
        SlotItemInfoUI.Instance?.Hide();
        
        var slot = inventory.Slots[slotIndex];
        if (slot.Item == null)
        {
            Debug.Log("음식 없음");
            return null;
        }

        IItem item = slot.Item;

        inventory.TryMoveOneToInventory(targetInven, slot);
        
        return item;
    }

    public Vector3 GetInteractPosition()
    {
        return movePoint.position;
    }
    public bool CanInteract(Constants.PlayerType playerType)
    {
        return playerType == PlayerType.Sowol;
    }

    private void SetFreshness(Slot slot)
    {
        if (slot.Item.FreshState == FreshState.Good)
            slot.Item.FridgeSpoilTimer = 0;
        else if (slot.Item.FreshState == FreshState.Normal)
            slot.Item.FridgeSpoilTimer = 1;
        else if (slot.Item.FreshState == FreshState.Bad)
            slot.Item.FridgeSpoilTimer = 2;
    }
}