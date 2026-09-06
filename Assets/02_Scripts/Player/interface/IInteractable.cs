using UnityEngine;
using static Constants;

public enum InteractMode
{
    ClickToInteract, // 자동 실행 이후 직접 실행
    AutoOnEnter, // 자동 실행
    Immediate // 즉시 실행
}

public interface IInteractable
{
    // 이 오브젝트의 상호작용 방식
    InteractMode Mode { get; }

    // 해당 플레이어 타입이 상호작용 가능한지 여부
    bool CanInteract(PlayerType playerType);

    // 상호작용 실행 - 플레이어 컨트롤러 참조를 받아 인벤토리 접근 등 필요한 처리를 할 수 있음
    void Interact(PlayerController Player);

    // 상호작용 종료 - AutoOnEnter 모드에서 드리거 퇴장 시 호출
    void OnInteractEnd(PlayerController player);

    // 플레이어가 이동해야 할 상호작용 위치
    Vector3 GetInteractPosition();
}
