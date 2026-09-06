using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.UI;
public class Receipt : MonoBehaviour
{
    [SerializeField] private GameObject objReceipt;
    private ReceiptData data;

    private DayManager dayManager;
    [SerializeField] private GuestResultTracker guestTracker;

    [SerializeField] private TextMeshProUGUI txtDay;

    [Header("��ǥ �׸��")]
    [SerializeField] private ReceiptSlot fame;
    [SerializeField] private ReceiptSlot debt;
    [SerializeField] private ReceiptSlot happyGuests;
    [SerializeField] private ReceiptSlot angryGuests;
    [SerializeField] private ReceiptSlot netIncome;

    [SerializeField] private GameObject objStamp;
    [SerializeField] private Button btnConfirm;

    [Header("�ִϸ��̼� ���� �ð�")]
    [Range(0f, 3f)][SerializeField] private float labelDelay;
    [Range(0f, 3f)][SerializeField] private float valueDuration;
    [Range(0f, 3f)][SerializeField] private float netIncomeDelay;
    [Range(0f, 3f)][SerializeField] private float netIncomeDuration;
    [Range(0f, 3f)][SerializeField] private float appendInterval;
    [Range(0f, 3f)][SerializeField] private float lastAppendInterval;

    // ��ǥ ����
    private Sequence receiptSequence;
    
    void Awake()
    {
        dayManager = DayManager.Instance;
        EventInit();
        SetReceiptActive(false);
    }

    private void OnEnable()
    {
        StoryManager.Instance.SetReceipt(this);
        GameManager.Instance.SetReceipt(this);
    }
    
    private void EventInit()
    {
        this.btnConfirm.onClick.AddListener(() =>
        {
            //this.onButtonConfirm?.Invoke();
            SetReceiptActive(false);
            GameManager.Instance.CompleteState(GameFlowState.Receipt);
        });
    }

    public void TurnOnReceipt(ReceiptData? data)
    {
        if (data == null)
        {
            Debug.Log("데이터 null");
            return;
        }
        
        this.data = data.Value;
        SetReceiptActive(true);
        StartAnimationText();
    }
    
    private void Show()
    {
        GameManager.Instance.SetState(GameFlowState.Receipt);
        
        ClearSlotText();

        if (this.objStamp != null)
            this.objStamp.SetActive(false);
        if (this.btnConfirm != null)
            this.btnConfirm.gameObject.SetActive(false);
    }

    public void Hide()
    {
        ClearSequence();
        ClearSlotText();
    }

    public void StartAnimationText()
    {
        ClearSequence();

        this.txtDay.text = data.week.day + "일째";

        this.receiptSequence = DOTween.Sequence();
        this.receiptSequence.AppendInterval(0.5f);

        this.receiptSequence.AppendInterval(this.appendInterval);

        this.receiptSequence.Append(this.fame.AnimationText(this.data.fame, this.labelDelay, this.valueDuration));
        this.receiptSequence.AppendInterval(this.appendInterval);

        this.receiptSequence.Append(this.happyGuests.AnimationText(this.data.happyGuests, this.labelDelay, this.valueDuration));
        this.receiptSequence.AppendInterval(this.appendInterval);

        this.receiptSequence.Append(this.debt.AnimationText(this.data.debt, this.labelDelay, this.valueDuration));
        this.receiptSequence.AppendInterval(this.appendInterval);

        this.receiptSequence.Append(this.angryGuests.AnimationText(this.data.angryGuests, this.labelDelay, this.valueDuration));
        this.receiptSequence.AppendInterval(this.lastAppendInterval);

        this.receiptSequence.Append(this.netIncome.AnimationText(this.data.netIncome, this.netIncomeDelay, this.netIncomeDuration));

        this.receiptSequence.OnComplete(() => {
            this.receiptSequence = null;
            RectTransform rt = this.objStamp.GetComponent<RectTransform>();

            UIAnimationManager.Instance.ShowStamp(rt,
                onStart: () =>
                {
                    if (this.objStamp != null)
                        this.objStamp.SetActive(true);
                },
                onComplete: () =>
                {
                    if (this.btnConfirm != null)
                        this.btnConfirm.gameObject.SetActive(true);
                }
            );
        });
    }
    public void SetReceiptActive(bool isActive)
    {
        if (!isActive)
            Hide();
        else
            Show();
        
        this.objReceipt.SetActive(isActive);
    }
    private void ClearSequence()
    {
        if (this.receiptSequence != null)
        {
            this.receiptSequence.Kill();
            this.receiptSequence = null;
        }
    }

    private void ClearSlotText()
    {
        this.txtDay.text = "";
        this.fame.ClearText();
        this.debt.ClearText();
        this.happyGuests.ClearText();
        this.angryGuests.ClearText();
        this.netIncome.ClearText();
    }
}
