using System;
using UnityEngine;

public class GuardManager : MonoBehaviour
{
    [SerializeField] private GameObject guard;

    private Transform originalTf;
    
    private void Awake()
    {
        originalTf = guard.transform;
        guard.SetActive(false);
    }

    private void OnEnable()
    {
        UpgradeManager.Instance.SelectedOptionUpgraded += ActiveGuard;
        DayManager.Instance.OnDayStart += GuardTransformReset;
        
        ActiveGuard(UpgradeType.Guard); // 불러오기 할 때 다시 한번 확인 용도
    }

    private void OnDisable()
    {
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.SelectedOptionUpgraded -= ActiveGuard;
        
        if(DayManager.Instance != null)
            DayManager.Instance.OnDayStart -= GuardTransformReset;
    }

    private void GuardTransformReset()
    {
        guard.transform.position = originalTf.position;
        guard.transform.rotation = originalTf.rotation;
    }
    
    private void ActiveGuard(UpgradeType upgrade)
    {
        if (upgrade == UpgradeType.Guard)
        {
            if(UpgradeManager.Instance.IsUnlocked(upgrade))
                guard.SetActive(true);
        }
    }
}
