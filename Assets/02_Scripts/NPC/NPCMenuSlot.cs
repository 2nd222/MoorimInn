using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCMenuSlot : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI title;

    private NPCMenuData data;
    public Button Button => button;
    
    public void Init(NPCMenuData data, Action<NPCMenuData> callback)
    {
        this.data = data;

        title.text = data.buttonName;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            callback?.Invoke(data);
        });
    }
}

[Serializable]
public class NPCMenuData
{
    public string buttonName;

    public NPCUIState state;

    public ExpressionData expression;
}