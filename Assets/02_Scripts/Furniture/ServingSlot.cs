using System.Collections;
using System.Net;
using UnityEngine;
using static Constants;

public class ServingSlot : MonoBehaviour, IInteractable
{
    [Header("위치 관련 변수")]
    [SerializeField] 
    private Transform servingPoint;
    [SerializeField]
    private Transform foodPosition; 

    // 음식 오브젝트 소환용
    private GameObject objFoodPrefab;
    //private GameObject currentFood;

    [SerializeField] private Inventory inventory;
    [SerializeField] private PopupController ui;

    public InteractMode Mode => InteractMode.AutoOnEnter;
    
    private void Start()
    {
        inventory.OnItemAdded += OnItemAddedToServing;
        DayManager.Instance.OnDayEnd += HandleDayEnd;
    }
    
    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnItemAdded -= OnItemAddedToServing;

        if (DayManager.Instance != null)
            DayManager.Instance.OnDayEnd -= HandleDayEnd;
    }
    
    private void Update()
    {
        if (DayManager.Instance.IsPaused) return;
        
        foreach (var slot in inventory.Slots)
        {
            if (slot.Item == null)
                continue;
            
            slot.Item.SpoilTimer += Time.deltaTime;

            if (slot.Item.SpoilTimer >= SPOIL_TIME * 1 / 3)
            {
                if (slot.Item.FreshState is FreshState.Good)
                {
                    slot.Item.FreshState = FreshState.Normal;
                    //Debug.Log($"신선도 {slot.Item.FreshState}으로 변경");
                }
            }

            if (slot.Item.SpoilTimer >= SPOIL_TIME * 2 / 3)
            {
                if (slot.Item.FreshState == FreshState.Normal)
                {
                    slot.Item.FreshState = FreshState.Bad;
                    //Debug.Log($"신선도 {slot.Item.FreshState}으로 변경");
                }
            }
            
            if (slot.Item.SpoilTimer >= SPOIL_TIME)
            {
                Debug.Log($"{slot.Item.ItemName} 상함");

                SlotItemInfoUI.Instance?.Hide();
                inventory.RemoveItem(slot.Item.ID, slot.Count);

                if (objFoodPrefab != null)
                {
                    Destroy(objFoodPrefab);
                }
            }
            //Debug.Log($"{slot.Index}번 슬롯 Spoil Timer : {slot.SpoilTimer}");
        }
    }

    // 시그니처 변경 (내용은 기존 로직 유지하시면서 player 활용)
    public void Interact(PlayerController player)
    {
        Debug.Log("Serving Interact 호출됨");

        // 상호작용 로직
        InventoryManager.Instance.OpenInventory(this.inventory);
        //this.feedback.PlayFeedback();
    }

    public void OnInteractEnd(PlayerController player)
    {
        // 종료 시 정리 로직
        InventoryManager.Instance.CloseInventory(this.inventory);
    }


    /// <summary>
    /// 배식대에 음식을 놓는 상황
    /// </summary>
    /// <param name="foodData"></param>
    public void PlaceFood(Inventory sourceInven, Slot sourceSlot)
    {
        IItem item = sourceSlot.Item;

        if (item == null)
            return;

        Slot targetSlot = sourceInven.TryMoveToInventory(inventory, sourceSlot, sourceSlot.Count);
    
        if (targetSlot == null)
        {
            Debug.Log("배식대에 자리가 없어 음식을 놓지 못함");
            return;
        }
    
        SetFreshness(item);         
        OnItemAddedToServing(targetSlot);
    }
    
    /// <summary>
    /// 배식대에서 음식을 꺼내는 상황
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

        if (objFoodPrefab != null)
        {
            Destroy(this.objFoodPrefab);
            objFoodPrefab = null;
        }
        //currentFood = null;
        
        return item;
    }
    
    private void OnItemAddedToServing(Slot slot)
    {
        if (slot == null || slot.Item == null)
            return;
        
        // 기존 음식 제거 (중복 방지)
        if (objFoodPrefab != null)
        {
            Destroy(objFoodPrefab);
        }

        // 프리팹 가져오기
        FoodData foodData = slot.Item.Data as FoodData;

        if (foodData == null || foodData.foodPrefab == null)
        {
            Debug.LogWarning("foodPrefab 없음");
            return;
        }

        // 스폰
        objFoodPrefab = Instantiate(
            foodData.foodPrefab,
            foodPosition.position,
            Quaternion.identity,
            this.transform // 선택: 배식대 자식으로
        );
    }
    
    private void HandleDayEnd()
    {
        if (this == null || inventory == null) return;
        
        if (inventory.gameObject.activeInHierarchy)
            InventoryManager.Instance.CloseInventory(inventory);
    }
    
    public Vector3 GetInteractPosition()
    {
        return servingPoint.position;
    }
    
    public bool CanInteract(PlayerType playerType)
    {
        return true;
    }
    
    private void SetFreshness(IItem item)
    {
        if (item.FreshState == FreshState.Good)
            item.SpoilTimer = 0;
        else if (item.FreshState == FreshState.Normal)
            item.SpoilTimer = SPOIL_TIME * 1 / 3;
        else if (item.FreshState == FreshState.Bad)
            item.SpoilTimer = SPOIL_TIME * 2/3;
    }

    private bool CheckFreshness(IItem item)
    {
        if (item.FreshState == FreshState.Good)
        {
            if (item.SpoilTimer is >= 0 and <= SPOIL_TIME * 1 / 3)
                return false;
        }

        if (item.FreshState == FreshState.Normal)
        {
            if (item.SpoilTimer is > SPOIL_TIME * 1 / 3 and <= SPOIL_TIME * 2 / 3)
                return false;
        }
        
        else if (item.FreshState == FreshState.Bad)
        {
            if (item.SpoilTimer is > SPOIL_TIME * 2 / 3 and <= SPOIL_TIME)
                return false;
        }

        return true;
    }
}