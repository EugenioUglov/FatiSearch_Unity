using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandController : MonoBehaviour
{
    [SerializeField] private CommandView _view;
    [SerializeField] private PageService _pageService;


    private void Awake()
    {
        EventAggregator.AddListener<KeyClickedEvent>(this, OnKeyDown);
    }

    public bool IsSelectedInputField()
    {
        return _view.IsSelected();
    }

    public void OnClickEnterButton()
    {
        OnEnterInputField();
    }

    public void ClearInputField()
    {
        _view.ClearInputField();
    }

    private void OnKeyDown(KeyClickedEvent keyClickedEvent)
    {
        if (_pageService.PageState == PageService.PageStateEnum.SearchPage && _view.IsSelected())
        {
            if (keyClickedEvent.KeyCodeEntered == KeyCode.Return)
            {
                OnEnterInputField();
            }
        }
    }

    private void OnEnterInputField()
    {
        CommandEnteredEvent commandEnteredEvent = new CommandEnteredEvent();

        string userRequest = _view.GetTextFromInputField().ToLower();
        commandEnteredEvent.Request = userRequest;
        print(userRequest);

        EventAggregator.Invoke<CommandEnteredEvent>(commandEnteredEvent);


        _view.OnEnterInputField();
    }
}
