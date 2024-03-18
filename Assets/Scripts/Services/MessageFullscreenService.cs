using UnityEngine;
using System;

public class MessageFullscreenService : MonoBehaviour
{
    [SerializeField] private MessageFullscreenView _view;

    public static class Title
    {
        public const string WaitingForResponse = "Waiting for response";
        public const string Loading = "Loading";
        public const string None = "";
    }


    public void Show(string text = "", string title = "", Action onCancel = null)
    {
        _view.SetText(text);
        _view.SetTitle(title);

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