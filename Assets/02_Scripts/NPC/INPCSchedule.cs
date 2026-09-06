using System;
using UnityEngine;

public interface INPCSchedule
{
    bool IsAvailableDay();
    void ViewActive();
    void DailyQuestCreate();
}

public enum NPCUIState
{
    Shop,
    Quest,
    Talk,
    Debt,        
    Upgrade,
}

[Serializable]
public class NPCDialogueData
{
    [TextArea]
    public string text;

    public ExpressionData expression;
}