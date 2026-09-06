using UnityEngine;
using UnityEngine.Events;
using static Constants;

public interface IControllable
{
    bool IsMoving { get; } // 네브메쉬에이전트 기반 이동 중 여부
    PlayerType PlayerType { get; } // 이 유닛 타입

    void OnSelected();   // 캐릭터 상태 진입 (스왑)
    void OnDeselected(); // 캐릭터 상태 퇴장? 
    void MoveTo(Vector3 destination, UnityAction onArrived = null);  // 네브메쉬 이동, 여부
    void Stop();
    void SetSpeed(float move, float angular, float accel); // 움직임
}
