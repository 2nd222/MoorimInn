using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class BookModel
{
    // 현재 선택된 메뉴 인덱스
    public int CurrentMenuIndex { get; private set; } = -1;

    public Dictionary<int, int> ActiveTabPerMenu { get; private set; } = new Dictionary<int, int>();

    public UnityAction<int> OnMenuChanged;
    public UnityAction<int, int> OnTabChanged; // menuIndex, tabIndex

    // 메뉴를 선택했을 때 호출
    public void ChangeMenu(int menuIndex)
    {
        if (this.CurrentMenuIndex == menuIndex) return;

        this.CurrentMenuIndex = menuIndex;
        this.OnMenuChanged?.Invoke(menuIndex);

        if (!this.ActiveTabPerMenu.ContainsKey(menuIndex))
        {
            ChangeTab(menuIndex, 0);
        }
        else
        {
            this.OnTabChanged?.Invoke(menuIndex, this.ActiveTabPerMenu[menuIndex]);
        }
    }

    // 메뉴에서 다른 탭을 선택했을 때 호출
    public void ChangeTab(int menuIndex, int tabIndex)
    {
        this.ActiveTabPerMenu[menuIndex] = tabIndex;
        this.OnTabChanged?.Invoke(menuIndex, tabIndex);
    }
}
