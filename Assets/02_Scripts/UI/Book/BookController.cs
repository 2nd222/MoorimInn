using UnityEngine;

public class BookController : MonoBehaviour
{
    [SerializeField] private BookView bookView;

    private BookModel bookModel;

    private void Awake()
    {
        this.bookModel = new BookModel();

        this.bookView.OnMenuToggleClicked += HandleMenuClicked;

        for (int i = 0; i < bookView.menus.Length; i++)
        {
            int menuIndex = i;
            this.bookView.menus[i].menuView.OnTabToggleClicked += (tabIndex) => HandleTabClicked(menuIndex, tabIndex);
        }

        this.bookModel.OnMenuChanged += UpdateMenuView;
        this.bookModel.OnTabChanged += UpdateTabView;
    }

    void OnEnable()
    {
        InitData();
    }
    void OnDisable()
    {
        this.bookModel.ChangeMenu(0);
        this.bookModel.ChangeTab(0, 0);
    }


    // ��� �޴��� Ŭ���Ǿ��� �� Model�� ����
    private void HandleMenuClicked(int index)
    {
        this.bookModel.ChangeMenu(index);
    }

    // �ϴ� ���� Ŭ���Ǿ��� �� Model�� ����
    private void HandleTabClicked(int menuIndex, int tabIndex)
    {
        this.bookModel.ChangeTab(menuIndex, tabIndex);
    }

    // �޴� ���°� ������ �� ���� View�� �޴��� ��� UI�� ����ȭ
    private void UpdateMenuView(int activeMenuIndex)
    {
        for (int i = 0; i < bookView.menus.Length; i++)
        {
            bool isActive = (i == activeMenuIndex);

            this.bookView.menus[i].toggle.SetIsOnWithoutNotify(isActive);

            this.bookView.menus[i].menuView.ShowMenu(isActive);
        }
    }

    // �� ���°� ������ ��, ���� View�� �ǰ� ��� UI�� ����ȭ
    private void UpdateTabView(int menuIndex, int activeTabIndex)
    {
        BaseMenu activeMenu = this.bookView.menus[menuIndex].menuView;

        for (int i = 0; i < activeMenu.tabs.Length; i++)
        {
            bool isActive = (i == activeTabIndex);
            BaseTab tab = activeMenu.tabs[i].tabScript;

            activeMenu.tabs[i].toggleTab.SetIsOnWithoutNotify(isActive);
            tab.ShowTab(isActive);

            if (isActive)
            {
                tab.ResetSetting();
            }
        }
    }

    // å�� ó�� ���� �� ��� ���� ��ȸ�ϸ� �ʱ� ������ ����
    private void InitData()
    {
        foreach (var menuPair in this.bookView.menus)
        {
            foreach (var tabPair in menuPair.menuView.tabs)
            {
                if (tabPair.tabScript != null)
                {
                    tabPair.tabScript.SetupData();
                }
            }
        }
    }
}
