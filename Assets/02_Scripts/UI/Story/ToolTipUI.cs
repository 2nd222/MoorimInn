using System;
using TMPro;
using UnityEngine;

public class ToolTipUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI description;
    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        StoryManager.Instance.SetTooltipUI(this);
    }

    public void Show(string text, RectTransform target)
    {
        description.text = text;

        Vector3 pos = target.position;
        pos.x -= 150f;   
        pos.y -= 20f;

        rect.position = pos;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
