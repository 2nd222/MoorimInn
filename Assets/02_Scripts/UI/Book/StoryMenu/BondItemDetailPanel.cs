using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BondItemDetailPanel : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtDescription;
    [SerializeField] private TextMeshProUGUI txtStory;

    [SerializeField] private Transform buffContent;
    [SerializeField] private BondItemBuffInfo buffPrefab;
    
    public void SetItem(IItem item)
    {
        BondItemData data = item.Data as BondItemData;

        if (data == null)
            return;

        icon.sprite = data.icon;
        txtName.text = data.itemName;
        txtDescription.text = data.description; // 물건에 대한 묘사
        txtStory.text = data.storyText; // 엮인 스토리에 대한 설명
        
        RefreshBuffs(data);
    }
    
    private void RefreshBuffs(BondItemData data)
    {
        foreach (Transform child in buffContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var modifier in data.modifiers)
        {
            BondItemBuffInfo slot = Instantiate(buffPrefab, buffContent);
            slot.Setup(modifier);
        }
    }
}
