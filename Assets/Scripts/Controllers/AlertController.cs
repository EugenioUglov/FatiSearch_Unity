using System;
using UnityEngine;

public class AlertController : MonoBehaviour
{
    [SerializeField] private AlertView _view;

    private Action _onClickButtonOKHandler = null;


    public void OnClickButtonOk()
    {
        _view.Hide();
        _onClickButtonOKHandler?.Invoke();
    }

    public void Show(string text, Action onClickButtonOK = null)
    {
        _onClickButtonOKHandler = onClickButtonOK;
        
        _view.SetText(text);
        _view.Show();
    }

    public void Hide()
    {
        _view.Hide();
    }
}
