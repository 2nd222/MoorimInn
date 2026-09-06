using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StorySO", menuName = "Scriptable Objects/StorySO")]
public class StorySO : ScriptableObject
{
    public List<Story> stories;
}

