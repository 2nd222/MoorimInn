using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogPopupController : PopupController
{
    [SerializeField] TextMeshProUGUI _characterNameText;
    [SerializeField] TextMeshProUGUI _dialogText;
    [SerializeField] private float textSpeed = 0.2f;
    private Button _button;

    private string _currentFullDialog;
    public void Show(string characterName, string dialog)
    {
        base.Show();   
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnPopupClicked);
        _currentFullDialog = dialog;
        _characterNameText.text = characterName;
        _dialogText.text = "";
        StartCoroutine(ShowDialog(dialog));
    }

    // 이 패널(버튼)을 클릭 시 전부 보여주기
    private void OnPopupClicked()
    {
        StopCoroutine(nameof(ShowDialog));
        _dialogText.text = _currentFullDialog;
    }

    public void OnClickConfirmButton()
    {
        Hide();
    }

    public void OnClickCancelButton()
    {
        Hide();
    }

    IEnumerator ShowDialog(string dialog)
    {
        foreach (char c in dialog)
        {
            _dialogText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }
}
