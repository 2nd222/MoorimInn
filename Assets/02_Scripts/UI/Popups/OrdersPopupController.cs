using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class OrdersPopupController : PopupController
{
    [SerializeField] private GameObject orderPopupPrefab;
    [SerializeField] private Transform contentTransform;
    [SerializeField] private GameObject orderNotificationObj;
    
    private Dictionary<GuestBase, OrderPopup> activeOrders = new Dictionary<GuestBase, OrderPopup>();

    void Start()
    {
        if (DayManager.Instance != null) 
            DayManager.Instance.OnDayEnd += ResetOrders;
    }
    
    void OnDestroy()
    {
        if (DayManager.Instance != null) 
            DayManager.Instance.OnDayEnd -= ResetOrders;
    }
    
    public new void Show()
    {
       base.Show();
       UpdateNotification(false);
    }

    public void OnClickConfirmButton()
    {
        Hide();
    }

    public void OnClickCancelButton()
    {
        UpdateNotification(false);
        Hide();
    }

    public void AddOrder(GuestBase guest, GuestOrderData order)
    {
        if (activeOrders.ContainsKey(guest))
            return;

        GameObject newPopupObj = Instantiate(orderPopupPrefab, contentTransform);

        // 먼저 들어온 주문이 맨 위에 오도록 맨 뒤에 배치
        newPopupObj.transform.SetAsLastSibling();

        OrderPopup popup = newPopupObj.GetComponent<OrderPopup>();
        popup.Init(order);

        activeOrders[guest] = popup;

        // 창이 켜져 있지 않으면 알림 켜기
        UpdateNotification(!gameObject.activeSelf);
    }

    // 손님 퇴장 또는 서빙 완료 시 호출
    public void RemoveOrder(GuestBase guest)
    {
        if (activeOrders.TryGetValue(guest, out OrderPopup popup))
        {
            Destroy(popup.gameObject);
            activeOrders.Remove(guest);
        }
    }
    void ResetOrders()
    {
        foreach (OrderPopup popup in activeOrders.Values)
        {
            if (popup != null)
            {
                Destroy(popup.gameObject);
            }
        }
        activeOrders.Clear();

        UpdateNotification(false);
    }
    void UpdateNotification(bool isActive)
    {
        if (orderNotificationObj == null) 
            return;
        
        orderNotificationObj.SetActive(isActive);
    }
}


