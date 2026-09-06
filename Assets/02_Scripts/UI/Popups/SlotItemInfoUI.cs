using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Constants;

public class SlotItemInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI freshnessText;
    [SerializeField] private TextMeshProUGUI remainText;

    private IItem currentItem;
    private InventoryType currentInventoryType;
    
    public static SlotItemInfoUI Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    
    public void Show(IItem item, InventoryType inventoryType, RectTransform target)
    {
        currentItem = item;

        itemName.text = item.ItemName;
        currentInventoryType = inventoryType;
        
        Refresh();
        SetPosition(target);

        CancelInvoke(nameof(Refresh));
        if (inventoryType == InventoryType.MealTable)
        { 
            InvokeRepeating(nameof(Refresh), 0.2f, 0.2f);
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        CancelInvoke(nameof(Refresh));

        currentItem = null;

        itemName.text = "";
        freshnessText.text = "";
        remainText.text = "";

        gameObject.SetActive(false);
    }

    private void Refresh()
    {
        if (currentItem == null)
            return;

        switch (currentInventoryType)
        {
            case InventoryType.Fridge:
                SetFridgeInfo(currentItem);
                break;

            case InventoryType.MealTable:
                SetMealTableInfo(currentItem);
                break;

            case InventoryType.Chef:
                SetPlayerInfo(currentItem);
                break;
            
            case InventoryType.Server:
                SetPlayerInfo(currentItem);
                break;
            
            default:
                freshnessText.gameObject.SetActive(false);
                remainText.gameObject.SetActive(false);
                break;
        }
    }
    
    private void SetPosition(RectTransform target)
    {
        RectTransform rect = (RectTransform)transform;

        // 레이아웃이 갱신되도록
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

        Vector3[] slotCorners = new Vector3[4];
        target.GetWorldCorners(slotCorners);

        float offsetX = 100f;
        float offsetY = 50f;

        // 기본 위치 : 오른쪽 아래
        Vector3 pos = slotCorners[3] + new Vector3(offsetX, -offsetY, 0);

        // 툴팁 크기
        float width = rect.rect.width;
        float height = rect.rect.height;

        // 오른쪽 화면 밖이면 왼쪽으로
        if (pos.x + width > Screen.width)
        {
            pos.x = slotCorners[0].x - width - offsetX;
        }

        // 아래 화면 밖이면 위로
        if (pos.y - height < 0)
        {
            pos.y = slotCorners[2].y + offsetY;
        }

        rect.position = pos;
    }
    
    private void SetFridgeInfo(IItem item)
    {
        freshnessText.gameObject.SetActive(true);
        remainText.gameObject.SetActive(true);
            
        int remainDay = Mathf.Max(0, MAX_FRIDGE_DAY - item.FridgeSpoilTimer);
        freshnessText.text = $"신선도 : {GetFreshnessName(item)}";
        remainText.text = $"남은 보관일 : {remainDay}일";
    }

    private void SetMealTableInfo(IItem item)
    {
        freshnessText.gameObject.SetActive(true);
        remainText.gameObject.SetActive(true);
        
        float remain = Mathf.Max(0, SPOIL_TIME - item.SpoilTimer);
        freshnessText.text = $"신선도 : {GetFreshnessName(item)}";
        
        int remainSecond = Mathf.CeilToInt(remain);
        remainText.text = $"남은 시간 : {remainSecond}초";
    }

    private void SetPlayerInfo(IItem item)
    {
        freshnessText.gameObject.SetActive(true);
        remainText.gameObject.SetActive(false);
        
        freshnessText.text = $"신선도 : {GetFreshnessName(item)}";
    }
    
    private string GetFreshnessName(IItem item)
    {
        FreshState state = item.FreshState;
        string result = "";
        
        switch(state)
        {
            case FreshState.Good:
                result = "<color=#55CC55>신선</color>";
                break;

            case FreshState.Normal:
                result = "<color=#FFD34A>보통</color>";
                break;

            case FreshState.Bad:
                result = "<color=#FF5555>나쁨</color>";
                break;
        }
        return result;
    }
}
