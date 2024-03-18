using System;
using TMPro;
using UnityEngine;

public class MessageFullscreenView : MonoBehaviour
{
    [SerializeField] private GameObject _messageFullscreenGO;
    [SerializeField] private TextMeshProUGUI _titleComponent;
    [SerializeField] private TextMeshProUGUI _textComponent;
    [SerializeField] private GameObject _cancelButton;
    
    private Action _onClickButtonCancel = null;


    public void SetTitle(string newTitle)
    {
        _titleComponent.text = newTitle;
    }

    public void SetText(string newText)
    {
        _textComponent.text = newText;
    }

    public void SetCancelHandler(Action newCancelHandler)
    {
        _onClickButtonCancel = newCancelHandler;
    }

    public void Show()
    {
        _messageFullscreenGO.SetActive(true);
    }

    public void Hide()
    {
        _messageFullscreenGO.SetActive(false);
    }

    public void OnClickButtonCancel()
    {
        _onClickButtonCancel?.Invoke();
    }

    public void ShowCancelButton()
    {
        _cancelButton.SetActive(true);
    }

    public void HideCancelButton()
    {
        _cancelButton.SetActive(false);
    }
}
