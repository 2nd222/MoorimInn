using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class OrderPopup : MonoBehaviour
{
    [SerializeField] private Image foodIcon;
    [SerializeField] private TMP_Text foodNameText;
    
    public void Init(GuestOrderData order)
    {
        if (order.orderedFood != null)
        {
            foodIcon.sprite = order.orderedFood.icon;
            foodNameText.text = order.orderedFood.itemName;
        }
        
    }
}