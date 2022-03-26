using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using Unity.VisualScripting;
using Debug = UnityEngine.Debug;


public class ActionBlockController : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private ActionBlockModel _model;
    [SerializeField] private ActionBlockView _view;

    [SerializeField] private ActionBlockModifierController _actionBlockModifierController;
    [SerializeField] private ActionBlockCreatorController _actionBlockCreatorController;
    [SerializeField] private AlertController _alertController;
    [SerializeField] private BottomMessageController _bottomMessageController;
    [SerializeField] private TextMeshProUGUI _centralLogText;
    
    
    public void Init()
    {
        EventAggregator.AddListener<ActionBlockClickedEvent>(this, OnActionBlockClicked);
        EventAggregator.AddListener<ActionBlockSettingsClickedEvent>(this, OnActionBlockSettingsClicked);
        EventAggregator.AddListener<SearchEnteredEvent>(this, OnSearchEntered);
        
        ActionBlockModel.ActionBlock[] actionBlocksFromFile = GetActionBlocksFromFile();
        SetActionBlocks(actionBlocksFromFile);
        ShowActionBlocks();
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
    
    public void ShowActionBlocks()
    {
        OnStartLoadingActionBlocksToShow();
        var allActionBlocks = _model.GetActionBlocks();
        _view.ShowActionBlocks(allActionBlocks.ToHashSet());
        OnActionBlocksShowed(allActionBlocks.Count.ToString());
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
    
    public void UpdateActionBlock(string title, ActionBlockModel.ActionBlock actionBlock)
    {
        _model.UpdateActionBlock(title, actionBlock);
    }

    public void DeleteActionBlock(ActionBlockModel.ActionBlock actionBlock)
    {
        _model.DeleteActionBlock(actionBlock);
        ShowActionBlocks();
    }
    
    public bool ExecuteByTitle(string title)
    {
        ActionBlockModel.ActionBlock actionBlock = GetActionBlockByTitle(title);

        if (actionBlock.Title != null)
        {
            ExecuteByActionBlock(actionBlock);
            _bottomMessageController.Show("Execution \"" + actionBlock.Title + "\"");
            
            return true;
        }

        return false;
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
        HashSet<ActionBlockModel.ActionBlock> actionBlocksToShow;
        
        if (userRequest == "")
        {
            actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
        }
        else
        {
            bool isExecutedByTitle = ExecuteByTitle(userRequest);
            actionBlocksToShow = _model.GetActionBlocksByRequest(userRequest).ToHashSet();
        }

        _view.ShowActionBlocks(actionBlocksToShow);
    }
    
    private void ExecuteByActionBlock(ActionBlockModel.ActionBlock actionBlock)
    {
        if (actionBlock.Action == ActionBlockModel.ActionEnum.OpenPath)
        {
            OpenPath(actionBlock.Content);
        }
        else if (actionBlock.Action == ActionBlockModel.ActionEnum.SelectPath) 
        {
            SelectPath(actionBlock.Content);
        }
    }

    private void OpenPath(string path)
    {
        try
        {
            Process.Start(path);
        }
        catch (Exception exception)
        {
            _alertController.Show("Not possible to execute Action-Block");
        }
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
