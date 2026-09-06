using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager>
{
    private Canvas _canvas;
    [SerializeField] private GameObject dialogPopupPrefab;
    
    [SerializeField] private GameObject amountPopupObj;
    [SerializeField] private GameObject okPopupObj;

    QuestPopupController _questPopupCtr;
    OrdersPopupController _orderPopupCtr;
    [SerializeField] private GameObject defaultUI;

    [SerializeField] private BookView book;
    [SerializeField] private Setting setting;
    [SerializeField] private SaveLoadUI saveLoadUI;
    
    private GameObject inventory;
    
    public bool IsBookActive => book != null && book.gameObject.activeSelf;  // 혹은 book.IsShowing 같은 프로퍼티
    public bool IsSettingActive => setting != null && setting.gameObject.activeSelf;
    public bool IsSaveLoadActive => saveLoadUI != null && saveLoadUI.gameObject.activeSelf;

    public UnityAction<float> OnFontSizeChanged;    // 스토리UI 글자 크기

    // temp 
    private Dictionary<Recipe, int> tempRecipes;
    [SerializeField] private List<RecipeData> recipes;
    List<Order> tempOrders = new List<Order>();
    
    protected override void Awake()
    {
        base.Awake();
        _canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        BindSceneUI();
        
        _questPopupCtr = FindFirstObjectByType<QuestPopupController>();
        _orderPopupCtr = FindFirstObjectByType<OrdersPopupController>();
        
        // OpenDefaultUI();
        
        
        // temp
        foreach (var recipe in recipes)
        {
            Recipe newRecipe = new Recipe(recipe);
            Dictionary<Recipe, int> order = new  Dictionary<Recipe, int>();
            order.Add(newRecipe, Random.Range(0, 3));
            tempOrders.Add(new Order(order));
        }
    }

    protected override void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        base.OnDestroy();
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindSceneCanvas();
        BindSceneUI();
    }
    
    // public void OpenDefaultUI()
    // {
    //     Instantiate(defaultUI,  _canvas.transform);
    // }

    // 글자 크기 저장
    public void SetTextScale(float scale)
    {
        float clampedSize = Mathf.Clamp(scale, 30f, 50f);

        PlayerPrefs.SetFloat("StoryFontSize", scale);
        PlayerPrefs.Save();

        this.OnFontSizeChanged?.Invoke(scale);
    }

    public float GetTextScale()
    {
        return PlayerPrefs.GetFloat("StoryFontSize", 40f);
    }

    // 해상도 저장
    public void SetResolutionIndex(int index)
    {
        PlayerPrefs.SetInt("ResolutionIndex", index);
        PlayerPrefs.Save();
    }

    // 저장된 해상도 인덱스 불러오기(기본값 2번)
    public int GetResolutionIndex()
    {
        return PlayerPrefs.GetInt("ResolutionIndex", 2);
    }

    // 공용 인벤토리 호출시 나오는 함수
    public void OpenCommonInventoryPopup(InventoryPopupController commonInventory)
    {
        if (commonInventory == null) return;

        commonInventory.Show();
    }
    
    // 공용 인벤 내릴 때 호출
    public void CloseCommonInventoryPopup(InventoryPopupController commonInventory)
    {
        if (commonInventory == null) return;

        commonInventory.Hide();
    }
    
    // 각 인벤토리 열 때 호출되는 함수
    public void OpenInventoryPopup(Inventory inventory)
    {
        var popup = inventory.transform.parent.GetComponent<InventoryPopupController>();
        
        if (popup == null)
        {
            Debug.LogError("Popup 없음");
            return;
        }

        popup.Show();
    }
    
    // 공용 인벤토리 닫기
    public void CloseInventoryPopup(Inventory inventory)
    {
        var popup = inventory.GetComponentInParent<InventoryPopupController>();

        if (popup != null)
        {
            popup.Hide();
        }
    }
    
    // 스토리에서 대화창 열기
    public void OpenDialogPopup(string characterName, string dialog)
    {
        var dialogPopupObject = Instantiate(dialogPopupPrefab, _canvas.transform);
        dialogPopupObject.GetComponent<DialogPopupController>().Show(characterName, dialog);
    }
    
    // 게임씬의 오른쪽 위 버튼 누르면 나오는 활성화 퀘스트 창 열기
    public void OpenQuestPopup(GameObject questObj)
    {
        questObj.SetActive(!questObj.activeSelf);
        if (questObj.activeSelf)
        {
            _questPopupCtr = defaultUI.GetComponent<QuestPopupController>();
            if (_questPopupCtr == null)
            {
                Debug.LogError("QuestPopupCtrl is null");
                return;
            }
            _questPopupCtr.Showt();
        }
            
    }

    // 게임씬의 오른쪽 위 버튼 누르면 나오는 활성화 주문 창 열기
    public void OpenOrdersPopup(GameObject orderObj)
    {
        orderObj.SetActive(!orderObj.activeSelf);
        if (orderObj.activeSelf)
        {
            _orderPopupCtr = defaultUI.GetComponent<OrdersPopupController>();
            if (_orderPopupCtr == null)
            {
                Debug.LogError("OrdersPopupCtrl is null");
                return;
            }
            _orderPopupCtr.Show();
        }
            
    }
    
    // 아직 쓸지 미정(Guest에서 구현시 삭제필요)
    public void EnableGuestOrderUI(/*GuestBase guestBase*/)
    {
        // Instantiate 말고 enabled로 바꿀수도
        var guestOrderUIObject = Instantiate(dialogPopupPrefab/* guestBase*/);
        
        guestOrderUIObject.GetComponent<OrderIconController>().Showt();
    }

    // 갯수가 필요한 팝업
    public AmountPopup CreateAmountPopup(string message, ItemData itemData, int owned, int maxBuyable, UnityAction<int> onOk, UnityAction onCancel)
    {
        var popup = Instantiate(amountPopupObj, _canvas.transform).GetComponent<AmountPopup>();
        popup.Init(itemData, owned, maxBuyable);
        popup.ButtonOk(onOk);
        popup.ButtonCancel(onCancel);

        return popup;
    }
    
    public AmountPopup CreateAmountPopup(string message, RecipeData recipeData, UnityAction<int> onOk, UnityAction onCancel)
    {
        var popup = Instantiate(amountPopupObj, _canvas.transform).GetComponent<AmountPopup>();
        popup.Init(recipeData);
        popup.ButtonOk(onOk);
        popup.ButtonCancel(onCancel);

        return popup;
    }
    // 설명을 띄워줄 팝업
    public OkPopup CreateOkPopup(string message, UnityAction onOk, UnityAction onCancel, bool useCancel)
    {
        if (_canvas == null)
        {
            BindSceneCanvas();
        }

        if (_canvas == null)
        {
            Debug.LogError("Canvas 없음 → Popup 생성 실패");
            return null;
        }
        
        var popup = Instantiate(okPopupObj, _canvas.transform).GetComponent<OkPopup>();
        popup.Init(message, useCancel);
        popup.ButtonOk(onOk);
        popup.ButtonCancel(onCancel);

        return popup;
    }

    // 도감
    public void ActiveBook(bool isShow)
    {
        if (book == null)
        {
            Debug.Log("Book 없음 (현재 씬)");
            return;
        }
        
        book.ShowBook(isShow);
        
        if(setting)
            setting.Hide();
        
        CheckAndPauseGame();
    }
    // 설정창
    public void ActiveSetting(bool isShow)
    {
        if (isShow)
            setting.Show(); 
        else
            setting.Hide();
        
        if(book)
            book.ShowBook(false);
        
        CheckAndPauseGame();
    }
    // 세이브 화면
    public void ActiveSaveUI(bool isShow)
    {
        if(setting != null)
        {
            if(setting.isActiveAndEnabled)
                setting.Hide();
        }
        
        if (isShow)
            saveLoadUI.OpenSaveUI(isShow); 
        else
            saveLoadUI.OpenSaveUI(isShow);
        CheckAndPauseGame();
    }
    // 로드 화면
    public void ActiveLoadUI(bool isShow)
    {
        if(setting != null)
        {
            if(setting.isActiveAndEnabled)
                setting.Hide();
        }
        
        if (isShow)
            saveLoadUI.OpenLoadUI(isShow); 
        else
            saveLoadUI.OpenLoadUI(isShow);
        CheckAndPauseGame();
    }
    
    
    private void CheckAndPauseGame()
    {
        //Debug.Log($"Book={IsBookActive}, Setting={IsSettingActive}, Save={IsSaveLoadActive}");
        
        bool recallStoryPlaying = StoryManager.Instance != null && StoryManager.Instance.IsRecallPlaying;
        
        if (IsBookActive || IsSettingActive || IsSaveLoadActive || recallStoryPlaying )
            DayManager.Instance.PauseTime();
        else 
            DayManager.Instance.ResumeTime();
    }

    private void BindSceneUI()
    {
        book = FindFirstObjectByType<BookView>(FindObjectsInactive.Include);
        setting = FindFirstObjectByType<Setting>(FindObjectsInactive.Include);
        saveLoadUI = FindFirstObjectByType<SaveLoadUI>(FindObjectsInactive.Include);
    }

    private void BindSceneCanvas()
    {
        var canvasObj = GameObject.FindGameObjectWithTag("Canvas");

        if (canvasObj == null)
        {
            Debug.LogError("Canvas 못 찾음");
            return;
        }

        _canvas = canvasObj.GetComponent<Canvas>();
    }
    
    public void CloseAllUI()
    {
        // 개별 UI 닫기
        if (book != null)
            book.ShowBook(false);

        if (setting != null)
            setting.Hide();

        if (saveLoadUI != null)
            saveLoadUI.OpenSaveUI(false); // 내부에서 둘 다 꺼지게 처리되어 있으면 OK
        // 아니면 OpenLoadUI(false)도 따로 호출

        // 인벤토리 팝업들 전부 닫기
        var popups = FindObjectsByType<InventoryPopupController>(FindObjectsSortMode.None);
        foreach (var popup in popups)
        {
            popup.Hide();
        }

        // 퀘스트 / 주문 팝업
        var quest = FindFirstObjectByType<QuestPopupController>(FindObjectsInactive.Include);
        if (quest != null)
            quest.Hide();

        /*
        var order = FindFirstObjectByType<OrdersPopupController>(FindObjectsInactive.Include);
        if (order != null)
            order.Hide();
        */

        var cook = FindFirstObjectByType<CookingController>(FindObjectsInactive.Include);
        if (cook != null)
            cook.CloseCookingSystem(true);
        
        // 🔥 핵심: 시간 복구
        Time.timeScale = 1f;
    }
}