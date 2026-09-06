using UnityEngine;

[CreateAssetMenu(fileName = "DayData", menuName = "Day/DayData")]
public class DayData : ScriptableObject
{
    public int day;
    public DayOfWeek dayOfWeek;
}
