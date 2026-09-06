using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatusTab : BaseTab
{
    [SerializeField] private GuestResultTracker guestTracker;
    // 왼쪽 페이지
    [SerializeField] private TextMeshProUGUI txtTotalRevenue;
    [SerializeField] private TextMeshProUGUI txtTotalFame;
    [SerializeField] private TextMeshProUGUI txtTotalHappyGuests;
    [SerializeField] private TextMeshProUGUI txtTotalAngryGuests;

    // 오른쪽 페이지

    // 명월
    [SerializeField] private PlayerData myungwolData;
    [SerializeField] private Image imgMyungwol;
    [SerializeField] private TextMeshProUGUI txtMyungwolLike;
    [SerializeField] private TextMeshProUGUI txtMyungwolDislike;
    [SerializeField] private TextMeshProUGUI txtMyungwolSkill;

    // 소월
    [SerializeField] private PlayerData sowolData;
    [SerializeField] private Image imgSowol;
    [SerializeField] private TextMeshProUGUI txtSowolLike;
    [SerializeField] private TextMeshProUGUI txtSowolDislike;
    [SerializeField] private TextMeshProUGUI txtSowolSkill;

    public override void SetupData()
    {
        ProfileSetting();
    }
    private void ProfileSetting()
    {
        this.txtTotalRevenue.text = EconomyManager.Instance.RestaurantEconomy.Money.ToString();
        this.txtTotalFame.text = EconomyManager.Instance.RestaurantEconomy.Fame.ToString();

        int happyGuests = this.guestTracker.GetTotalResult().goodCount + this.guestTracker.GetTotalResult().veryGoodCount;
        int angryGuests = this.guestTracker.GetTotalResult().badCount + this.guestTracker.GetTotalResult().veryBadCount;

        this.txtTotalHappyGuests.text = happyGuests.ToString();
        this.txtTotalAngryGuests.text = angryGuests.ToString();

        ProfileMyungwol(this.myungwolData);
        ProfileSowol(this.sowolData);
    }
    public void ProfileMyungwol(PlayerData data)
    {
        this.imgMyungwol.sprite = data.icon;
        this.txtMyungwolLike.text = data.like;
        this.txtMyungwolDislike.text = data.dislike;
        this.txtMyungwolSkill.text = data.skill;
    }
    public void ProfileSowol(PlayerData data)
    {
        this.imgSowol.sprite = data.icon;
        this.txtSowolLike.text = data.like;
        this.txtSowolDislike.text = data.dislike;
        this.txtSowolSkill.text = data.skill;
    }

    public override void ResetSetting() { }
}
