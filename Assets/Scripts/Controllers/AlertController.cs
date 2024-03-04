using UnityEngine;

public class AlertController : MonoBehaviour
{
    [SerializeField] private AlertView _view;


    public void OnClickButtonOk()
    {
        _view.Hide();
    }

    public void Show(string text)
    {
        _view.SetText(text);
        _view.Show();
    }

    public void Hide()
    {
        _view.Hide();
    }
}
