using UnityEngine;

public class BadGuest : GuestBase
{
    [Header("몇 초 뒤에 주문 바꿈")]
    [SerializeField] private float changeOrderTime = 10f; 
    
    private bool hasChangedOrder = false;

    public override void ResetState()
    {
        base.ResetState();
        hasChangedOrder = false; // 풀링에서 재사용될 때 플래그 초기화
    }

    protected override void HandleWaitFoodState()
    {
        // 부모의 기존 로직(음식이 너무 안 나오면 화내는 로직)은 그대로 실행
        base.HandleWaitFoodState();

        if (IsAngryExiting) return;

        // 메뉴를 아직 안 바꿨고, 음식을 기다린 지(stateTimer) changeOrderTime 이상 지났다면
        if (!hasChangedOrder && stateTimer >= changeOrderTime)
        {
            ChangeToRandomMenu();
        }
    }

    private void ChangeToRandomMenu()
    {
        hasChangedOrder = true;

        // 1. 기존 메뉴 저장 (동일한 메뉴로 바뀌는 것을 방지하기 위함)
        RecipeData oldRecipe = myOrder.recipe;
        RecipeData newRecipe = oldRecipe;

        // 2. 다른 메뉴가 나올 때까지 뽑기 (무한루프 방지를 위해 최대 10번만 시도)
        int safetyCount = 0;
        while (newRecipe == oldRecipe && safetyCount < 10)
        {
            var candidate = recipeUnlockManager.RandomUnlockedRecipe();
            
            if (candidate != null)
                newRecipe = candidate;
            
            safetyCount++;
        }
        
        if (newRecipe == null || newRecipe == oldRecipe)
            return;

        // 3. 주문서 데이터 업데이트
        myOrder.recipe = newRecipe;
        myOrder.orderedFood = newRecipe.result;
        myOrder.isServed = false; // 서빙 안 된 상태로 다시 확인

        Debug.Log($"<color=red>진상 고객 메뉴 변경 : {oldRecipe.name} -> {newRecipe.name}</color>");

        // 4. UI 및 외부 매니저들에게 주문이 바뀌었음을 알림
        InvokeOrderChanged();
    }
}