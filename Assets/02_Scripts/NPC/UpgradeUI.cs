using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeUI : MonoBehaviour
{
    [SerializeField] private UpgradeSlotUI slotPrefab;
    [SerializeField] private Transform content;

    [SerializeField] private UpgradeDetailPanel detailPanel;

    private UpgradeSlotUI currentSelectedSlot;
    private UpgradeType? currentSelectedType;
    
    private void OnEnable()
    {
        Refresh();
        UpgradeManager.Instance.SelectedOptionUpgraded += Refresh;
    }
    
    private void OnDisable()
    {
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.SelectedOptionUpgraded -= Refresh;
    }

    public void Refresh(UpgradeType upgradeType = UpgradeType.Null)
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);

        currentSelectedSlot = null;
        
        foreach (var data in UpgradeManager.Instance.GetUpgradeList())
        {
            var slot = Instantiate(slotPrefab, content);

            slot.Init(data, OnSelectUpgrade);
            
            if (currentSelectedType.HasValue && data.type == currentSelectedType.Value)
            {
                currentSelectedSlot = slot;
                slot.SetSelected(true);

                detailPanel.Show(data);
            }
        }
    }

    private void OnSelectUpgrade(UpgradeData data, UpgradeSlotUI slot)
    {
        // 이전 선택 해제
        if (currentSelectedSlot != null)
            currentSelectedSlot.SetSelected(false);

        // 현재 선택
        currentSelectedSlot = slot;
        currentSelectedType = data.type;
        
        currentSelectedSlot.SetSelected(true);
        
        detailPanel.Show(data);
    }
}
