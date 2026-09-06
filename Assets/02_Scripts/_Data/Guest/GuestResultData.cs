using UnityEngine;
using static Constants;

[System.Serializable]
public class GuestResultData
{
    public int veryBadCount;
    public int badCount;
    public int neutralCount;
    public int goodCount;
    public int veryGoodCount;

    public int totalCount;

    public void AddResult(GuestMood mood)
    {
        totalCount++;

        switch(mood)
        {
            case GuestMood.VeryBad:     veryBadCount++;     break;
            case GuestMood.Bad:         badCount++;         break;
            case GuestMood.Neutral:     neutralCount++;     break;
            case GuestMood.Good:        goodCount++;        break;
            case GuestMood.VeryGood:    veryGoodCount++;    break;
        }
    }

    public void Reset()
    {
        veryBadCount = 0;
        badCount = 0;
        neutralCount = 0;
        goodCount = 0;
        veryGoodCount = 0;
    }
}
