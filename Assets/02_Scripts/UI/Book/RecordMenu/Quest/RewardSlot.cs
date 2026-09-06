using UnityEngine;
using UnityEngine.UI;

public class RewardSlot : MonoBehaviour
{
    [SerializeField] private Image imgIcon;

    public void SetIcon(Sprite icon)
    {
        this.imgIcon.sprite = icon;
    }
}
