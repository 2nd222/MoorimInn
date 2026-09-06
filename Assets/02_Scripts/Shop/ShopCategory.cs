using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Constants;
using UnityEngine.Events;

public class ShopCategory : MonoBehaviour
{
    [SerializeField]
    private Toggle toggle;
    [SerializeField]
    private TextMeshProUGUI categoryName;

    public UnityAction<bool> onToggleEvent;

    public void Init(string categoryName)
    {
        this.categoryName.text = categoryName;

        this.toggle.onValueChanged.AddListener((bool isOn) =>
        {
            this.onToggleEvent?.Invoke(isOn);
        });
    }

    public void SetToggleGloup(ToggleGroup group)
    {
        this.toggle.group = group;
    }
}
