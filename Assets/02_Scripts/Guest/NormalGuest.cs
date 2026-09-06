using UnityEngine;

public class NormalGuest : GuestBase
{
    protected override void HandleWaitOrderState()
    {
        base.HandleWaitOrderState();
    }

    protected override void HandleWaitFoodState()
    {
        base.HandleWaitFoodState();
    }
    
    protected override void HandleEatState()
    {
        base.HandleEatState();
    }

    protected override void HandlePayState()
    {
        base.HandlePayState();
    }
}
