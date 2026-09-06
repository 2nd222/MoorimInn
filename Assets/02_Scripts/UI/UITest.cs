using UnityEngine;
using UnityEngine.UI;

public class UITest : MonoBehaviour
{
    [SerializeField]
    private RectTransform rt;
    [SerializeField]
    private Button btnShowPop;
    [SerializeField]
    private Button btnHidePop;
    [SerializeField]
    private Button btnShowOrder;
    [SerializeField]
    private Button btnHideOrder;
    [SerializeField]
    private Button btnShowVerticalUnfold;
    [SerializeField]
    private Button btnHideVerticalUnfold;
    [SerializeField]
    private Button btnShowHorizontalUnfold;
    [SerializeField]
    private Button btnHideHorizontalUnfold;
    [SerializeField]
    private Button btnShowPaper;
    [SerializeField]
    private Button btnHidePaper;
    [SerializeField]
    private Button btnShowBounce;
    [SerializeField]
    private Button btnHideBounce;
    [SerializeField]
    private Button btnShowZigzag;
    [SerializeField]
    private Button btnHideZigzag;
    [SerializeField]
    private Button btnShowStamp;

    void Start()
    {
        UIAnimationManager ui = UIAnimationManager.Instance;
        this.btnShowPop.onClick.AddListener(() => ui.ShowPop(this.rt));
        this.btnHidePop.onClick.AddListener(() => ui.HidePop(this.rt));

        this.btnShowOrder.onClick.AddListener(() => ui.ShowOrder(this.rt));
        this.btnHideOrder.onClick.AddListener(() => ui.HideOrder(this.rt));

        this.btnShowVerticalUnfold.onClick.AddListener(() => ui.ShowVerticalUnfold(this.rt));
        this.btnHideVerticalUnfold.onClick.AddListener(() => ui.HideVerticalUnfold(this.rt));

        this.btnShowHorizontalUnfold.onClick.AddListener(() => ui.ShowHorizontalUnfold(this.rt));
        this.btnHideHorizontalUnfold.onClick.AddListener(() => ui.HideHorizontalUnfold(this.rt));

        this.btnShowPaper.onClick.AddListener(() => ui.ShowPaper(this.rt));
        this.btnHidePaper.onClick.AddListener(() => ui.HidePaper(this.rt));

        this.btnShowBounce.onClick.AddListener(() => ui.ShowBounce(this.rt));
        this.btnHideBounce.onClick.AddListener(() => ui.HideBounce(this.rt));

        this.btnShowZigzag.onClick.AddListener(() => ui.ShowZigzag(this.rt));
        this.btnHideZigzag.onClick.AddListener(() => ui.HideZigzag(this.rt));

        this.btnShowStamp.onClick.AddListener(() => ui.ShowStamp(this.rt));
    }
}
