using UnityEngine;
using System;
using System.Collections;

public class LoaderFullscreenService : MonoBehaviour
{
    [SerializeField] private LoaderFullscreenView _view;

    private bool _isToShow = false;
    

    public void SetText(string newText)
    {
        _view.SetText(newText);
    }

    public void ShowDuringActionInProgress(Action actionDuringLoading, Action onCancel = null)
    {
        if (onCancel != null) {
            _view.ShowCancelButton();
            OnCancel(onCancel);
        }
        else
        {
            _view.HideCancelButton();
        }
        
        _view.ShowDuringActionInProgress(actionDuringLoading);
    }

    public void Show(Action onCancel = null)
    {
        _isToShow = true;

        if (onCancel != null) {
            _view.ShowCancelButton();
            OnCancel(onCancel);
        }
        else
        {
            _view.HideCancelButton();
        }

        _view.Show();

        // StartCoroutine(ShowLoaderullscreen());

        IEnumerator ShowLoaderullscreen()
        {
            // yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(0.01f);

            if (_isToShow)
            {
                print("SHOW NOW");
                _view.Show();
            }
        }
    }

    public void Hide()
    {
        _isToShow = false;
        _view.Hide();

        // StartCoroutine(HideLoaderullscreen());

        IEnumerator HideLoaderullscreen()
        {
            // yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(1f);
            print(_isToShow);

            if (_isToShow == false)
            {
                print("HIDE NOW");
                _view.Hide();
            }
        }
    }

    public void OnCancel(Action onCancel)
    {
        Hide();
        _view.SetCancelHandler(onCancel);
    }
}
