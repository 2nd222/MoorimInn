using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 추적되는 대상에 대한 관찰자의 인터페이스
/// </summary>
public interface ISubject
{
    void AddObserver(IObserver observer);
    void RemoveObserver(IObserver observer);
    void NotifyListener(QuestEvent questEvent, QuestType questType);
}