using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestPool", menuName =  "Quest/QuestPool")]
public class QuestPool : ScriptableObject
{
    public List<QuestData> quests;
}
