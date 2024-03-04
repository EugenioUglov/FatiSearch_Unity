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

    public void Show(Action onCancel = null)
    {
        print("Show");
        _isToShow = true;

        if (onCancel != null) {
            _view.ShowCancelButton();
            OnCancel(onCancel);
        }
        else
        {
            _view.HideCancelButton();
        }

        StartCoroutine(ShowLoaderullscreen());

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
        print("Hide");
        _isToShow = false;

        StartCoroutine(HideLoaderullscreen());

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
