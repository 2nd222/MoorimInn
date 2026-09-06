using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BookView : MonoBehaviour
{
    [Serializable]
    public class MenuUIPair
    {
        public Toggle toggle;
        public BaseMenu menuView;
    }

    [Header("¸Þ´º UI")]
    public MenuUIPair[] menus;

    public UnityAction<int> OnMenuToggleClicked;

    private void Awake()
    {
        for (int i = 0; i < this.menus.Length; i++)
        {
            int index = i;
            this.menus[i].toggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                    this.OnMenuToggleClicked?.Invoke(index);
            });
        }
    }

    public void ShowBook(bool isShow) => this.gameObject.SetActive(isShow);
}