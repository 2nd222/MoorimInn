using TMPro;
using UnityEngine;

public class DateUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _dateText;
    [SerializeField] private TextMeshProUGUI _dayOfWeekText;
    [SerializeField] private TextMeshProUGUI _timeText;

    private DayManager _dayManager;

    void Awake()
    {
        _dayManager = FindFirstObjectByType<DayManager>();
        _dayManager.OnDateChanged += DateUIUpdate;
        _dayManager.OnTimeChanged += TimeUIUpdate;
    }

    // 하루 시작할 때마다 날짜 UI 업데이트
    void DateUIUpdate()
    {
        if (_dayManager == null || _dayManager.DayData == null)
            return;
        
        string dayofweekText;
        
        // 한글 요일로 변환
        switch (_dayManager.DayData.dayOfWeek)
        {
            case DayOfWeek.Monday:
                dayofweekText = "월";
                break;
            case DayOfWeek.Tuesday:
                dayofweekText = "화";
                break;
            case DayOfWeek.Wednesday:
                dayofweekText = "수";
                break;
            case DayOfWeek.Thursday:
                dayofweekText = "목";
                break;
            case DayOfWeek.Friday:
                dayofweekText = "금";
                break;
            case DayOfWeek.Saturday:
                dayofweekText = "토";
                break;
            case DayOfWeek.Sunday:
                dayofweekText = "일";
                break;
            default:
                Debug.Log("DateUIController: DayData에서 DayofWeek 값이 없음");
                dayofweekText = "";
                break;
        }
        
        _dateText.text = "Day " + _dayManager.DayData.day.ToString();
        _dayOfWeekText.text = dayofweekText;
    }

    void TimeUIUpdate(int hour, int minute)
    {
        string timeText;
        string ampmText;
        
        ampmText = hour <= 12 ? "AM" : "PM";
        
        string minuteText = minute < 10 ? "0" + minute.ToString() : minute.ToString();
        string hourText = hour < 10 ? "0" + hour.ToString() : hour.ToString();
        timeText = $"{hourText}:{minuteText} {ampmText}";
        _timeText.text = timeText;
    }

    void OnDestroy()
    {
        _dayManager.OnDayStart -= DateUIUpdate;
    }
}
