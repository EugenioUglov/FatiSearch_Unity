using System;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Controllers;
using TMPro;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;


public class ActionBlockController : MonoBehaviour
{
    [Header("Links to scripts")]
    [SerializeField] private ActionBlockModel _model;
    [SerializeField] private ActionBlockView _view;

    [SerializeField] private ActionBlockModifierController _actionBlockModifierController;
    [SerializeField] private ActionBlockCreatorController _actionBlockCreatorController;
    [SerializeField] private SearchController _searchController;
    [SerializeField] private AlertController _alertController;
    [SerializeField] private BottomMessageController _bottomMessageController;
    [SerializeField] private PageService _pageService;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _centralLogText;
    
    
    private HashSet<ActionBlockModel.ActionBlock> _actionBlocksToShow;
    private int _maxCountActionBlocksToShowAtTime = 10;
    private int _countShowedActionBlocks = 0;
    
    private float _startScrollValue = 1;
    private float _endScrollValue = 0;
    
    private bool _isMouseButtonLeftDown = false;
    
    
    public void Init()
    {
        EventAggregator.AddListener<ActionBlockClickedEvent>(this, OnActionBlockClicked);
        EventAggregator.AddListener<ActionBlockSettingsClickedEvent>(this, OnActionBlockSettingsClicked);
        EventAggregator.AddListener<SearchEnteredEvent>(this, OnSearchEntered);

        _actionBlocksToShow = new HashSet<ActionBlockModel.ActionBlock>();
        ActionBlockModel.ActionBlock[] actionBlocksFromFile = GetActionBlocksFromFile();
        _view.BindScrollbarValueChange(OnScrollbarValueChange);
        
        SetActionBlocks(actionBlocksFromFile);
        HashSet<ActionBlockModel.ActionBlock> actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
        SetActionBlocksToShow(actionBlocksToShow);
        RefreshActionBlocksOnPage();
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
        //_scrollbar.interactable = true;
    }

    public void ShowSettingsForActionBlock()
    {
        _actionBlockCreatorController.ShowSettingsForActionBlock();
    }

    public void HideSettingsForActionBlocks()
    {
        _actionBlockCreatorController.HidePage();
    }

    public bool CreateActionBlock(ActionBlockModel.ActionBlock actionBlock)
    {
        bool isCreated = _model.CreateActionBlock(actionBlock);
        return isCreated;
    }
    
    public bool UpdateActionBlock(string title, ActionBlockModel.ActionBlock actionBlock)
    {
        return _model.UpdateActionBlock(title, actionBlock);
    }

    public void DeleteActionBlock(ActionBlockModel.ActionBlock actionBlock)
    {
        _model.DeleteActionBlock(actionBlock);
        HashSet<ActionBlockModel.ActionBlock> actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
        SetActionBlocksToShow(actionBlocksToShow);
        RefreshActionBlocksOnPage();
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
            
            print("Add more");
        }
    }
    
    IEnumerator WaitForMouseButtonLeftUp(Action callbackMouseButtonLeftUp)
    {
        while (_isMouseButtonLeftDown)
        {
            yield return null;
        }
        
        callbackMouseButtonLeftUp.Invoke();
    }
    
    IEnumerator RefreshActionBlocksAfterPause(float pauseSec, Action callbackEnd = null)
    {
        yield return new WaitForSeconds(pauseSec);
        
        RefreshActionBlocksOnPage();
        
        callbackEnd?.Invoke();
    }

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

        if (Input.GetMouseButton(1))
        {
            Debug.Log("Right Mouse Button Down");
        }

        if (Input.GetMouseButton(2))
        {
            Debug.Log("Middle Mouse Button Down");
        }
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

    public void OnClickButtonSaveNewSettingsActionBlock()
    {
        string title = _view.TitleInputField.GetComponent<TMP_InputField>().text;
        string action = ActionBlockModel.ActionEnum.OpenPath;
        //ActionBlockModel.ActionEnum action = _actionDropdown.GetComponent<Dropdown>().value.ToString();
        string content = _view.ContentInputField.GetComponent<TMP_InputField>().text;
        string tagsTextFromInputField = _view.TagsInputField.GetComponent<TMP_InputField>().text;
        List<string> tagsList = new List<string>();

        if (string.IsNullOrEmpty(tagsTextFromInputField) == false)
        {
            string[] tags_to_add = tagsTextFromInputField.Split(',');

            for (int i_tag = 0; i_tag < tags_to_add.Length; i_tag++)
            {
                // Delete empty spaces from sides of tags.
                string tagToAdd = tags_to_add[i_tag].Trim();
                
                tagsList.Add(tagToAdd);
            }
        }

        string imagePath = _view.ImagePathInputField.GetComponent<TMP_InputField>().text;
        ActionBlockModel.ActionBlock actionBlock = new ActionBlockModel.ActionBlock(title, action, content, tagsList, imagePath);

        if (_pageService.PageState == PageService.PageStateEnum.ActionBlockCreator)
        {
            bool isCreated = CreateActionBlock(actionBlock);
            if (isCreated == false) return;
        }
        else if (_pageService.PageState == PageService.PageStateEnum.ActionBlockModifier)
        {
            bool isUpdated = UpdateActionBlock(_actionBlockModifierController.OriginalTitle, actionBlock);
            if (isUpdated == false) return;
                
            _actionBlockModifierController.HideDeleteButton();
        }
        
        
        _searchController.ClearInputField();
        _actionBlockCreatorController.HidePage();
        _searchController.ShowPage();
        HashSet<ActionBlockModel.ActionBlock> actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
        SetActionBlocksToShow(actionBlocksToShow);
        RefreshActionBlocksOnPage();
        _view.SetDefaultSettingsFields();
    }

    public void SetActionBlocksToShow(HashSet<ActionBlockModel.ActionBlock> newActionBlocksToShow = null)
    {
        if (newActionBlocksToShow == null)
        {
            newActionBlocksToShow = _model.GetActionBlocks().ToHashSet();
        }
        
        _view.ShowCountTextFoundActionBlocks(newActionBlocksToShow.Count);
        
        _countShowedActionBlocks = 0;
        _view.ClearActionBlocks();
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
        _actionBlockModifierController.ShowSettingsToUpdateActionBlock(actionBlock);
    }

    private void OnSearchEntered(SearchEnteredEvent searchEnteredEvent)
    {
        string userRequest = searchEnteredEvent.Request;
        //HashSet<ActionBlockModel.ActionBlock> actionBlocksToShow;
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
    
    private void ExecuteByActionBlock(ActionBlockModel.ActionBlock actionBlock)
    {
        if (actionBlock.Action == ActionBlockModel.ActionEnum.OpenPath)
        {
            bool isOpened = OpenPath(actionBlock.Content);
            
            //if (isOpened)
            {
                _bottomMessageController.Show("Execution \"" + actionBlock.Title + "\"");
            }
        }
        else if (actionBlock.Action == ActionBlockModel.ActionEnum.SelectPath) 
        {
            SelectPath(actionBlock.Content);
        }
    }

    private bool OpenPath(string path)
    {
        bool isOpened = false;
        
        try
        {
            Process.Start(path);
            isOpened = true;
        }
        catch (Exception exception)
        {
            _alertController.Show("Not possible to execute Action-Block");
        }

        return isOpened;
    }

    private void SelectPath(string path)
    {
        if (File.Exists(path))
        {
            Process.Start(new ProcessStartInfo("explorer.exe", " /select, " + path));
        }
        else {
            print("Path doesn't exist: " + path);
        }
    }

}
