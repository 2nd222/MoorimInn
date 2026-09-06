using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Loading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Constants;

/// <summary>
/// 인벤토리 클래스, 각 인벤토리 UI에서 슬롯들을 묶는 group역할을 하는 오브젝트에 컴포넌트로 들어간다.
/// 인벤토리 안의 슬롯에 담긴 아이템을 관리하는 클래스.
/// </summary>
public class Inventory : MonoBehaviour, IInitializable
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private int slotCount = 10;
    [SerializeField] private List<Slot> slots  = new List<Slot>();
    public List<Slot> Slots => slots;
    
    [SerializeField] private InventoryType type;
    [SerializeField] private Button closeButton;
    
    public InventoryType Type => type;
    
    public Action<Slot> OnItemAdded;

    public UnityAction OnCloseRequested;

    private bool isInit = false;
    public bool IsReadOnly => type == InventoryType.BondItem;
    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() =>
            {
                if (OnCloseRequested != null)
                    OnCloseRequested.Invoke();
                else
                    UIManager.Instance.CloseInventoryPopup(this);
            });
        }
    }
    
    public void Init()
    {
        if (isInit) return;

        CreateSlots(slotCount);

        isInit = true;
    }
    
    
    private void CreateSlots(int num)
    {
        for (int i = 0; i < num; i++)
        {
            Slot slot = Instantiate(slotPrefab, transform).GetComponent<Slot>();
            slot.SetOwner(this);
            //slot.SetIndex(i); 
            slot.SetIndex(slots.Count);
            slot.OnSlotDrop += HandleSlotDrop;
            slot.OnSlotClick += HandleSlotClick;
            slots.Add(slot);
        }
    }
    
    
    /// <summary>
    /// 업그레이드 적용함수
    /// </summary>
    /// <param name="targetCount"></param>
    public void ApplyUpgradeSlotCount(int targetCount)
    {
        if (targetCount <= slots.Count)
            return;

        AddSlot(targetCount - slots.Count);
    }
    
    /// <summary>
    /// 업그레이드 등으로 슬롯의 개수가 늘어날 때 사용하는 슬롯 추가 함수
    /// </summary>
    /// <param name="amount"></param>
    public void AddSlot(int amount)
    {
        CreateSlots(amount);
        slotCount += amount;
    }
    
    /// <summary>
    /// 슬롯에서 아이템 추가, 이동 등의 변경이 일어났을 때 냉장고의 아이템들을 기준으로 퀘스트 완료 가능 여부를 업데이트하기 위한 함수 
    /// </summary>
    /// <returns></returns>
    private void NotifyItemChanged(int itemID)
    {
        if (type != InventoryType.Fridge)
            return; 

        QuestManager.Instance.NotifyListener(new QuestEvent
        {
            type = QuestEventType.ItemChanged,
            ID = itemID,
            currentCount = GetItemCount(itemID)
        });
    }
    
    /// <summary>
    /// 슬롯간의 아이템 이동을 처리하는 함수, 슬롯과의 소통을 담당하는 함수
    /// </summary>
    /// <param name="target"></param>
    /// <param name="source"></param>
    private void HandleSlotDrop(Slot target, Slot source)
    {
        if (!target.Owner.CanAcceptFoodItem(source.Item))
        {
            Debug.Log("이 인벤토리에는 넣을 수 없음");
            return;
        }
        
        // 같은 인벤토리면 내가 처리
        if (target.Owner == this && source.Owner == this)
        {
            MoveItem(target, source, source.Count);
        }
        // 다른 인벤토리면 target 쪽이 본인의 컴포넌트에서 처리
        else if (target.Owner == this)
        {
            MoveItem(target, source, source.Count);
        }
    }

    /// <summary>
    /// 인벤토리 슬롯 클릭시 호출
    /// </summary>
    /// <param name="slot"></param>
    private void HandleSlotClick(Slot slot)
    {
        PlayerController currentPlayer = PlayerManager.Instance.SelectedUnit;
        currentPlayer?.OnSlotClicked(slot);
    }

    /// <summary>
    /// 대상 아이템이랑 소스 아이템이랑 합칠 수 있는지
    /// </summary>
    /// <param name="target"></param>
    /// <param name="targetSlot"></param>
    /// <param name="source"></param>
    /// <param name="sourceSlot"></param>
    /// <returns></returns>
    private bool CanMergeItems(IItem target, IItem source)
    {
        if (target == null || source == null)
            return false;

        if (target.ID != source.ID)
            return false;

        if (target.FreshState != source.FreshState)
        {
            Debug.Log("신선도 다름");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 타겟에 소스의 아이템을 합칠 수 있는지
    /// </summary>
    /// <param name="target"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    private bool CanMerge(Slot target, Slot source)
    {
        if (target.IsEmpty || source.IsEmpty)
            return false;

        return CanMergeItems(target.Item, source.Item);
    }
    
    /// <summary>
    /// 슬롯끼리 아이템 합치는 함수
    /// </summary>
    /// <param name="target"></param>
    /// <param name="source"></param>
    private int MergeSlot(Slot target, Slot source, int amount)
    {
        int max = target.Item.InventoryCapacity;
        int canAdd = max - target.Count;

        if (canAdd <= 0)
            return 0;

        int move = Mathf.Min(canAdd, amount);

        target.AddCount(move);
        source.AddCount(-move);

        if (source.Count <= 0)
            source.Clear();

        return move;
    }
    
    /// <summary>
    /// 빈 슬롯에 아이템 넣는 함수
    /// </summary>
    /// <param name="target"></param>
    /// <param name="source"></param>
    private int MoveToEmptySlot(Slot target, Slot source, int amount)
    {
        int move = Mathf.Min(amount, source.Count);

        if (move == source.Count)
        {
            // 전부 이동
            target.SetItem(source.Item, move);
            source.Clear();
        }
        else
        {
            // 일부 이동
            target.SetItem(new Item(source.Item), move);
            source.AddCount(-move);
        }

        return move;
    }
    
    /// <summary>
    /// 슬롯끼리 스왑
    /// </summary>
    /// <param name="target"></param>
    /// <param name="source"></param>
    private void SwapSlot(Slot target, Slot source)
    {
        // dragslot은 원래 슬롯을 참조하는 것이기 때문에 원래 슬롯이 변경됨
        //“dragSlot은 A 슬롯 객체를 참조하고 있고, dragSlot을 통해 호출한 메서드는 A 슬롯 인스턴스의 상태를 직접 변경한다.”
        IItem tempItem = target.Item;
        int tempCount = target.Count;

        target.SetItem(source.Item, source.Count);

        source.SetItem(tempItem, tempCount);
    }
    
    /// <summary>
    /// 소스 슬롯에서 타겟 슬롯에 아이템을 이동시키는 함수
    /// </summary>
    /// <param name="target"></param>
    /// <param name="source"></param>
    private void MoveItem(Slot target, Slot source, int amount)
    {
        if (source.IsEmpty)
            return;

        amount = Mathf.Clamp(amount, 1, source.Count);
        
        int itemID = source.Item.ID;

        // 컨트롤 누른 채 드래그하면 절반으로 줄이기
        if (target.IsEmpty && Input.GetKey(KeyCode.LeftControl))
        {
            amount = source.Count / 2;

            if (amount <= 0)
                return;
        }
        
        // 1. 빈 슬롯
        if (target.IsEmpty)
        {
            MoveToEmptySlot(target, source, amount);
        }
        // 2. 같은 아이템 → 합치기
        else if (CanMerge(target, source))
        {
            MergeSlot(target, source, amount);
        }
        // 3. 스왑
        else
        {
            // 부분 이동이면 스왑 불가
            if (amount != source.Count)
                return;
            
            SwapSlot(target, source);
        }

        if (target.Owner.Type == InventoryType.MealTable)
            OnItemAdded?.Invoke(target);
        
        NotifyItemChanged(itemID);
        if (!target.IsEmpty)
            NotifyItemChanged(target.Item.ID);
    }

    /// <summary>
    /// 아이템 생성(GetItem) 전에 수용 가능한지 확인하는 함수
    /// </summary>
    /// <param name="item"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public int CanReceiveItem(IItem item, int count)
    {
        int remain = count;

        // 기존 스택
        foreach (var slot in slots)
        {
            if (remain <= 0)
                break;

            if (slot.IsEmpty)
                continue;

            if (slot.Item.ID != item.ID)
                continue;

            if (slot.Item.FreshState != item.FreshState)
                continue;

            int canAdd = item.InventoryCapacity - slot.Count;

            if (canAdd <= 0)
                continue;

            remain -= canAdd;
        }

        // 빈 슬롯
        foreach (var slot in slots)
        {
            if (remain <= 0)
                break;

            if (!slot.IsEmpty)
                continue;

            remain -= item.InventoryCapacity;
        }

        return count - remain;
    }
    
    /// <summary>
    /// 아이템을 획득 했을 때 호출되는 함수
    /// </summary>
    /// <param name="item"></param>
    /// <param name="count"></param>
    public int GetItem(IItem item, int count)
    {
        int original = count;
        
        int itemID = item.ID;
        bool changed = false;
        
        // 1. 기존 슬롯 채우기
        foreach (var slot in slots)
        {
            if (count <= 0)
                break;

            if (slot.IsEmpty)
                continue;
            
            if (!CanMergeItems(slot.Item, item))
                continue;

            int max = item.InventoryCapacity;
            int canAdd = max - slot.Count;

            if (canAdd <= 0)
                continue;
            
            Debug.Log($"{slot.name}에 {itemID} 생성");

            int add = Mathf.Min(canAdd, count);
            slot.AddCount(add);
            count -= add;
            changed = true;
            
            OnItemAdded?.Invoke(slot);
        }

        // 2. 빈 슬롯에 넣기
        foreach (var slot in slots)
        {
            if (count <= 0)
                break;

            if (!slot.IsEmpty)
                continue;
            
            Debug.Log($"{slot.name}에 {itemID} 생성");

            int add = Mathf.Min(item.InventoryCapacity, count);
            slot.AddItem(new Item(item), add);
            count -= add;   
            changed = true;
            
            OnItemAdded?.Invoke(slot);
        }

        if (changed)
        {
            NotifyItemChanged(itemID);
        }

        return original - count;
    }

    /// <summary>
    /// 아이템을 인벤토리에서 지울 때 호출하는 함수
    /// </summary>
    /// <param name="itemID"></param>
    /// <param name="removeCount"></param>
    public void RemoveItem(int itemID, int removeCount)
    {
        bool changed = false;
        
        foreach (var slot in slots)
        {
            if (removeCount <= 0)
                break;
            
            if (slot.IsEmpty)
                continue;
            
            if (slot.Item.ID != itemID)
                continue;
            
            changed = true;
            
            int removable = Mathf.Min(slot.Count, removeCount);
            slot.AddCount(-removable);
            removeCount -= removable;

            if (slot.Count <= 0)
                slot.Clear();
        }
        
        if (changed)
            NotifyItemChanged(itemID);
    }
    
    /// <summary>
    /// 본인 인벤토리 내의 아이템의 전체 개수를 구하는 함수
    /// </summary>
    /// <param name="itemID"></param>
    /// <returns></returns>
    public int GetItemCount(int itemID)
    {
        int count = 0;

        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
                continue;

            if (slot.Item.ID != itemID)
                continue;

            count += slot.Count;
        }

        return count;
    }
    
    /// <summary>
    /// 음식 아이템을 넣을 수 있는 곳인지 확인하는 함수(냉장고에는 재료 아이템을 넣을 수 없다.)
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool CanAcceptFoodItem(IItem item)
    {
        if (type is InventoryType.Fridge or InventoryType.MealTable or InventoryType.Chef or InventoryType.Server)
            return item.Data is FoodData;

        if (type == InventoryType.IngredientsButMeatOnly)
        {
            if (item.Data is IngredientData ingredientData)
            {
                return ingredientData.type is Constants.IngredientType.Meats;
            }
            else
                return false;
        }
        
        if(type == InventoryType.Ingredients)
        {
            if (item.Data is IngredientData ingredientData)
            {
                return ingredientData.type is not Constants.IngredientType.Meats;
            }
            else
                return false;
        }

        return true;
    }
    
    /// <summary>
    /// 클릭으로 아이템 이동할 때 쓰는 함수
    /// </summary>
    /// <param name="targetInventory"></param>
    /// <param name="sourceSlot"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public Slot TryMoveToInventory(Inventory targetInventory, Slot sourceSlot, int amount)
    {
        if (sourceSlot.IsEmpty)
            return null;
        
        amount = Mathf.Clamp(amount, 1, sourceSlot.Count);
        int itemID = sourceSlot.Item.ID;
        Slot firstTarget = null;
        int remain = amount;
        
        foreach (var targetSlot in targetInventory.Slots)
        {
            if (!CanMerge(targetSlot, sourceSlot))
                continue;

            int moved = MergeSlot(targetSlot, sourceSlot, amount);

            if(moved > 0)
                firstTarget = targetSlot;
            
            remain -= moved;

            if (remain <= 0)
                break;
        }

        if(remain > 0)
        {
            foreach (var targetSlot in targetInventory.Slots)
            {
                if (!targetSlot.IsEmpty)
                    continue;

                int moved = MoveToEmptySlot(targetSlot, sourceSlot, remain);

                if (moved > 0 && firstTarget == null)
                    firstTarget = targetSlot;

                remain -= moved;

                if (remain <= 0)
                    break;
            }
        }

        if (targetInventory.Type == InventoryType.MealTable && firstTarget != null)
            targetInventory.OnItemAdded?.Invoke(firstTarget);
        
        NotifyItemChanged(itemID);
        targetInventory.NotifyItemChanged(itemID);
        
        return firstTarget;
    }
    
    public Slot TryMoveOneToInventory(Inventory targetInventory, Slot sourceSlot)
    {
        if (sourceSlot.IsEmpty)
            return null;
        
        return TryMoveToInventory(targetInventory, sourceSlot, 1);
    }
    
    public void RefreshItemOrder()
    {
        var items = slots.Where(x => !x.IsEmpty).OrderBy(x => x.Item.ID)
            .Select(x => new
            {
                x.Item,
                x.Count
            }).ToList();

        foreach (var slot in slots)
            slot.Clear();

        for (int i = 0; i < items.Count; i++)
        {
            slots[i].SetItem(items[i].Item, items[i].Count);
        }
    }

    public void RefreshItemOrder(bool mergeStack = true)
    {
        var list = slots
            .Where(x => !x.IsEmpty)
            .Select(x => (item: x.Item, count: x.Count))
            .OrderBy(x => x.item.ID)
            .ToList();

        // ===== 스택 합치기 =====
        if (mergeStack)
        {
            var merged = new List<(IItem item, int count)>();

            foreach (var data in list)
            {
                int remain = data.count;

                for (int i = 0; i < merged.Count; i++)
                {
                    var m = merged[i];

                    if (!CanMergeItems(m.item, data.item))
                        continue;

                    if (m.count >= m.item.InventoryCapacity)
                        continue;

                    int canAdd = m.item.InventoryCapacity - m.count;
                    int add = Mathf.Min(canAdd, remain);

                    merged[i] = (m.item, m.count + add);

                    remain -= add;

                    if (remain <= 0)
                        break;
                }

                while (remain > 0)
                {
                    int add = Mathf.Min(data.item.InventoryCapacity, remain);
                    merged.Add((new Item(data.item), add));
                    remain -= add;
                }
            }

            list = merged
                .Select(x => (item: x.item, count: x.count))
                .ToList();
        }

        foreach (var slot in slots)
            slot.Clear();

        for (int i = 0; i < list.Count && i < slots.Count; i++)
        {
            slots[i].SetItem(new Item(list[i].item), list[i].count);
        }
    }

    public InventorySaveData GetSaveData()
    {
        var data = new InventorySaveData();
        data.type = this.type;

        foreach (var slot in slots)
        {
            data.slots.Add(new SlotSaveData
            {
                itemID = slot.IsEmpty ? -1 : slot.Item.ID,
                count = slot.Count,
                spoilTimer = slot.IsEmpty ? 0 : slot.Item.SpoilTimer,
                fridgeSpoilTimer = slot.IsEmpty ? 0 : slot.Item.FridgeSpoilTimer,
                freshState = slot.IsEmpty ? FreshState.None : slot.Item.FreshState
            });
        }

        return data;
    }
    
    public void LoadFromData(InventorySaveData data)
    {
        int count = Mathf.Min(slots.Count, data.slots.Count);
        
        for (int i = 0; i < count; i++)
        {
            var slotData = data.slots[i];

            if (slotData.itemID == -1)
            {
                slots[i].Clear();
                continue;
            }

            var itemData = DataManager.Instance.GetItemData(slotData.itemID);
            IItem item = new Item(itemData);

            slots[i].SetItem(item, slotData.count);
            slots[i].Item.SpoilTimer = slotData.spoilTimer;
            slots[i].Item.FridgeSpoilTimer = slotData.fridgeSpoilTimer;
            slots[i].Item.FreshState = slotData.freshState;
        }
    }
    
    public void ResetData()
    {
        if (slots == null || slots.Count == 0)
            return;

        foreach (var slot in slots)
        {
            if (slot == null)
                continue;

            slot.Clear();
        }
    }
}
