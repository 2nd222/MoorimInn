using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebtPanel : MonoBehaviour
{
    [SerializeField] private RestaurantEconomy restaurantEconomy;
    [SerializeField] private TextMeshProUGUI debtText;
    
    [SerializeField] private TMP_InputField amountInput;
    
    [SerializeField] private Button repayButton;
    [SerializeField] private Button cancelButton;
    
    [SerializeField] private MerchantUI merchantUI;
    
    private int remainDebt;

    public event Action FullPayDebt;
    
    private void OnEnable()
    {
        repayButton.onClick.AddListener(RepaymentDebt);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
        remainDebt = restaurantEconomy.Debt;

        amountInput.text = null;
        
        UpdateUI();
    }

    private void OnDisable()
    {
        repayButton.onClick.RemoveListener(RepaymentDebt);
        cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
    }
    
    private void UpdateUI()
    {
        string debtString = $"남은 빚 : {remainDebt}\n현재 소지금 : {restaurantEconomy.Money}";
        debtText.text = debtString;
    }
    
    private int GetInputAmount()
    {
        if (string.IsNullOrEmpty(amountInput.text))
            return 0;

        if (!int.TryParse(amountInput.text, out int amount))
            return 0;

        return Mathf.Max(0, amount);
    }
    
    private void RepaymentDebt()
    {
        int amount = GetInputAmount();

        if (amount <= 0)
        {
            merchantUI.SetHappyExpression();
            merchantUI.ChangeText("얼마를 갚을 거야?");
            return;
        }

        if (!EconomyManager.Instance.CanSpend(amount))
        {
            merchantUI.SetSurprisedExpression();
            merchantUI.ChangeText("갖고 있는 돈보다 많이 부른 거 같아...");
            return;
        }
        
        bool isOverPay = amount > remainDebt;

        if (isOverPay)
        {
            amount = remainDebt;
        }
        else
        {
            merchantUI.SetHappyExpression();
            merchantUI.ChangeText($"좋아. {amount}만큼 갚은 거야.");
        }

        EconomyManager.Instance.SpendMoney(amount);
        remainDebt -= amount;
        EconomyManager.Instance.RepayDebt(amount);
        
        if (remainDebt <= 0)
        {
            if (isOverPay)
            {
                merchantUI.SetSurprisedExpression();
                merchantUI.ChangeText($"너무 많이 줬어. {amount}원 정도면 충분해. 빚은 전부 갚았어!");
            }
            else
            {
                merchantUI.SetHappyExpression();
                merchantUI.ChangeText("빚을 전부 갚았어. 좋아! 이제 빚은 없어.");
            }
            FullPayDebt?.Invoke();
        }
        amountInput.text = null;
        UpdateUI();
    }

    private void OnCancelButtonClicked()
    {
        merchantUI.SetDefaultExpression();
        merchantUI.ChangeText("돈은 나중에 갚아도 돼");
        gameObject.SetActive(false);
    }
}
