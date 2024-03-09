using UnityEngine;
using System;

public class LoaderFullscreenService : MonoBehaviour
{
    [SerializeField] private LoaderFullscreenView _view;


    public void Show(string text = "", Action onCancel = null)
    {
        _view.SetText(text);

        if (onCancel != null) {
            _view.ShowCancelButton();
            SetCancelHandler(onCancel);
        }
        else
        {
            _view.HideCancelButton();
        }

        _view.Show();
    }

    public void SetCancelHandler(Action onCancel) 
    {
        _view.SetCancelHandler(onCancel);
    }

    public void SetText(string newText)
    {
        _view.SetText(newText);
    }

    public void Hide()
    {
        _view.Hide();
    }
}
