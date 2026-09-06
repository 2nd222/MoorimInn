using UnityEngine;
using static Constants;

public class RichManGuest : GuestBase
{
    [Header("부자 패널티")]
    public int downFame = 5;

    public override void Init(int id, GuestAppearance appearance, Transform exit, GuestManager manager)
    {
        SetBaseWaitTime(10f, 15f, 30f, 45f);

        base.Init(id, appearance, exit, manager);

        OnGuestPaid += GiveRichBonus;
        OnGuestAngryExited += ApplyRichPenalty;
    }

    private void GiveRichBonus()
    {
        if (myOrder == null || myOrder.orderedFood == null) return;

        int basePrice = myOrder.orderedFood.price;
        int bonusMoney = 0;

        // 부모(GuestBase)에서 기본 음식값(1배)을 이미 지급했으므로, 차액(보너스)만 계산해서 더해줌.
        switch (CurrentMood)
        {
            case GuestMood.Neutral:
                bonusMoney = basePrice * 1; // 총 2배 (기본 1 + 보너스 1)
                break;
            case GuestMood.Good:
                bonusMoney = basePrice * 2; // 총 3배 (기본 1 + 보너스 2)
                break;
            case GuestMood.VeryGood:
                bonusMoney = basePrice * 3; // 총 4배 (기본 1 + 보너스 3)
                break;
            case GuestMood.Bad:
            case GuestMood.VeryBad:
            default:
                bonusMoney = 0; // 배율 1배이므로 추가 보너스 없음
                break;
        }

        Debug.Log($"<color=blue> 손님의 기분 : {CurrentMood}.</color>");        

        // 보너스 금액이 있다면 경제 매니저에 추가
        if (bonusMoney > 0)
        {
            EconomyManager.Instance.AddMoney(bonusMoney);
            Debug.Log($"<color=yellow>원래 가격 {basePrice}, 추가로 {bonusMoney}원</color>");        
        }
    }

    private void ApplyRichPenalty(GuestBase guest)
    {
        // Very Bad 상태로 나갈 때 명성치 추가 삭감
        if (downFame > 0)
        {
            EconomyManager.Instance.AddFame(-downFame);
            Debug.Log($"<color=red>Very Bad , 추가 명성 하락: -{downFame}</color>");
        }
    }
}