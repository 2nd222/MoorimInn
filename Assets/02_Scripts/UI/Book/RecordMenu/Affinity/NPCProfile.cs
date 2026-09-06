using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class NPCProfile : MonoBehaviour
{
    [SerializeField] Image imgIcon;
    [SerializeField] TextMeshProUGUI txtName;
    [SerializeField] Button btnSlot;

    public void Init(AffinityNPCData data, Action<AffinityNPCData> onClickAction)
    {
        this.imgIcon.sprite = data.icon;
        this.txtName.text = data.myName;

        this.btnSlot.onClick.RemoveAllListeners();

        this.btnSlot.onClick.AddListener(() =>
        {
            onClickAction?.Invoke(data);
        });
    }
}
