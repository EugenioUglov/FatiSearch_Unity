using UnityEngine;
using System.Collections;
using Controllers;

public class Main : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private ActionBlockController _actionBlockController;
    [SerializeField] private DragAndDropController _dragAndDropController;
    [SerializeField] private SearchController _searchController;
    [SerializeField] private CommandController _commandController;
    [SerializeField] private LoaderFullscreenService _loaderFullscreenService;


    private void Start()
    {
        _loaderFullscreenService.Show(text: "Preparing all Action-Blocks");
        

        StartCoroutine(StartWithNoFreeze());

        IEnumerator StartWithNoFreeze()
        {
            yield return null;

            UserSettings userSettings = new UserSettings();
            userSettings.ApplySettings();
            _dragAndDropController.Init();
            _searchController.Init();
            _actionBlockController.Init();
        }
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