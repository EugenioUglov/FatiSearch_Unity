using Controllers;
using UnityEngine;
using System.IO;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

public class Main : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private ActionBlockController _actionBlockController;
    [SerializeField] private DragAndDropController _dragAndDropController;
    [SerializeField] private SearchController _searchController;
    [SerializeField] private CommandController _commandController;
    
    
    void Start()
    {
        UserSettings userSettings = new UserSettings();
        userSettings.ApplySettings();

        _dragAndDropController.Init();
        _searchController.Init();
        _actionBlockController.Init();
        

        IndexingFilesFromFilesToIndexDirectory();
    }

    void Update()
    {
        if (_searchController.IsSelectedInputField() == false && _commandController.IsSelectedInputField() == false)
        {
            _searchController.FocusInputField();
        }
    }

    private void IndexingFilesFromFilesToIndexDirectory()
    {

        string indexedFilesDirectory = @"Admin\IndexedFiles\";
        string filesToIndexDirectory = @"Admin\FilesToIndex\";

        List<string> filePathsToIndex = new List<string>();

        DirectoryInfo indexedFilesDirectoryInfo = new DirectoryInfo(indexedFilesDirectory);
        DirectoryInfo filesToIndexDirectoryInfo = new DirectoryInfo(filesToIndexDirectory);

        CreateDirectoryIfDoesNotExist(indexedFilesDirectory);
        CreateDirectoryIfDoesNotExist(filesToIndexDirectory);

        MoveFiles(filesToIndexDirectoryInfo, indexedFilesDirectoryInfo);
        // RemoveFolders(filesToIndexDirectoryInfo);
        RemoveEmptyFolders(filesToIndexDirectory);

        _actionBlockController.CreateActionBlocksByPaths(paths: filePathsToIndex.ToArray(), isShowError: true);

        
        void CreateDirectoryIfDoesNotExist(string directory)
        {
            if (!Directory.Exists(directory)) 
            {
                Directory.CreateDirectory(directory);
            }
        }

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

        
        void CopyFiles(DirectoryInfo source, DirectoryInfo target) 
        {
            Directory.CreateDirectory(target.FullName);

            foreach (var file in source.GetFiles())
            {
                Thread.Sleep(50);
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
            }

            foreach (var sourceSubdirectory in source.GetDirectories())
            {
                Thread.Sleep(50);
                var targetSubdirectory = target.CreateSubdirectory(sourceSubdirectory.Name);
                CopyFiles(sourceSubdirectory, targetSubdirectory);
            }
        }

        void MoveFiles(DirectoryInfo source, DirectoryInfo target) 
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
                    file.MoveTo(targetPath);
                    filePathsToIndex.Add(targetPath);
                }
            }

            foreach (var sourceSubdirectory in source.GetDirectories())
            {
                var targetSubdirectory = target.CreateSubdirectory(sourceSubdirectory.Name);
                MoveFiles(sourceSubdirectory, targetSubdirectory);
            }
        }

        void RemoveFiles(DirectoryInfo directoryInfo)
        {
            foreach (FileInfo file in directoryInfo.EnumerateFiles())
            {
                file.Delete(); 
            }
        } 

        void RemoveFolders(DirectoryInfo directoryInfo)   
        {
            foreach (DirectoryInfo dir in directoryInfo.EnumerateDirectories())
            {
                dir.Delete(true); 
            }
        }

        void RemoveEmptyFolders(string startLocation)
        {    
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                RemoveEmptyFolders(directory);
                if (Directory.GetFiles(directory).Length == 0 && 
                    Directory.GetDirectories(directory).Length == 0)
                {
                    print("Remove directory " + directory);
                    Directory.Delete(directory, false);
                }
            }
        }
    }
    
    public IEnumerator GetFilesFromDirectoryAsync(string directory, Action<string> onGetFile = null, Action<string[]> onEnd = null)
    {
        List<string> files = new List<string>();

        foreach(var file in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)) 
        {
            yield return null;
            
            files.Add(file);

            onGetFile?.Invoke(file);
        }
        

        onEnd?.Invoke(files.ToArray());
    }

    public void Quit()
    {
        UnityEngine.Application.Quit();
    }
}
