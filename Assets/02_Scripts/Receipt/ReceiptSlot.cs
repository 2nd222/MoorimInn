using UnityEngine;
using TMPro;
using DG.Tweening;

public enum ReceiptType
{
    None,
    Income,
    Expense
}
public enum UnitType
{
    None,
    Currency,
    People
}
public class ReceiptSlot : MonoBehaviour
{
    [SerializeField]
    private ReceiptType receiptType;
    [SerializeField]
    private UnitType unitType;
    [Header("Label 이름")]
    [SerializeField] 
    private string labelName;

    [SerializeField]
    private TextMeshProUGUI txtLabel;
    [SerializeField]
    private TextMeshProUGUI txtValue;
    void Start()
    {
        SetRowColor();
    }
    public void ClearText()
    {
        this.txtLabel.text = "";
        this.txtValue.text = "";
    }
    private void SetRowColor()
    {
        this.txtValue.color = this.receiptType switch
        {
            ReceiptType.Income => Color.green,  // 흑자일 때 초록
            ReceiptType.Expense => Color.red,   // 적자일 때 빨강
            _ => Color.darkGray
        };
    }
    public Tween AnimationText(int targetValue, float labelDelay, float valueDuration)
    {
        Sequence rowSeq = DOTween.Sequence();
        int currentValue = 0;

        (string prefix, string suffix) = GetUnitStrings();

        rowSeq.AppendCallback(() =>
        {
            this.txtLabel.text = this.labelName;
        });

        rowSeq.AppendInterval(labelDelay);

        rowSeq.Append(DOTween.To(() => currentValue, x => currentValue = x, targetValue, valueDuration)
            .OnUpdate(() =>
            {
                this.txtValue.text = $"{prefix}{currentValue}{suffix}";
            })
            .SetEase(Ease.OutQuad)
        );

        return rowSeq;
    }
    private (string prefix, string suffix) GetUnitStrings()
    {
        return this.unitType switch
        {
            UnitType.Currency => ("$ ", ""),   // 앞에 $ 붙임
            UnitType.People => ("", " 명"),  // 뒤에 명 붙임
            _ => ("", "")      // 없음
        };
    }
}
