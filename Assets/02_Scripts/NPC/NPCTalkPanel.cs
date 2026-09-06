using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCTalkPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image npcImage;
    [SerializeField] private float textSpeed = 0.02f;
    
    public void ChangeText(string text)
    {
        if (!this.isActiveAndEnabled)
            return;
        
        StopAllCoroutines();
        this.text.text = "";
        StartCoroutine(TempTextCoroutine(text));
    }

    private IEnumerator TempTextCoroutine(string message)
    {
        int count = 0;
        
        foreach (char c in message)
        {
            this.text.text += c;
            count++;
            if (!char.IsPunctuation(c) && !char.IsWhiteSpace(c) && count % 2 == 0)
            {
                SoundManager.Instance.PlayTypeSound();
            }
            yield return new WaitForSeconds(textSpeed);
        }
    }
    
    public void Clear()
    {
        StopAllCoroutines();
        text.text = "";
    }
}
