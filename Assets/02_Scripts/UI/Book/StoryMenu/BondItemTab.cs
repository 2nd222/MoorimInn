using UnityEngine;

public class BondItemTab : BaseTab
{
    [SerializeField] private Inventory bondInventory;
    [SerializeField] private BondItemDetailPanel detailPanel;
    
    private Slot currentSelectedSlot;

    private Color normalColor = Color.white;
    private readonly Color selectedColor = new Color(0.44f, 0.44f, 0.44f);
    
    public override void SetupData()
    {
        bondInventory = InventoryManager.Instance.GetInventory(Constants.InventoryType.BondItem);

        bondInventory.RefreshItemOrder();
        
        foreach (var slot in bondInventory.Slots)
        {
            slot.OnSlotClick += OnClickSlot;
        }
    }

    public override void ResetSetting()
    {
        bondInventory.RefreshItemOrder();
        
        detailPanel.gameObject.SetActive(false);
        
        if (currentSelectedSlot != null)
        {
            currentSelectedSlot.SlotIcon.color = normalColor;
            currentSelectedSlot = null;
        }
    }
    
    private void OnClickSlot(Slot slot)
    {
        // 이전 선택 해제
        if (currentSelectedSlot != null)
            currentSelectedSlot.SlotIcon.color = normalColor;

        // 현재 선택
        currentSelectedSlot = slot;
        currentSelectedSlot.SlotIcon.color = selectedColor;
     
        detailPanel.gameObject.SetActive(!slot.IsEmpty);

        if (!slot.IsEmpty)
            detailPanel.SetItem(slot.Item);
    }
}
