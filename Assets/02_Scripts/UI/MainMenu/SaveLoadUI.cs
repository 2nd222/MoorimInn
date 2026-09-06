using UnityEngine;
using UnityEngine.UI;

public enum SaveLoadMode
{
    Save,
    Load
}

public class SaveLoadUI : MonoBehaviour
{
    [SerializeField] private SaveSlotUI[] slots;
    [SerializeField] private Button closeButton;

    [SerializeField] private Image screenImage;
        
    [SerializeField] private Sprite saveScreen;
    [SerializeField] private Sprite loadScreen;
    
    private SaveLoadMode _currentMode;
    
    private void Start()
    {
        closeButton.onClick.AddListener(Close);
    }
    
    private void OpenUI(bool show, SaveLoadMode mode)
    {
        gameObject.SetActive(show);

        if(!show)
            return;

        _currentMode = mode;

        screenImage.sprite = mode == SaveLoadMode.Save ? saveScreen : loadScreen;

        slots[0].Init(0, SaveSlotType.Auto, mode);

        for(int i = 1; i < slots.Length; i++)
        {
            slots[i].Init(i, SaveSlotType.Manual, mode);
        }
    }
    
    public void OpenSaveUI(bool isShow)
    {
        OpenUI(isShow, SaveLoadMode.Save);
    }


    public void OpenLoadUI(bool isShow)
    {
        OpenUI(isShow, SaveLoadMode.Load);
    }

    private void Close()
    {
        if(_currentMode == SaveLoadMode.Save)
            UIManager.Instance.ActiveSaveUI(false);
        else
            UIManager.Instance.ActiveLoadUI(false);
    }
}
