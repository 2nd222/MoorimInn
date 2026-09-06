using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogItem : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private LayoutElement layoutElement;

    [Header("Mode Root")]
    [SerializeField] private RectTransform headerRoot;
    [SerializeField] private RectTransform dialogueRoot;
    
    [Header("일반 대사")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text contentText;
    [SerializeField] private GameObject barImage;

    [Header("헤더")]
    [SerializeField] private GameObject headerUI;
    [SerializeField] private TMP_Text headerText;
    
    public void Setup(DialogueLog log)
    {
        // 헤더 로그
        if (log.logType == LogType.Header)
        {
            headerUI.SetActive(true);
            dialogueUI.SetActive(false);

            headerText.text = $"{log.day}일 - {log.title}";
            headerText.color = Color.black;

            StartCoroutine(RefreshLayout(headerRoot));
        }
        else
        {
            headerUI.SetActive(false);
            dialogueUI.SetActive(true);
            
            if (string.IsNullOrEmpty(log.speaker))
            {
                speakerText.text = "";
            }
            else
            {
                speakerText.text = log.isInnerThought
                    ? $"{log.speaker} (속마음) :"
                    : $"{log.speaker} :";
            }
            
            contentText.text = log.content;

            if (log.isNarration)
            {
                speakerText.gameObject.SetActive(false);
                barImage.SetActive(false);
                contentText.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                barImage.SetActive(true);
                speakerText.gameObject.SetActive(true);
                speakerText.alignment = TextAlignmentOptions.MidlineLeft;
                contentText.alignment = TextAlignmentOptions.TopLeft;
            }

            if (log.isInnerThought)
            {
                speakerText.color = Color.gray;
                contentText.color = Color.gray;
            }
            else
            {
                speakerText.color = Color.black;
                contentText.color = Color.black;
            }

            StartCoroutine(RefreshLayout(dialogueRoot));
        }
    }
    
    private IEnumerator RefreshLayout(RectTransform targetRoot)
    {
        // Layout 계산 한 프레임 대기
        yield return null;

        LayoutRebuilder.ForceRebuildLayoutImmediate(targetRoot);

        float height = LayoutUtility.GetPreferredHeight(targetRoot);

        layoutElement.preferredHeight = height;
    }
}
