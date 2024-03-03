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
        _loaderFullscreenService.Show();
        // CopyFileKeepingFolders(@"D:\Fun\Games my data\0 Shortcuts\Play web games in browser.txt", @"D:\Test");

        UserSettings userSettings = new UserSettings();
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