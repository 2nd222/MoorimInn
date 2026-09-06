using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StoryTriggerSO", menuName = "Story/StoryTriggerSO")]
public class StoryTriggerSO : ScriptableObject
{
    public List<StoryTrigger> triggers;
}
