using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class LoaderFullscreenView : MonoBehaviour
{
    [SerializeField] private GameObject _loaderFullscreenGO;
    [SerializeField] private TextMeshProUGUI _textComponent;
    [SerializeField] private GameObject _cancelButton;
    
    private Action _onClickButtonCancel = null;


    public void SetText(string newText)
    {
        _textComponent.text = newText;
    }

    public void SetCancelHandler(Action newCancelHandler)
    {
        _onClickButtonCancel = newCancelHandler;
    }

    public void ShowDuringActionInProgress(Action actionDuringLoading)
    { 
        StartCoroutine(ShowLoadingWhileActionInProgress());
        
        IEnumerator ShowLoadingWhileActionInProgress()
        {
            Show();
            yield return null;
            actionDuringLoading();
            Hide();
        }
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

    public void ShowCancelButton()
    {
        _cancelButton.SetActive(true);
    }

    public void HideCancelButton()
    {
        _cancelButton.SetActive(false);
    }
}
