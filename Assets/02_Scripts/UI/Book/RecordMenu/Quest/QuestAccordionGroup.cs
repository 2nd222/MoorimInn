using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QuestAccordionGroup : MonoBehaviour
{
    [SerializeField] private Button btnHeader;
    [SerializeField] private Transform tsParent;     // 슬롯들이 들어갈 부모
    [SerializeField] private Transform tsArrowIcon;  // 화살표 아이콘

    [SerializeField] private VerticalLayoutGroup group;

    [SerializeField] private GameObject objQuestSlot; // 프리팹 연결

    private List<QuestSlot> slotPool = new List<QuestSlot>();
    private bool isOpen = false;

    private int currentDataCount = 0;

    private Coroutine refreshCoroutine;

    private void Start()
    {
        UpdateToggleUI();

        this.btnHeader.onClick.AddListener(() =>
        {
            OnClickGroup(!this.isOpen);
        });
    }
    public void UpdateList(List<Quest> questDataList, Action<QuestSlot> onClickAction)
    {
        this.currentDataCount = questDataList.Count;

        for (int i = 0; i < questDataList.Count; i++)
        {
            if (i >= this.slotPool.Count)
            {
                QuestSlot slot = Instantiate(this.objQuestSlot, this.tsParent).GetComponent<QuestSlot>();
                this.slotPool.Add(slot);
            }

            this.slotPool[i].Init(questDataList[i], onClickAction);

            this.slotPool[i].gameObject.SetActive(this.isOpen);
        }

        for (int i = questDataList.Count; i < this.slotPool.Count; i++)
        {
            this.slotPool[i].gameObject.SetActive(false);
        }
    }

    private void UpdateToggleUI()
    {
        for (int i = 0; i < this.currentDataCount; i++)
        {
            if (i < this.slotPool.Count)
            {
                this.slotPool[i].gameObject.SetActive(this.isOpen);
            }
        }
        if (this.tsArrowIcon != null)
        {
            float xRotation = this.isOpen ? 0f : 180f;
            this.tsArrowIcon.localRotation = Quaternion.Euler(xRotation, 0, 0);
        }
    }
    public void OnClickGroup(bool open)
    {
        if (!this.gameObject.activeInHierarchy)
            return;

        this.isOpen = open;
        UpdateToggleUI();

        // 임시
        if (this.refreshCoroutine != null)
        {
            StopCoroutine(this.refreshCoroutine);
        }

        this.refreshCoroutine = StartCoroutine(RefreshGroupRoutine());
    }
    private IEnumerator RefreshGroupRoutine()
    {
        this.group.enabled = false;

        yield return new WaitForSeconds(0.001f);

        this.group.enabled = true;
    }
}
