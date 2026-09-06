using System;
using UnityEngine;
using static Constants;

[RequireComponent(typeof(GuestBase))]
public class GuestInteractable : MonoBehaviour, IInteractable
{
    private GuestBase myBrain;
    public GuestBase MyBrain => myBrain;

    private bool bottomUIOpenedByMe = false;
 
    void Awake()
    {
        myBrain = GetComponent<GuestBase>();
    }
 
    // IInteractable 구현 
 
    // 손님은 클릭해서 상호작용 (ClickToInteract)
    public InteractMode Mode => InteractMode.AutoOnEnter;
 
    // 서빙 캐릭터만 상호작용 가능
    public bool CanInteract(PlayerType playerType)
    {
        //분노 퇴장 중이면 상호작용 불가
        if (myBrain == null || myBrain.IsAngryExiting) return false;
        
        return playerType == PlayerType.Myungwol;

    }
 
    // 명월이가 손님을 클릭했을 때 맵의 어디로 걸어와야 하는가?
    public Vector3 GetInteractPosition()
    {
        return transform.position;
    }
 
    // 명월이가 손님 근처에 도착해서 상호작용을 완료했을 때 벌어질 일
    public void Interact(PlayerController player)
    {
        if (myBrain == null || myBrain.IsAngryExiting) return;

        // 난동 중인 괴한은 제압 (상태와 무관하게 최우선)
        if (myBrain is ViolenceGuest violence && violence.isCausingTrouble)
        {
            violence.SuppressViolence();
            return;
        }

        if (myBrain.currentState == GuestState.WaitOrder)
            myBrain.TakeOrder();
        else if (myBrain.currentState == GuestState.WaitFood)
        {
            InventoryManager.Instance.RequestBottomUI(true);
            bottomUIOpenedByMe = true;
        }
        else if (myBrain.currentState == GuestState.Exit)
        {
            ThiefGuyGuest thief = myBrain as ThiefGuyGuest;

            if (thief != null)
                thief.CatchThief();
        }
    }
 
    public void OnInteractEnd(PlayerController player)
    {
        if (player.PlayerType == PlayerType.Myungwol)
        {
            InventoryManager.Instance.RequestBottomUI(false);
            bottomUIOpenedByMe = false;
        }
    }
    
    public void DeliverFood(Inventory serverInven, Slot slot)
    {
        if (myBrain == null || myBrain.IsAngryExiting)
        {
            InventoryManager.Instance.CloseInventory(serverInven);
            return;
        }
    
        if (myBrain.currentState != GuestState.WaitFood)
            return;
    
        FoodData food = slot.Item.Data as FoodData;
        if (food == null) return;

        serverInven.RemoveItem(slot.Item.ID, 1);
        
        FoodQuality quality = FoodQuality.Normal;
        var mastery = DataManager.Instance.GetMasteryByFood(food);
        if (mastery != null)
            quality = DataManager.Instance.GetQualityByProficiency(mastery.proficiency);

        myBrain.ReceiveFood(food, quality);

        InventoryManager.Instance.CloseInventory(serverInven);
    }
}
