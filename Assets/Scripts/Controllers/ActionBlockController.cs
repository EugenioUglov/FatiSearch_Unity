using System.Collections.Generic;
using Controllers;
using UnityEngine;
using Unity.VisualScripting;

public class ActionBlockController : MonoBehaviour
{
    [Header("Links to scripts")]
    [SerializeField] private ActionBlockModel _model;
    [SerializeField] private ActionBlockView _view;
    [SerializeField] private ActionBlockService _service;

    [SerializeField] private ActionBlockSettingsController _actionBlockSettingsController;
    [SerializeField] private SearchController _searchController;
    [SerializeField] private CommandController _commandController;

    private string _userRequest = "";
    private bool _isLoadingActionBlocks = false;
    private DirectoryManager _directoryManager;
    

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
        
        EventAggregator.AddListener<ActionBlockClickedEvent>(this, OnActionBlockClicked);
        EventAggregator.AddListener<ActionBlockSettingsClickedEvent>(this, OnActionBlockSettingsClicked);
        EventAggregator.AddListener<ActionBlockFileLocationClickedEvent>(this, OnActionBlockFileLocationClicked);
        EventAggregator.AddListener<SearchEnteredEvent>(this, OnSearchEntered);
        EventAggregator.AddListener<CommandEnteredEvent>(this, OnCommandEntered);
        EventAggregator.AddListener<ValueChangedInInputFieldSearchEvent>(this, OnValueChangedInInputFieldSearch);

        _view.BindScrollbarValueChange(OnScrollbarValueChange);
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
        _service.ExecuteFirstShowedActionBlock();
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
        string userRequest = valueChangedInInputFieldSearchEvent.Request;
        _userRequest = userRequest;

        _service.ShowActionBlocksInScrollPanel(userRequest);
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