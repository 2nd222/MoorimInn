using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Constants;
using System.Collections.Generic;

public class RecipeCategoryTab : BaseTab
{

    [SerializeField] private FoodType categoryType;
    private List<RecipeMastery> currentList = new List<RecipeMastery>();

    [SerializeField] private RecipePage leftPage;
    [SerializeField] private RecipePage rightPage;

    [SerializeField] private Button btnPrev;
    [SerializeField] private Button btnNext;

    private int currentIndex = 0;
    void Awake()
    {
        this.btnPrev.onClick.AddListener(OnClickPrev);
        this.btnNext.onClick.AddListener(OnClickNext);
    }
    public override void SetupData()
    {
        this.currentList = DataManager.Instance.GetRecipeList(this.categoryType);
        this.currentIndex = 0;
        UpdatePageUI();
    }
    private void UpdatePageUI()
    {
        // 왼쪽 페이지
        if (this.currentIndex < this.currentList.Count)
        {
            this.leftPage.UpdatePage(this.currentList[this.currentIndex]);
        }
        else
        {
            this.leftPage.HidePage();
        }

        // 오른쪽 페이지
        int rightIndex = this.currentIndex + 1;
        if (rightIndex < this.currentList.Count)
        {
            this.rightPage.UpdatePage(this.currentList[rightIndex]);
        }
        else
        {
            this.rightPage.HidePage();
        }

        this.btnPrev.interactable = (this.currentIndex > 0);
        this.btnNext.interactable = (this.currentIndex + 2 < this.currentList.Count);
    }
    private void OnClickPrev()
    {
        this.currentIndex -= 2;
        UpdatePageUI();
    }

    private void OnClickNext()
    {
        this.currentIndex += 2;
        UpdatePageUI();
    }
    public void ChangeCategory(FoodType newType)
    {
        this.categoryType = newType;

        SetupData();
    }

    public override void ResetSetting() { }
}
