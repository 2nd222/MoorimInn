using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingButton : MonoBehaviour
{
    [SerializeField] private Button settingButton;

    private void Start()
    {
        settingButton.onClick.AddListener(OnClickSettingsButton);
    }
    
    private void OnClickSettingsButton()
    {
        UIManager.Instance.ActiveSetting(true);
    }

    private void OnDestroy()
    {
        settingButton.onClick.RemoveListener(OnClickSettingsButton);
    }
}
