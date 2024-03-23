using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using TMPro;
using UnityEngine;
using Unity.VisualScripting;
using System.Windows.Forms;

public class ActionBlockController : MonoBehaviour
{
    [Header("Links to scripts")]
    [SerializeField] private ActionBlockModel _model;
    [SerializeField] private ActionBlockView _view;
    [SerializeField] private ActionBlockService _service;

    [SerializeField] private ActionBlockSettingsController _actionBlockSettingsController;
    [SerializeField] private SearchController _searchController;
    [SerializeField] private AlertController _alertController;
    [SerializeField] private BottomMessageController _bottomMessageController;
    [SerializeField] private CommandController _commandController;

    private string _userRequest = "";


    private bool _isLoadingActionBlocks = false;
    private DirectoryManager _directoryManager;
    private DialogMessageService _dialogMessageService;


    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            _service.IsMouseButtonLeftDown = true;
        }
        else
        {
            _service.IsMouseButtonLeftDown = false;
        }
    }
    
    public void Init()
    {
        _directoryManager = new DirectoryManager();
        _dialogMessageService = new DialogMessageService();
        
        EventAggregator.AddListener<ActionBlockClickedEvent>(this, OnActionBlockClicked);
        EventAggregator.AddListener<ActionBlockSettingsClickedEvent>(this, OnActionBlockSettingsClicked);
        EventAggregator.AddListener<ActionBlockFileLocationClickedEvent>(this, OnActionBlockFileLocationClicked);
        EventAggregator.AddListener<SearchEnteredEvent>(this, OnSearchEntered);
        EventAggregator.AddListener<CommandEnteredEvent>(this, OnCommandEntered);
        EventAggregator.AddListener<ValueChangedInInputFieldSearchEvent>(this, OnValueChangedInInputFieldSearch);

        _view.BindScrollbarValueChange(OnScrollbarValueChange);


        // _service.GetActionBlocksToShow() = new HashSet<ActionBlockModel.ActionBlock>();

        // HashSet<ActionBlockModel.ActionBlock> actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
    }


    public void OnClickButtonCreate()
    {
        _searchController.HidePage();
        _actionBlockSettingsController.ShowSettingsToCreateActionBlock();
    }

    
    private void OnActionBlockClicked(ActionBlockClickedEvent actionBlockClickedEvent)
    {
        string titleActionBlock = actionBlockClickedEvent.Title;
        _service.ExecuteByTitle(titleActionBlock);
    }

    private void OnActionBlockSettingsClicked(ActionBlockSettingsClickedEvent actionBlockSettingsClickedEvent)
    {
        string titleActionBlock = actionBlockSettingsClickedEvent.Title;
        ActionBlockModel.ActionBlock actionBlock = _service.GetActionBlockByTitle(titleActionBlock);
        _actionBlockSettingsController.ShowSettingsToUpdateActionBlock(actionBlock);
    }

    private void OnActionBlockFileLocationClicked(ActionBlockFileLocationClickedEvent actionBlockFileLocationClickedEvent)
    {
        string titleActionBlock = actionBlockFileLocationClickedEvent.Title;
        ActionBlockModel.ActionBlock actionBlock = _service.GetActionBlockByTitle(titleActionBlock);
        _directoryManager.ShowInExplorer(path: actionBlock.Content);
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
            bool isExecutedByTitle = _service.ExecuteByTitle(userRequest);
            ActionBlockModel.ActionBlock actionBlockByTitle = _model.GetActionBlockByTitle(userRequest);

            HashSet<ActionBlockModel.ActionBlock> actionBlocksByRequest = _model.GetActionBlocksByRequest(userRequest).ToHashSet();

            if (string.IsNullOrEmpty(actionBlockByTitle.Title) == false)
            {
                actionBlocksToShow.Add(actionBlockByTitle);

                foreach (var actionBlock in actionBlocksByRequest)
                {
                    if (actionBlockByTitle.Title == actionBlock.Title) continue;
                    actionBlocksToShow.Add(actionBlock);
                }
            }
            else 
            {
                actionBlocksToShow = actionBlocksByRequest;
            }
        }
        
        _service.SetActionBlocksToShow(actionBlocksToShow);
        _service.RefreshActionBlocksOnPage();
    }

    private void OnCommandEntered(CommandEnteredEvent commandEnteredEvent)
    {
        string commandRequest = commandEnteredEvent.Request.ToLower();

        if (commandRequest == "execute all found results")
        {
            OnRequestExecuted();

            foreach (ActionBlockModel.ActionBlock actionBlock in _service.GetActionBlocksToShow())
            {
                _service.ExecuteByTitle(actionBlock.Title);
            }
        }
        else if (commandRequest == "update")
        {
            OnRequestExecuted();
            _service.CreateActionBlocksByDirectoriesAndShow();
        }
        else if (commandRequest.Contains("exact tags"))
        {
            OnRequestExecuted();

            string[] tagsFromRequest = _userRequest.Split(' ');
            HashSet<ActionBlockModel.ActionBlock> _actionBlocksByExactTagsToShow = new HashSet<ActionBlockModel.ActionBlock>();
            
            foreach (var actionBlock in _service.GetActionBlocksToShow())
            {
                // In all tags of request.
                foreach (var tagFromReqeust in tagsFromRequest)
                {
                    // In all tags of Action Block.
                    foreach (string tagFromActionBlock in actionBlock.Tags)
                    {
                        if (tagFromReqeust == tagFromActionBlock)
                        {
                            _actionBlocksByExactTagsToShow.Add(actionBlock);
                            break;
                        }
                    }
                }
            }

            _service.SetActionBlocksToShow(_actionBlocksByExactTagsToShow.ToHashSet());

            _service.RefreshActionBlocksOnPage();
            _view.DestroyLoadingText();

        }
        else if (commandRequest == "delete all action-blocks")
        {
            OnRequestExecuted();
            _model.DeleteAllActionBlocks();
            _service.RefreshView();
            _model.SaveToFile();
        }

        void OnRequestExecuted()
        {
            _commandController.ClearInputField();
        }
    }

    private void OnValueChangedInInputFieldSearch(ValueChangedInInputFieldSearchEvent valueChangedInInputFieldSearchEvent)
    {
        _model.StopCoroutineGetActionBlocksByRequestAsync();

        string userRequest = valueChangedInInputFieldSearchEvent.Request;
        _userRequest = userRequest;
 
        HashSet<ActionBlockModel.ActionBlock> actionBlocksToShow = new HashSet<ActionBlockModel.ActionBlock>();
        _view.ClearActionBlocks();
        _view.AddLoadingText();

        if (userRequest == "")
        {
            actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
            _service.SetActionBlocksToShow(actionBlocksToShow);
            _service.RefreshActionBlocksOnPage();
            _view.DestroyLoadingText();
        }
        else
        {
            ActionBlockModel.ActionBlock actionBlockByTitle = _model.GetActionBlockByTitle(userRequest);
            // Not async.
            // HashSet<ActionBlockModel.ActionBlock> actionBlocksByRequest = _model.GetActionBlocksByRequest(userRequest).ToHashSet();

            StartCoroutine(_model.GetActionBlocksByRequestAsync(
                request: userRequest,
                onGet: (actionBlocks) =>
                {
                    ShowActionBlocks(actionBlocks);
                }
            ));

            void ShowActionBlocks(ActionBlockModel.ActionBlock[] actionBlocks)
            {
                HashSet<ActionBlockModel.ActionBlock> actionBlocksByRequest = actionBlocks.ToHashSet();

                if (string.IsNullOrEmpty(actionBlockByTitle.Title) == false)
                {
                    actionBlocksToShow.Add(actionBlockByTitle);

                    foreach (var actionBlock in actionBlocksByRequest)
                    {
                        if (actionBlockByTitle.Title == actionBlock.Title) continue;
                        
                        actionBlocksToShow.Add(actionBlock);
                    }
                }
                else 
                {
                    actionBlocksToShow = actionBlocksByRequest;
                }
                
                _service.SetActionBlocksToShow(actionBlocksToShow);
                _service.RefreshActionBlocksOnPage();
                _view.DestroyLoadingText();
            }

            // StartCoroutine(_model.GetActionBlocksByRequestAsync(
            //     userRequest, 
            //     onGet: (actionBlocksToShow) => {
            //         _service.SetActionBlocksToShow(actionBlocksToShow.ToHashSet());
            //         _service.RefreshActionBlocksOnPage();
            //         _view.DestroyLoadingText();
            //     }
            // ));
        }
    }

    private void OnScrollbarValueChange(float value)
    {
        if (value < 0) return;

        if (_isLoadingActionBlocks == false && value <= 0.004f)
        {
            if (_service.GetActionBlocksToShow().Count <= _service.GetCountShowedActionBlocks())
            {
                return;
            }

            _isLoadingActionBlocks = true;

            
            bool isMouseButtonLeftDownOnStartRefreshActionBlocks = _service.IsMouseButtonLeftDown;
            
            _view.AddLoadingText();
            
            _view.BlockScrollCapability();
            StartCoroutine(_service.RefreshActionBlocksAfterPause(0.1f, OnRefreshed));

            void OnRefreshed()
            {   
                _view.DestroyLoadingText();
                
                if (isMouseButtonLeftDownOnStartRefreshActionBlocks)
                {
                    StartCoroutine(_service.WaitForMouseButtonLeftUp(OnMouseButtonLeftUp));

                    void OnMouseButtonLeftUp()
                    {
                        _view.UnblockScrollCapability();
                    }
                }
                else
                {
                    _view.UnblockScrollCapability();
                }

                _isLoadingActionBlocks = false;
            }
        }
    }
}