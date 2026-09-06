using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private int slotIndex;
    [SerializeField] private SaveSlotType slotType;

    [Header("UI")]
    [SerializeField] private TMP_Text slotNameText;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private Button button;

    private SaveLoadMode currentMode;

    public void Init(int index, SaveSlotType type, SaveLoadMode mode)
    {
        slotIndex = index;
        slotType = type;
        currentMode = mode;

        slotNameText.text = type == SaveSlotType.Auto
            ? "자동 저장"
            : $"슬롯 {index}";

        Refresh();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        string path = slotType == SaveSlotType.Auto ? GetSavePath() : GetManualPath();

        if(slotType == SaveSlotType.Manual && currentMode == SaveLoadMode.Save && !SaveManager.Instance.CanManualSave())
        {
            UIManager.Instance.CreateOkPopup(
                "오늘 영업을 시작한 이후에만 저장할 수 있습니다.",
                ()=>{},
                ()=>{},
                false);

            return;
        }
        
        if (currentMode == SaveLoadMode.Save)
        {
            if (slotIndex == 0)
            {
                UIManager.Instance.CreateOkPopup(
                    "자동 저장 파일에는 저장하실 수 없습니다.",
                    () => { }, () => {},false
                );
                return;
            }
            
            if (File.Exists(path))
            {
                UIManager.Instance.CreateOkPopup(
                    "덮어쓰시겠습니까?",
                    () =>
                    {
                        SaveManager.Instance.SaveGame(slotIndex, slotType);
                        Refresh();
                    },
                    () => {}, true
                );
            }
            else
            {
                SaveManager.Instance.SaveGame(slotIndex, slotType);
                Refresh();
            }
        }
        else
        {
            if (File.Exists(path))
                SaveManager.Instance.LoadGame(slotIndex, slotType);
            else
                UIManager.Instance.CreateOkPopup(
                    "해당 위치에는 저장된 파일이 없습니다.",
                    () => { }, () => {},false
                );
        }
    }

    public void Refresh()
    {
        if (slotType == SaveSlotType.Auto)
        {
            RefreshAuto();
        }
        else
        {
            RefreshManual();
        }
    }

    private void RefreshAuto()
    {
        string path = GetSavePath();

        if (!File.Exists(path))
        {
            infoText.text = "비어 있음";
            return;
        }

        SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));

        if(data == null || data.day == null)
        {
            infoText.text = "비정상 저장 파일";
            return;
        }
        
        string state = data.day.flowState switch
        {
            GameFlowState.Gameplay => "영업 중",
            GameFlowState.Receipt => "영업 종료",
            GameFlowState.Story => "스토리",
            GameFlowState.NPC => "NPC",
            _ => ""
        };

        infoText.text = $"{data.day.day}일차 ({state})";
    }
    
    private void RefreshManual()
    {
        string path = GetManualPath();

        if (!File.Exists(path))
        {
            infoText.text = "비어 있음";
            return;
        }

        ManualSaveData data = JsonUtility.FromJson<ManualSaveData>(File.ReadAllText(path));

        if(data == null || data.meta == null)
        {
            infoText.text = "비정상 저장 파일";
            return;
        }
        
        string state = data.meta.flowState switch
        {
            GameFlowState.Gameplay => "영업 중",
            GameFlowState.Receipt => "영업 종료",
            GameFlowState.Story => "스토리",
            GameFlowState.NPC => "NPC",
            _ => ""
        };

        infoText.text = $"{data.meta.day}일차 ({state})\n{data.meta.saveTime}";
    }
    
    private string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, "auto_save.json");
    }

    private string GetManualPath()
    {
        return Path.Combine(Application.persistentDataPath, $"save_{slotIndex}.json");
    }
}
