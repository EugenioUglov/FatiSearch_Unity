using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting;
using Debug = UnityEngine.Debug;



public class ActionBlockController : MonoBehaviour
{
    [SerializeField] private ActionBlockModel _model;
    [SerializeField] private ActionBlockView _view;

    [SerializeField] private ActionBlockModifierController _actionBlockModifierController;
    [SerializeField] private ActionBlockCreatorController _actionBlockCreatorController;

    public Action CallbackStartLoadingActionBlocksToShow;
    public Action<string> CallBackActionBlockShowed;
    
    public void Init()
    {
        EventAggregator.AddListener<ActionBlockClickedEvent>(this, OnActionBlockClicked);
        EventAggregator.AddListener<ActionBlockSettingsClickedEvent>(this, OnActionBlockSettingsClicked);
        EventAggregator.AddListener<SearchEnteredEvent>(this, OnSearchEntered);
        _view.CallbackStartLoadingActionBlocksToShow = OnStartLoadingActionBlocksToShow;
        _view.CallBackActionBlockShowed = OnActionBlockShowed;
    }

    public void OnStartLoadingActionBlocksToShow()
    {
        if (CallbackStartLoadingActionBlocksToShow != null) CallbackStartLoadingActionBlocksToShow();
    }
    
    public void OnActionBlockShowed(string countActionBlocks)
    {
        if (CallBackActionBlockShowed != null) CallBackActionBlockShowed(countActionBlocks);
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
        var allActionBlocks = _model.GetActionBlocks();
        _view.ShowActionBlocks(allActionBlocks.ToHashSet());
    }

    public void ShowSettingsForActionBlock()
    {
        _actionBlockCreatorController.ShowSettingsForActionBlock();
    }

    public void HideSettingsForActionBlocks()
    {
        _actionBlockCreatorController.HidePage();
    }

    
    public void CreateActionBlock(ActionBlockModel.ActionBlock actionBlock)
    {
        _model.CreateActionBlock(actionBlock);
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
        print("Settings clicked " + titleActionBlock);
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
            Console.WriteLine(exception);
            print("Not possible to open path: " + path);
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
