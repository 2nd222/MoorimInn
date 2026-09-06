using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
public class QuestSlot : MonoBehaviour
{
    public QuestData questData { get; private set; }
    [SerializeField] private Button btnSlot;
    [SerializeField] private TextMeshProUGUI txtQuestName;
    [SerializeField] private GameObject objSelectArrow;



    public void Init(Quest quest, Action<QuestSlot> onClickAction)
    {
        this.questData = quest.Data;

        this.txtQuestName.text = quest.QuestName;

        this.btnSlot.onClick.RemoveAllListeners();

        this.btnSlot.onClick.AddListener(() =>
        {
            if (this.questData != null)
            {
                onClickAction?.Invoke(this);
            }
        });
    }
    public void SetSelect(bool isSelected)
    {
        if (this.objSelectArrow != null)
        {
            this.objSelectArrow.SetActive(isSelected);
        }
    }
}
