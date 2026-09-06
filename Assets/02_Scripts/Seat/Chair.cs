using UnityEngine;

public class Chair : MonoBehaviour
{
    [SerializeField] public Transform sitPoint; // 손님이 도착해서 앉을 위치/방향

    public Table table;
    public Transform myFoodSpot;

    public bool isBuilt = true;
    private GuestBase currentGuest;

    // 핵심 : 빈자리인지 판단하는 실시간 프로퍼티
    public bool IsEmpty
    {
        get
        {
            // 의자가 설치되어있고, 테이블이 존재하며 설치되어있고, 현재 앉아있거나 찜한 손님이 없다면 => 빈자리 반환
            return isBuilt && table != null && table.isBuilt && currentGuest == null;
        }
    }

    void Start()
    {
        // 게임 시작 시 매니저에게 자기 자신을 등록
        if (SeatManager.Instance != null)
            SeatManager.Instance.RegisterChair(this);
    }

    // 손님이 이 자리를 찜할 때 호츨
    public void Reserve(GuestBase guest)
    {
        currentGuest = guest;
        SeatManager.Instance.ReserveChair(this); // 빈자리 리스트에서 제거
    }

    // 손님이 이 자리를 떠날 때 호출
    public void Empty()
    {
        currentGuest = null;
        SeatManager.Instance.FreeChair(this); // 빈자리 리스트에 추가
    }
}
