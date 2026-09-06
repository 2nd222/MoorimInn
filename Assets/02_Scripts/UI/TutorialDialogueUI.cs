using System.Collections;
using TMPro;
using UnityEngine;

public class TutorialDialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject root;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Typing")]
    [SerializeField] private float typingSpeed = 0.03f;

    private Coroutine typingCoroutine;
    private bool isTyping;

    public bool IsTyping => isTyping;

    /*
    public void Show(string speakerName, string line)
    {
        root.SetActive(true);

        nameText.text = speakerName;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeRoutine(line));
    }
    */
    
    public void Show(string speakerName, string line)
    {
        Debug.Log($"[TutorialDialogueUI] Show 호출됨. 화자: {speakerName}, 대사: {line}");

        if (root == null) 
        { 
            Debug.LogError("[TutorialDialogueUI] 🚨 root(대화창 패널)가 연결되지 않았습니다!"); 
            return; 
        }

        root.SetActive(true);
        Debug.Log("[TutorialDialogueUI] root(대화창 패널) SetActive(true) 완료");

        if (nameText == null) 
        { 
            Debug.LogError("[TutorialDialogueUI] 🚨 nameText가 연결되지 않았습니다!"); 
            return; 
        }
        nameText.text = speakerName;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        Debug.Log("[TutorialDialogueUI] TypeRoutine 코루틴 시작 직전");
        typingCoroutine = StartCoroutine(TypeRoutine(line));
    }

    public void Hide()
    {
        root.SetActive(false);

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
    }

    public void SkipTyping(string fullText)
    {
        if (!isTyping)
            return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.text = fullText;
        isTyping = false;
    }

    private IEnumerator TypeRoutine(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        int count = 0;
        
        foreach (char c in line)
        {
            dialogueText.text += c;
            count++;
            
            if (count % 2 == 0 && !char.IsPunctuation(c) && !char.IsWhiteSpace(c))
            {
                SoundManager.Instance.PlayTypeSound();
            }
            
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;
    }
}
