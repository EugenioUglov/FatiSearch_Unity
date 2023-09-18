using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoaderFullscreenView : MonoBehaviour
{
    [SerializeField] private GameObject _loaderFullscreenGO;
    [SerializeField] private TextMeshProUGUI _textComponent;
    
    private Action _onClickButtonCancel = null;


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
        _loaderFullscreenGO.SetActive(true);
    }

    public void Hide()
    {
        _loaderFullscreenGO.SetActive(false);
    }

    public void OnClickButtonCancel()
    {
        _onClickButtonCancel?.Invoke();
    }
}
