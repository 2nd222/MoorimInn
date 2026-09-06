using System.Collections;
using UnityEngine;
using static Constants;

public class BottomSlideAnim : MonoBehaviour
{
    private RectTransform rectTransform;

    [Header("애니메이션 설정")]
    [SerializeField] private float hiddenPosY = 120f;   // 숨어있을 때의 Y 좌표 
    [SerializeField] private float shownPosY = 242f;       // 다 올라왔을 때의 Y 좌표

    [SerializeField] private float duration = 0.2f;  // 이동 시간

    private int openCount = 0; // 동시에 열린 개수 추적

    private Coroutine currentRoutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // 시작할 때는 무조건 숨겨둔 상태로 시작
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, hiddenPosY);
    }

    void Start()
    {
        InventoryManager.Instance.OnInventoryUIOpened += OnInvOpened;
        InventoryManager.Instance.OnInventoryUIClosed += OnInvClosed;
        InventoryManager.Instance.OnBottomUIToggleRequested += OnToggleRequested;
        
        DayManager.Instance.OnDayEnd += ForceHide;
    }

    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryUIOpened -= OnInvOpened;
            InventoryManager.Instance.OnInventoryUIClosed -= OnInvClosed;
            InventoryManager.Instance.OnBottomUIToggleRequested -= OnToggleRequested;
        }
        
        if (DayManager.Instance != null)
            DayManager.Instance.OnDayEnd -= ForceHide;
    }

    // private void OnInvOpened(Inventory inven) => MoveUI(inven, true);
    private void OnInvOpened(Inventory inven)
    {
        //Debug.Log($"[BottomSlideAnim] OnInvOpened: {inven.Type}");
        MoveUI(inven, true);
    }
    private void OnInvClosed(Inventory inven) => MoveUI(inven, false);

    private bool IsTriggerInventory(InventoryType type)
    {
        return type == InventoryType.Fridge
            || type == InventoryType.MealTable;
    }

    private void OnToggleRequested(bool isOpen)
    {
        if (isOpen) openCount++;
        else openCount = Mathf.Max(0, openCount - 1);

        bool shouldBeUp = openCount > 0;

        if (currentRoutine != null) StopCoroutine(currentRoutine);
        float targetY = shouldBeUp ? shownPosY : hiddenPosY;
        currentRoutine = StartCoroutine(SmoothMove(targetY));
    }
    
    private void MoveUI(Inventory inven, bool isOpen)
    {
        if (!IsTriggerInventory(inven.Type))
            return;

        // 열림 카운트 관리 (Chef/Server 두 개가 동시에 열릴 수 있으니까)
        if (isOpen) openCount++;
        else openCount = Mathf.Max(0, openCount - 1);

        bool shouldBeUp = openCount > 0;

        if (currentRoutine != null) StopCoroutine(currentRoutine);
        float targetY = shouldBeUp ? shownPosY : hiddenPosY;
        currentRoutine = StartCoroutine(SmoothMove(targetY));
    }

    private IEnumerator SmoothMove(float targetY)
    {
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = new Vector2(startPos.x, targetY);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 자연스러운 가속/감속을 원한다면 t = t * t * (3f - 2f * t); 를 추가하세요.
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        rectTransform.anchoredPosition = endPos;
    }

    private void ForceHide()
    {
        openCount = 0;

        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(SmoothMove(hiddenPosY));
    }

}
