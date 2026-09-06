using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BaseMenu : MonoBehaviour
{
    [Serializable]
    public class TabUIPair
    {
        public Toggle toggleTab;
        public BaseTab tabScript;
    }

    [Header("ег UI")]
    public TabUIPair[] tabs;

    public UnityAction<int> OnTabToggleClicked;

    private void Awake()
    {
        for (int i = 0; i < this.tabs.Length; i++)
        {
            int index = i;
            this.tabs[i].toggleTab.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                    this.OnTabToggleClicked?.Invoke(index);
            });
        }
    }

    public void ShowMenu(bool isShow) => this.gameObject.SetActive(isShow);
}