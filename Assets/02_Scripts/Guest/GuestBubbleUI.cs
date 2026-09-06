using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Constants;

public class GuestBubbleUI : MonoBehaviour
{
    [Header("Bubble")]
    [SerializeField] private GameObject moodBubble;
    [SerializeField] private RectTransform moodRect;
    [SerializeField] private GameObject statusBubble;
    [SerializeField] private RectTransform statusRect;

    [Header("Bubble UI")]
    [SerializeField] private Image moodIcon;
    [SerializeField] private Image statusIcon;
    [SerializeField] private Image statusFill;
    
    [Header("Mood UI")]
    [SerializeField] private Sprite veryBadSprite;
    [SerializeField] private Sprite badSprite;
    [SerializeField] private Sprite neutralSprite;
    [SerializeField] private Sprite goodSprite;
    [SerializeField] private Sprite veryGoodSprite;

    [Header("State UI")]
    [SerializeField] private Sprite orderSprite;
    [SerializeField] private Sprite paySprite;
    
    private Coroutine gaugeCoroutine;

    private GuestBase guest;

    void Awake()
    {
        moodRect = moodBubble.GetComponent<RectTransform>();
        statusRect = statusBubble.GetComponent<RectTransform>();
        
        
    }

    public void Bind(GuestBase target)
    {
        guest = target;

        guest.OnGuestStateChanged += BubbleChanged;
        guest.OnGuestMoodChanged += MoodChanged;
        guest.OnGuestReadyToOrder += ReadyToOrder;
        guest.OnOrderPlaced += OrderPlaced;
        guest.OnGuestReadyToPay += ReadyToPay;
        guest.OnOrderChanged += OrderPlaced;
    }

    private void BubbleChanged(GuestBase g, GuestState state)
    {
        switch (state)
        {
            case GuestState.WaitOrder:
                // 착석 → 기분 말풍선 켜기
                ShowBubble(moodBubble, moodRect);
                moodIcon.sprite = GetMoodSprite(g.CurrentMood);
                break;

            case GuestState.WaitFood:
                // 기분 유지, 상태 말풍선은 OrderPlaced에서 켜짐
                StartGauge();
                break;

            case GuestState.Eat:
                // 기분 유지, 상태 말풍선 끄기 (음식 받았으니까)
                HideBubble(moodBubble, moodRect);
                HideBubble(statusBubble, statusRect);
                StopGauge();
                break;

            case GuestState.Pay:
                // 계산 할 땐 돈, 기분 아이콘 다 킴.
                // 둘 다 ReadyToPay에서 켜줌
                break;

            case GuestState.Exit:
                // 전부 끄기
                HideBubble(moodBubble, moodRect);
                HideBubble(statusBubble, statusRect);
                break;
        }
    }

    private void MoodChanged(GuestBase g, GuestMood mood)
    {
        moodIcon.sprite = GetMoodSprite(mood);
        UpdateGaugeColor(mood);
    }

    private void ReadyToOrder(GuestBase g)
    {
        // 메뉴를 골랐을 때 주문표 아이콘 표시, 게이지 시작
        ShowBubble(statusBubble, statusRect);
        statusIcon.sprite = orderSprite;
        StartGauge();
    }

    private void OrderPlaced(GuestBase g, GuestOrderData order)
    {
        // 주문 접수 → 음식 아이콘으로 변경
        if (order.recipe != null && order.recipe.icon != null)
        {
            ShowBubble(statusBubble, statusRect);  // ★ moodRect 아니라 statusRect
            statusIcon.sprite = order.recipe.icon;
        }
    }

    private void ReadyToPay(GuestBase g)
    {
        // 계산 아이콘 표시 (Pop 애니메이션 포함)
        statusIcon.sprite = paySprite;
        ShowBubble(statusBubble, statusRect);
        
        moodIcon.sprite = GetMoodSprite(g.CurrentMood);
        ShowBubble(moodBubble, moodRect);
    }

    public void HideAll()
    {
        moodBubble.SetActive(false);
        statusBubble.SetActive(false);
        StopGauge();
    }
    
    public void SetViolenceBubble(bool isCausingTrouble, Sprite icon = null)
    {
        if (!isCausingTrouble)
        {
            HideBubble(statusBubble, statusRect);
            return;
        }

        HideBubble(moodBubble, moodRect);
        StopGauge();

        if (icon == null)
        {
            HideBubble(statusBubble, statusRect);
            return;
        }

        statusIcon.sprite = icon;
        ShowBubble(statusBubble, statusRect);
    }

    private Sprite GetMoodSprite(GuestMood mood)
    {
        switch (mood)
        {
            case GuestMood.VeryGood: return veryGoodSprite;
            case GuestMood.Good:     return goodSprite;
            case GuestMood.Neutral:  return neutralSprite;
            case GuestMood.Bad:      return badSprite;
            case GuestMood.VeryBad:  return veryBadSprite;
            default:                 return neutralSprite;
        }
    }

    private void ShowBubble(GameObject bubble, RectTransform rect)
    {
        bubble.SetActive(true);
        UIAnimationManager.Instance.ShowPop(rect);
    }

    private void HideBubble(GameObject bubble, RectTransform rect)
    {
        UIAnimationManager.Instance.HidePop(rect);
        bubble.SetActive(false);
    }
    
    private void StartGauge()
    {
        StopGauge();
        gaugeCoroutine = StartCoroutine(UpdateGauge());
    }

    private void StopGauge()
    {
        if (gaugeCoroutine != null)
        {
            StopCoroutine(gaugeCoroutine);
            gaugeCoroutine = null;
        }

        if (statusFill != null)
        {
            statusFill.gameObject.SetActive(false);
            statusFill.fillAmount = 0;
        }
    }

    private IEnumerator UpdateGauge()
    {
        if (statusFill == null)
            yield break;

        statusFill.gameObject.SetActive(true);
        UpdateGaugeColor(guest.CurrentMood);

        float startProgress = guest.AngerProgress;

        while (true)
        {
            if (guest.IsAngryExiting)
                break;

            statusFill.fillAmount = Mathf.InverseLerp(startProgress, 1f, guest.AngerProgress);
            yield return null;
        }

        statusFill.gameObject.SetActive(false);
        gaugeCoroutine = null;
    }
    
    private void UpdateGaugeColor(GuestMood mood)
    {
        if (statusFill == null) return;

        // Bad 이하면 빨강, 아니면 주황
        bool isBadMood = mood == GuestMood.Bad || mood == GuestMood.VeryBad;
        statusFill.color = isBadMood ? Color.red : Color.orange;
    }
}