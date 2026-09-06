using UnityEngine;
using UnityEngine.UI;

public abstract class BaseTab : MonoBehaviour
{
    public void ShowTab(bool isShow) => this.gameObject.SetActive(isShow);

    public abstract void SetupData();
    public abstract void ResetSetting();
}
