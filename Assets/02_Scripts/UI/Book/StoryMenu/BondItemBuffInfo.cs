using TMPro;
using UnityEngine;

public class BondItemBuffInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI buffValue;

    public void Setup(StatModifier modifier)
    {
        txtName.text = GetEffectName(modifier.type);
        buffValue.text = $"+{modifier.value}%";
    }
    
    private string GetEffectName(BondEffectType type)
    {
        return type switch
        {
            BondEffectType.FameGainRate => "명성 획득량",
            BondEffectType.MoneyGainRate => "골드 획득량",
            BondEffectType.CookingSpeed => "조리 속도",
            BondEffectType.CustomerPatience => "손님 인내심",
            BondEffectType.MoveSpeed => "이동 속도",
            _ => "알 수 없음"
        };
    }
}
