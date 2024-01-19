using Controllers;
using UnityEngine;
using System.IO;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using Unity.VisualScripting;

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
        

        IndexingFilesFromFilesToIndexDirectory();
        _loaderFullscreenService.Hide();
    }

    // void Update()
    // {
    //     if (_searchController.IsSelectedInputField() == false && _commandController.IsSelectedInputField() == false)
    //     {
    //         _searchController.FocusInputField();
    //     }
    // }

    private void IndexingFilesFromFilesToIndexDirectory()
    {
        string indexedFilesDirectory = @"Admin\IndexedFiles\";
        string filesToIndexDirectory = @"Admin\FilesToIndex\";

        List<string> filePathsToIndex = new List<string>();

        DirectoryManager directoryManager = new DirectoryManager();

        DirectoryInfo indexedFilesDirectoryInfo = new DirectoryInfo(indexedFilesDirectory);
        DirectoryInfo filesToIndexDirectoryInfo = new DirectoryInfo(filesToIndexDirectory);

        directoryManager.CreateDirectoryIfDoesNotExist(indexedFilesDirectory);
        directoryManager.CreateDirectoryIfDoesNotExist(filesToIndexDirectory);

        MoveFilesToIndexedFilesFolder(filesToIndexDirectoryInfo, indexedFilesDirectoryInfo);
        // RemoveFolders(filesToIndexDirectoryInfo);
        directoryManager.RemoveEmptyFolders(filesToIndexDirectory);

        _actionBlockController.CreateActionBlocksByPaths(paths: filePathsToIndex.ToArray(), isShowError: true);


        // var allDirectories = Directory.GetDirectories(indexedFilesDirectory, "*", SearchOption.AllDirectories);
        // foreach (string dir in allDirectories)
        // {
        //     string dirToCreate = dir.Replace(indexedFilesDirectory, filesToIndexDirectory);
        //     Directory.CreateDirectory(dirToCreate);
        // }

        // Move files to index directory.
        // StartCoroutine(GetFilesFromDirectoryAsync(
        //     directory: filesToIndexDirectory, 
        //     onGetFile: (file) => {
        //         Directory.Move(Path.GetDirectoryName(file) + "\\" + Path.GetFileName(file), indexedFilesDirectory + Path.GetFileName(file));
        //     })
        // );


        void MoveFilesToIndexedFilesFolder(DirectoryInfo source, DirectoryInfo target) 
        {
            Directory.CreateDirectory(target.FullName);

            foreach (var file in source.GetFiles())
            {
                string targetPath = Path.Combine(target.FullName, file.Name);

                if (File.Exists(targetPath))
                {
                    print("File already exists: " + targetPath);
                    continue;
                }
                else 
                {
                    print("Move file " + targetPath);
                    targetPath = targetPath.Substring(targetPath.IndexOf("Admin"));
                    file.MoveTo(targetPath);
                    filePathsToIndex.Add(targetPath);
                }
            }

            foreach (var sourceSubdirectory in source.GetDirectories())
            {
                var targetSubdirectory = target.CreateSubdirectory(sourceSubdirectory.Name);
                MoveFilesToIndexedFilesFolder(sourceSubdirectory, targetSubdirectory);
            }
        }
    }
    
    public void Quit()
    {
        UnityEngine.Application.Quit();
    }
}
