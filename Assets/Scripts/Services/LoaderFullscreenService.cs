using UnityEngine;
using System;

public class LoaderFullscreenService : MonoBehaviour
{
    [SerializeField] private LoaderFullscreenView _view;
    

    public void SetText(string newText)
    {
        _view.SetText(newText);
    }

    public void Show(Action onCancel = null)
    {
        if (onCancel != null) {
            _view.ShowCancelButton();
            OnCancel(onCancel);
        }
        else
        {
            _view.HideCancelButton();
        }

        _view.Show();
    }

    public void Hide()
    {
        _view.Hide();
    }

    public void OnCancel(Action onCancel)
    {
        Hide();
        _view.SetCancelHandler(onCancel);
    }
}
