using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using TMPro;
using UnityEngine;
using Unity.VisualScripting;


public class ActionBlockController : MonoBehaviour
{
    [Header("Links to scripts")]
    [SerializeField] private ActionBlockModel _model;
    [SerializeField] private ActionBlockView _view;

    [SerializeField] private ActionBlockSettingsController _actionBlockSettingsController;
    [SerializeField] private SearchController _searchController;
    [SerializeField] private AlertController _alertController;
    [SerializeField] private BottomMessageController _bottomMessageController;
    [SerializeField] private PageService _pageService;
    [SerializeField] private FileManager _fileManager;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _centralLogText;
    
    
    private HashSet<ActionBlockModel.ActionBlock> _actionBlocksToShow;
    private int _maxCountActionBlocksToShowAtTime = 10;
    private int _countShowedActionBlocks = 0;
    
    private bool _isMouseButtonLeftDown = false;

    
    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            _isMouseButtonLeftDown = true;
        }
        else
        {
            _isMouseButtonLeftDown = false;
        }
    }
    
    public void Init()
    {
        EventAggregator.AddListener<ActionBlockClickedEvent>(this, OnActionBlockClicked);
        EventAggregator.AddListener<ActionBlockSettingsClickedEvent>(this, OnActionBlockSettingsClicked);
        EventAggregator.AddListener<SearchEnteredEvent>(this, OnSearchEntered);
        EventAggregator.AddListener<ValueChangedInInputFieldSearchEvent>(this, OnValueChangedInInputFieldSearch);

        _actionBlocksToShow = new HashSet<ActionBlockModel.ActionBlock>();
        ActionBlockModel.ActionBlock[] actionBlocksFromFile = GetActionBlocksFromFile();
        _view.BindScrollbarValueChange(OnScrollbarValueChange);
        
        SetActionBlocks(actionBlocksFromFile);
        HashSet<ActionBlockModel.ActionBlock> actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
        SetActionBlocksToShow(actionBlocksToShow);
        RefreshActionBlocksOnPage();

        // string[] directoriesForAutoCreationActionBlocks = _model.GetDirectoriesForAutoCreationActionBlocksFromFile();

        // foreach (string currentDirectory in directoriesForAutoCreationActionBlocks)
        // {
        //     print("Auto directory: " + currentDirectory);
        //     CreateActionBlocksFromFolderIncludingSubfolders(directory: currentDirectory);
        // }
        // CreateActionBlocksFromFolderIncludingSubfolders(directory: @"D:\Fun\Games\0 Shortcuts\Feels");
        
    }

    private void CreateActionBlocksFromFolderIncludingSubfolders(string directory)
    {
        string[] fileDirectories = _fileManager.GetFileDirectoriesFromFolderWithSubfolders(directory);

        foreach (string currentDirectory in fileDirectories)
        {
            CreateActionBlockByPath(path: currentDirectory, isShowError: false);
        }
    }

    public void OnStartLoadingActionBlocksToShow()
    {
        HideSettingsForActionBlocks();
        _centralLogText.text = "Loading...";
    }
    
    public void OnActionBlocksShowed(string countActionBlocks)
    {
        _centralLogText.text = "";
    }

    public ActionBlockModel.ActionBlock[] GetActionBlocksFromFile()
    {
        return _model.GetActionBlocksFromFile();
    }

    public void SetActionBlocks(ActionBlockModel.ActionBlock[] actionBlocks)
    {
        _model.SetActionBlocks(actionBlocks);
    }
    
    public ActionBlockModel.ActionBlock GetActionBlockByTitle(string title)
    {
        return _model.GetActionBlockByTitle(title);
    }
    
    public void RefreshActionBlocksOnPage()
    {
        if (_actionBlocksToShow.Count <= _countShowedActionBlocks)
        {
            _searchController.ShowPage();

            return;
        }
        
        ActionBlockModel.ActionBlock[] actionBlocksToShowArray = _actionBlocksToShow.ToArray();
        int countShowedAtTime = 0;
        
        _searchController.HidePage();
        OnStartLoadingActionBlocksToShow();

        for (var i = 0; i < actionBlocksToShowArray.Length; i++)
        {
            if (_actionBlocksToShow.Count <= _countShowedActionBlocks || countShowedAtTime >= _maxCountActionBlocksToShowAtTime)
            {
                break;
            }

            ActionBlockModel.ActionBlock actionBlock = actionBlocksToShowArray[_countShowedActionBlocks];
            _view.AddActionBlock(actionBlock);
            _countShowedActionBlocks++;
            countShowedAtTime++;
        }

        _searchController.ShowPage();
        OnActionBlocksShowed(_actionBlocksToShow.Count.ToString());
    }

    public void HideSettingsForActionBlocks()
    {
        _actionBlockSettingsController.HidePage();
    }

    public bool CreateActionBlock(ActionBlockModel.ActionBlock actionBlock, bool isShowError = true)
    {
        bool isCreated = _model.CreateActionBlock(actionBlock, isShowError);
        
        RefreshView();
        
        return isCreated;
    }

    public bool CreateActionBlockByPath(string path, bool isShowError = true)
    {
        string fileName = Path.GetFileNameWithoutExtension(path);
        String[] foldersOfPath = path.Split('\\');
        List<string> tags = new List<string>();
                
        for (int i = 0; i < foldersOfPath.Length - 1; i++)
        {
            // Add to tags folder names from path of a file.
            
            tags.Add(foldersOfPath[i]);
        }

        ActionBlockModel.ActionBlock actionBlock = 
        new ActionBlockModel.ActionBlock(fileName, ActionBlockModel.ActionEnum.OpenPath, 
            path, tags);
        
        CreateActionBlock(actionBlock, isShowError);
        SetActionBlocksToShow();
        RefreshActionBlocksOnPage();

        return true;
    }

    
    public bool UpdateActionBlock(string title, ActionBlockModel.ActionBlock actionBlock)
    {
        bool isUpdated = _model.UpdateActionBlock(title, actionBlock);
        if (isUpdated == false) return isUpdated;
        
        RefreshView();
        
        return isUpdated;
    }
    
    public void DeleteActionBlock(ActionBlockModel.ActionBlock actionBlock)
    {
        _model.DeleteActionBlock(actionBlock);
        HashSet<ActionBlockModel.ActionBlock> actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
        SetActionBlocksToShow(actionBlocksToShow);
        RefreshActionBlocksOnPage();
    }

    public bool ExecuteByTitle(string title)
    {
        ActionBlockModel.ActionBlock actionBlock = GetActionBlockByTitle(title);

        if (actionBlock.Title != null)
        {
            ExecuteByActionBlock(actionBlock);
            
            return true;
        }

        return false;
    }

    public void OnClickButtonCreate()
    {
        _searchController.HidePage();
        _actionBlockSettingsController.ShowSettingsToCreateActionBlock();
    }

    public void SetActionBlocksToShow(HashSet<ActionBlockModel.ActionBlock> newActionBlocksToShow = null)
    {
        if (newActionBlocksToShow == null)
        {
            newActionBlocksToShow = _model.GetActionBlocks().ToHashSet();
        }
        
        _view.ClearActionBlocks();
        
        _view.ShowCountTextFoundActionBlocks(newActionBlocksToShow.Count);
        
        _countShowedActionBlocks = 0;
        _actionBlocksToShow = newActionBlocksToShow;
    }
    
    
    private void OnActionBlockClicked(ActionBlockClickedEvent actionBlockClickedEvent)
    {
        string titleActionBlock = actionBlockClickedEvent.Title;
        ExecuteByTitle(titleActionBlock);
    }

    private void OnActionBlockSettingsClicked(ActionBlockSettingsClickedEvent actionBlockSettingsClickedEvent)
    {
        string titleActionBlock = actionBlockSettingsClickedEvent.Title;
        ActionBlockModel.ActionBlock actionBlock = GetActionBlockByTitle(titleActionBlock);
        _actionBlockSettingsController.ShowSettingsToUpdateActionBlock(actionBlock);
    }

    private void OnSearchEntered(SearchEnteredEvent searchEnteredEvent)
    {
        string userRequest = searchEnteredEvent.Request;
        HashSet<ActionBlockModel.ActionBlock> actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
        
        if (userRequest == "")
        {
            actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
        }
        else
        {
            bool isExecutedByTitle = ExecuteByTitle(userRequest);
            actionBlocksToShow = _model.GetActionBlocksByRequest(userRequest).ToHashSet();
        }
        
        SetActionBlocksToShow(actionBlocksToShow);
        RefreshActionBlocksOnPage();
    }

    private void OnValueChangedInInputFieldSearch(ValueChangedInInputFieldSearchEvent valueChangedInInputFieldSearchEvent)
    {
        string userRequest = valueChangedInInputFieldSearchEvent.Request;
        HashSet<ActionBlockModel.ActionBlock> actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
        
        if (userRequest == "")
        {
            actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
        }
        else
        {
            actionBlocksToShow = _model.GetActionBlocksByRequest(userRequest).ToHashSet();
        }
        
        SetActionBlocksToShow(actionBlocksToShow);
        RefreshActionBlocksOnPage();
    }
    
    private void ExecuteByActionBlock(ActionBlockModel.ActionBlock actionBlock)
    {
        if (actionBlock.Action == ActionBlockModel.ActionEnum.OpenPath)
        {
            bool isOpened = _fileManager.OpenDirectory(actionBlock.Content);
            
            if (isOpened)
            {
                _bottomMessageController.Show("Execution \"" + actionBlock.Title + "\"");
            }
            else
            {
                _alertController.Show("Not possible to execute Action-Block " + actionBlock.Title);
            }
        }
        else if (actionBlock.Action == ActionBlockModel.ActionEnum.OpenPathAsAdministrator)
        {
            bool isOpened = _fileManager.OpenDirectoryAsAdministrator(actionBlock.Content);
            
            if (isOpened)
            {
                _bottomMessageController.Show("Execution \"" + actionBlock.Title + "\"");
            }
            else
            {
                _alertController.Show("Not possible to execute Action-Block " + actionBlock.Title);
            }
        }
        else if (actionBlock.Action == ActionBlockModel.ActionEnum.SelectPath) 
        {
            _fileManager.SelectDirectory(actionBlock.Content);
        }
    }

    private void RefreshView()
    {
        _searchController.ClearInputField();
        _searchController.ShowPage();
        HashSet<ActionBlockModel.ActionBlock> actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
        SetActionBlocksToShow(actionBlocksToShow);
        RefreshActionBlocksOnPage();
        _view.SetDefaultSettingsFields();
    }
    
    private void OnScrollbarValueChange(float value)
    {
        if (value <= 0.2f)
        {
            if (_actionBlocksToShow.Count <= _countShowedActionBlocks)
            {
                return;
            }
            
            bool isMouseButtonLeftDownOnStartRefreshActionBlocks = _isMouseButtonLeftDown;


            _view.BlockScrollCapability();
            StartCoroutine(RefreshActionBlocksAfterPause(0.1f, OnRefreshed));

            void OnRefreshed()
            {            
                if (isMouseButtonLeftDownOnStartRefreshActionBlocks)
                {
                    StartCoroutine(WaitForMouseButtonLeftUp(OnMouseButtonLeftUp));

                    void OnMouseButtonLeftUp()
                    {
                        _view.UnblockScrollCapability();
                    }
                }
                else
                {
                    _view.UnblockScrollCapability();
                }
            }
        }
    }
    
    private IEnumerator WaitForMouseButtonLeftUp(Action callbackMouseButtonLeftUp)
    {
        while (_isMouseButtonLeftDown)
        {
            yield return null;
        }
        
        callbackMouseButtonLeftUp.Invoke();
    }
    
    private IEnumerator RefreshActionBlocksAfterPause(float pauseSec, Action callbackEnd = null)
    {
        yield return new WaitForSeconds(pauseSec);
        
        RefreshActionBlocksOnPage();
        
        callbackEnd?.Invoke();
    }
}