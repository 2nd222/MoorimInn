using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlaceManager : MonoBehaviour
{
    private GuestManager guestManager;
    
    public TMP_InputField openTableCount;
    public GameObject[] tableSets;
    private int currentTableIndex;

    public GameObject[] cookingStations;
    public GameObject[] kitchenTables;
    private int currentStoveIndex;

    private void Awake()
    {
        guestManager = FindFirstObjectByType<GuestManager>();
        
        currentTableIndex = tableSets.Count(t => t.activeSelf);
        currentStoveIndex = cookingStations.Count(t => t.activeSelf);
    }
    
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Debug.Log("Press R");
            guestManager.SpawnGuset();
        }
    }
    
    private void OnEnable()
    {
        UpgradeManager.Instance.SelectedOptionUpgraded += ApplyUpgrade;
    }

    private void OnDisable()
    {
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.SelectedOptionUpgraded -= ApplyUpgrade;
    }
    
    private void ApplyUpgrade(UpgradeType type)
    {
        Debug.Log($"ApplyUpgrade : {type}");
        switch (type)
        {
            case UpgradeType.TableCount:
                OpenTable();
                break;

            case UpgradeType.CookStationAmount:
                OpenStove();
                break;
        }
    }

    private void OpenTable()
    {
        int openCount = Mathf.RoundToInt(UpgradeManager.Instance.GetValue(UpgradeType.TableCount));
        Debug.Log($"TableCount Value : {openCount}");
        if (openCount <= 0)
            return;
        
        int limit = Mathf.Min(openCount, tableSets.Length);
        
        //
        // // 입력값이 비어있거나 파싱에 실패할 경우를 대비한 안전 장치 (선택 사항)
        // if (!int.TryParse(openTableCount.text, out int targetCount)) return;
        //
        // // 현재 인덱스부터 targetCount만큼 추가로 활성화 (O(K), K는 추가할 테이블 수)
        // int limit = Mathf.Min(currentTableIndex + targetCount, tableSets.Length);
        
        for (int i = currentTableIndex; i < limit; i++)
        {
            if (!tableSets[i].activeSelf)
            {
                tableSets[i].SetActive(true);
                currentTableIndex++; // 다음번에 열기 시작할 위치 저장
            }
        }
    }

    private void OpenStove()
    {
        int targetOpened = Mathf.RoundToInt(UpgradeManager.Instance.GetValue(UpgradeType.CookStationAmount));
        int limit = Mathf.Min(targetOpened, tableSets.Length)-1;
        
        for (int i = 0; i < limit; i++)
        {
            if (!cookingStations[i].activeSelf)
            {
                cookingStations[i].SetActive(true);
                kitchenTables[i].SetActive(false);
                currentStoveIndex++;
            }
        }
    }
    
    public void Register(GameObject[] tables, GameObject[] stations, GameObject[] kitchens, GuestManager guest)
    {
        tableSets = tables;
        cookingStations = stations;
        kitchenTables = kitchens;
        guestManager = guest;

        currentTableIndex = tableSets.Count(t => t.activeSelf);
        currentStoveIndex = cookingStations.Count(t => t.activeSelf);
    }
}