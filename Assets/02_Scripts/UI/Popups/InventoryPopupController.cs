using System;
using System.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class InventoryPopupController : PopupController
{
    [SerializeField] private bool isCommonInventory;

    public bool IsCommonInventory()
    {
        return isCommonInventory;
    }
    
    // 공용 인벤용 기능 추가
    // 공용 인벤 타입 버튼 눌렀을 때 그 타입 아이템만 나오도록
}