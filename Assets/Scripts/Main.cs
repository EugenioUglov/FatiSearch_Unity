using Controllers;
using UnityEngine;

public class Main : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private ActionBlockController _actionBlockController;
    [SerializeField] private DragAndDropController _dragAndDropController;
    [SerializeField] private SearchController _searchController;
    [SerializeField] private CommandController _commandController;
    [SerializeField] LoaderFullscreenService _loaderFullscreenService;
    
    
    void Start()
    {
        UserSettings userSettings = new UserSettings();

        _loaderFullscreenService.Show();
        userSettings.ApplySettings();
        _dragAndDropController.Init();
        _searchController.Init();
        _actionBlockController.Init();
        _loaderFullscreenService.Hide();
    }

    // void Update()
    // {
    //     if (_searchController.IsSelectedInputField() == false && _commandController.IsSelectedInputField() == false)
    //     {
    //         _searchController.FocusInputField();
    //     }
    // }

    
    public void Quit()
    {
        UnityEngine.Application.Quit();
    }
}