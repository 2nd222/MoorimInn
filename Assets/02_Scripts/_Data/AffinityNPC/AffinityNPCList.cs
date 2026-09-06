using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "AffinityNPCList", menuName = "Scriptable Objects/AffinityNPCList")]
public class AffinityNPCList : ScriptableObject
{
    public List<AffinityNPCData> listAffinityNPC;
}
