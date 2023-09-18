using System.Collections;
using System.Collections.Generic;
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
        OnCancel(onCancel);
        _view.Show();
    }

    public void Hide()
    {
        _view.Hide();
    }

    public void OnCancel(Action onCancel)
    {
        _view.SetCancelHandler(onCancel);
    }
}
