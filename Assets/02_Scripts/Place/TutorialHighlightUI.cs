using UnityEngine;

[ExecuteAlways] // 에디터 화면에서도 플레이를 누르지 않고 실시간으로 조절되는 걸 볼 수 있게 해줍니다.
public class TutorialHighlightUI : MonoBehaviour
{
    [Header("강조할 UI (이 부분만 구멍이 뚫림)")]
    public RectTransform targetHole; 

    [Header("4개의 반투명 가림막")]
    public RectTransform topPanel;
    public RectTransform bottomPanel;
    public RectTransform leftPanel;
    public RectTransform rightPanel;

    void LateUpdate()
    {
        // 타겟이나 패널이 하나라도 연결 안 되어있으면 작동하지 않음
        if (targetHole == null || topPanel == null || bottomPanel == null || leftPanel == null || rightPanel == null)
            return;

        RectTransform parentRect = GetComponent<RectTransform>();

        // 1. 타겟 사각형의 모서리 4개 실제 좌표 가져오기
        Vector3[] corners = new Vector3[4];
        targetHole.GetWorldCorners(corners);

        // 2. 부모(이 스크립트가 붙은 전체화면 패널) 기준의 로컬 좌표로 변환
        Vector3 bottomLeft = parentRect.InverseTransformPoint(corners[0]);
        Vector3 topRight = parentRect.InverseTransformPoint(corners[2]);

        // 3. 계산하기 쉽게 기준점을 부모의 좌측 하단(0, 0)으로 맞추기
        float canvasWidth = parentRect.rect.width;
        float canvasHeight = parentRect.rect.height;

        float targetX = bottomLeft.x - parentRect.rect.xMin;
        float targetY = bottomLeft.y - parentRect.rect.yMin;
        float targetW = topRight.x - bottomLeft.x;
        float targetH = topRight.y - bottomLeft.y;

        // 4. 4개의 가림막 크기와 위치를 타겟 주변으로 꽉 채우도록 자동 조절
        // 상단 가림막
        SetPanel(topPanel, 0, targetY + targetH, canvasWidth, canvasHeight - (targetY + targetH));
        // 하단 가림막
        SetPanel(bottomPanel, 0, 0, canvasWidth, targetY);
        // 좌측 가림막
        SetPanel(leftPanel, 0, targetY, targetX, targetH);
        // 우측 가림막
        SetPanel(rightPanel, targetX + targetW, targetY, canvasWidth - (targetX + targetW), targetH);
    }

    private void SetPanel(RectTransform panel, float x, float y, float width, float height)
    {
        // 스크립트가 앵커와 피벗을 좌측 하단(0,0)으로 강제로 고정해서 무조건 딱 맞물리게 합니다.
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.zero;
        panel.pivot = Vector2.zero;

        // 크기가 마이너스가 되는 것 방지
        width = Mathf.Max(0, width);
        height = Mathf.Max(0, height);

        panel.anchoredPosition = new Vector2(x, y);
        panel.sizeDelta = new Vector2(width, height);
    }
}