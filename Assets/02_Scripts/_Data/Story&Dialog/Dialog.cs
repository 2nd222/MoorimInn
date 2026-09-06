using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Dialog
{
    public int id;
    // 대화 상대 NPC
    
    [TextArea(3, 10)]
    public List<string> dialogs;
}