using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeSlotUI : MonoBehaviour
{
    
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI level;

    [SerializeField] private Button upgradeButton;
    
    [SerializeField] private Image background;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;
    
    private UpgradeData data;

    public void Init(UpgradeData data, Action<UpgradeData, UpgradeSlotUI> callback)
    {
        this.data = data;
        title.text = data.upgradeName;
        int lv = UpgradeManager.Instance.GetLevel(data.type);

        SetSelected(false);
        
        level.text = $"Lv.{lv}";
        upgradeButton.onClick.AddListener(() => { callback?.Invoke(data, this); });
    }
    
    public void SetSelected(bool isSelected)
    {
        background.sprite = isSelected ? selectedSprite : normalSprite;
    }
}
