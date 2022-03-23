using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Controllers;
using TMPro;
using UnityEngine;


public class Main : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private ActionBlockController _actionBlockController;
    [SerializeField] private TextMeshProUGUI _centralLogText;
    [SerializeField] private DragAndDropController _dragAndDropController;
    [SerializeField] private SearchController _searchController;

    void Start()
    {
        _dragAndDropController.CallbackGetDroppedFilesPaths = OnGetDroppedFilesPaths;
        
        _actionBlockController.CallbackStartLoadingActionBlocksToShow = OnStartLoadingActionBlocksToShow;
        _actionBlockController.CallBackActionBlockShowed = OnActionBlocksShowed;
        
        _actionBlockController.Init();
        ActionBlockModel.ActionBlock[] actionBlocksFromFile = _actionBlockController.GetActionBlocksFromFile();
        _actionBlockController.SetActionBlocks(actionBlocksFromFile);
        _actionBlockController.ShowActionBlocks();
        _searchController.ShowPage();
    }

    private void OnGetDroppedFilesPaths(string[] paths)
    {
        foreach (string path in paths)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            String[] foldersOfPath = path.Split('\\');
            List<string> tags = new List<string>();
                    
            for (int i = 0; i < foldersOfPath.Length - 1; i++)
            {
                // Add to tags folders without the file name or last folder.
                
                tags.Add(foldersOfPath[i]);
            }

            ActionBlockModel.ActionBlock actionBlock = 
                new ActionBlockModel.ActionBlock(fileName, ActionBlockModel.ActionEnum.OpenPath, 
                    path, tags);
            
            _actionBlockController.CreateActionBlock(actionBlock);
            _actionBlockController.ShowActionBlocks();
        }
    }

    private void OnStartLoadingActionBlocksToShow()
    {
        _actionBlockController.HideSettingsForActionBlocks();
        _centralLogText.text = "Loading...";
        print(_centralLogText.text);
    }
    
    private void OnActionBlocksShowed(string countActionBlocks)
    {
        _centralLogText.text = "";
        // print("_centralLogText.text");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
