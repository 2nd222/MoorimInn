using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 퀘스트 리스트 SO
/// </summary>
/// 
[CreateAssetMenu(fileName = "QuestList", menuName = "Scriptable Objects/QuestList")]
public class QuestList : ScriptableObject
{
    public List<Quest> quests;
}
