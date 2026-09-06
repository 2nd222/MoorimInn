using UnityEngine;

/// <summary>
/// 주간 운영 시간에 별도의 이벤트가 있을 때를 대비해 만들어 놓은 클래스
/// </summary>
public class EventManager : MonoBehaviour
{
    // 이벤트 관련 매니저
    [SerializeField] DayManager dayManager;
    
    void Awake()
    {
        dayManager.OnDayStart += GenerateEvent;
    }

    private void GenerateEvent()
    {
        
    }
}
