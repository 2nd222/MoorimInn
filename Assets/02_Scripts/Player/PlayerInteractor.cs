using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactable = other.GetComponentInParent<IInteractable>();

        if (interactable == null)
            return;

        PlayerController owner = GetComponentInParent<PlayerController>();
        owner.ActiveInteractable = interactable;

        PlayerManager pm = PlayerManager.Instance;

        // 현재 선택된 캐릭터가 아니면 예약 실행하지 않음
        if (pm.SelectedUnit != owner) return;

        // 트리거 안에 있다는 사실 항상 기록
        // pm.ActiveInteractable = interactable;

        // 예약된 대상이 아니면 무시
        if (interactable != pm.ReservedInteractable)
            return;

        // 모드에 관계없이 ExecuteInteract가 이전 UI를 닫고 새로 연다
        pm.ExecuteInteract(interactable);
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponentInParent<IInteractable>();

        if (interactable == null)
            return;

        PlayerController owner = GetComponentInParent<PlayerController>();
        
        PlayerManager pm = PlayerManager.Instance;

        // AutoOnEnter 모드: 트리거 퇴장 시 자동으로 상호작용 종료
        if (interactable.Mode == InteractMode.AutoOnEnter)
        {
            pm.EndInteract(interactable, owner);
        }
        // ClickToInteract(조리대 등)도 멀어지면 닫는다.
        // 원래는 여기서 안 닫혀서 조리 UI가 남은 채로 배식대 UI가 같이 뜨는 문제가 있었음.
        else if (pm.CurrentInteracting == interactable)
        {
            pm.CloseCurrentInteract();
        }
 
        // ActiveInteractable 해제
        if (owner.ActiveInteractable == interactable)
            owner.ActiveInteractable = null;
    }
}