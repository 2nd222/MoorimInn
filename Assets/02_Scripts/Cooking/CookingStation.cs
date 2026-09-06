using System.Collections;
using static Constants;
using UnityEngine;
using System;

public enum CookingState { Idle, Cooking, Done }

public class CookingStation : MonoBehaviour, IInteractable
{
    [Header("상호작용 위치")]
    [SerializeField] private Transform interactPoint;
 
    [Header("조리 설정")]
    [SerializeField] private float cookTime = 15f;
    [SerializeField] private float minimumCookTime = 3f;

    [SerializeField] private float cookSpeedCheckForDebug = 0;
    
    private Coroutine cookingRoutine;
    
    // ── 이벤트 ──
    /// <summary>
    /// 조리대 상태가 변경될 때 발행됩니다. (상태, 완성 음식 아이콘)
    /// Done 상태일 때만 Sprite가 전달되고, 나머지는 null입니다.
    /// CookingStationEffects 등 연출 컴포넌트가 구독합니다.
    /// </summary>
    public event Action<CookingState, Sprite> OnStateChanged;
 
    // ── 상태 ──
    public CookingState CurrentState { get; private set; } = CookingState.Idle;
    private Recipe currentRecipe;
 
    // ── IInteractable 구현 ──
 
    public InteractMode Mode => InteractMode.ClickToInteract;
 
    private void OnEnable()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayEnd += HandleDayEnd;
    }

    private void OnDisable()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayEnd -= HandleDayEnd;
    }
    
    public bool CanInteract(PlayerType playerType)
    {
        return playerType == PlayerType.Sowol;
    }
 
    public void Interact(PlayerController player)
    {
        switch (CurrentState)
        {
            case CookingState.Idle:
                if (CookingController.Instance != null)
                    CookingController.Instance.OpenCookingSystem(this);
                break;
 
            case CookingState.Cooking:
                Debug.Log("보글보글.. 현재 조리 중입니다!");
                break;
 
            case CookingState.Done:
                Inventory sowolInven = InventoryManager.Instance.GetInventory(InventoryType.Chef);
                CollectFood(sowolInven);
                break;
        }
    }
 
    public void OnInteractEnd(PlayerController player)
    {
        if (CookingController.Instance != null && CookingController.Instance.IsCookingUIOpen)
            CookingController.Instance.CloseCookingSystem(true);
    }
 
    public Vector3 GetInteractPosition()
    {
        return interactPoint != null ? interactPoint.position : transform.position;
        // return default;
    }
 
    // 조리 흐름
    public void StartCooking(Recipe recipe)
    {
        currentRecipe = recipe;
        SetState(CookingState.Cooking);
        cookingRoutine = StartCoroutine(CookingRoutine());
    }
 
    private IEnumerator CookingRoutine()
    {
        float bondBonus = BondManager.Instance.GetPercentModifier(BondEffectType.CookingSpeed);
        float upgradeBonus = UpgradeManager.Instance.GetPercentValue(UpgradeType.CookSpeed);
        
        float totalBonus = (bondBonus + upgradeBonus);
        cookSpeedCheckForDebug = totalBonus;
        
        float finalCookTime = Mathf.Max(minimumCookTime, cookTime * (1f - totalBonus));
        
        float elapsed = 0f;

        while (elapsed < finalCookTime)
        {
            if (!DayManager.Instance.IsPaused)
            {
                elapsed += Time.deltaTime;
            }

            yield return null;
        }
 
        // PlayerManager.Instance.StopCurrentPlayer();
        cookingRoutine = null;
        SetState(CookingState.Done);
    }
 
    private void CollectFood(Inventory targetInventory)
    {
        if (targetInventory == null)
            return;
 
        Item resultItem = new Item(currentRecipe.Data.result);
        int received = targetInventory.GetItem(resultItem, 1);
        int remain = 1 - received;

        if (remain > 0)
        {
            UIManager.Instance.CreateOkPopup("소월이 인벤토리가 꽉 찼습니다.", () => { }, ()=>{}, false);
            return;
        }
        
        currentRecipe = null;
        SetState(CookingState.Idle);
    }
 
    // 상태 변경
    private void SetState(CookingState newState)
    {
        if (CurrentState == newState)
            return;
 
        CurrentState = newState;
 
        // Done 상태일 때만 완성 음식 아이콘을 함께 전달
        Sprite icon = (newState == CookingState.Done && currentRecipe != null) ? currentRecipe.Data.icon : null;
 
        OnStateChanged?.Invoke(newState, icon);
    }

    private void HandleDayEnd()
    {
        if (cookingRoutine != null)
        {
            StopCoroutine(cookingRoutine);
            cookingRoutine = null;
        }

        currentRecipe = null;
        SetState(CookingState.Idle);
    }
}