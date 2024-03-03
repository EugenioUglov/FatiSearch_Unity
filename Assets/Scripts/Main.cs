using Controllers;
using UnityEngine;





using System;
using UnityEngine;
using System.Windows.Forms;
using System.IO;

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
        _loaderFullscreenService.Hide();

        // DirectoryManager _directoryManager;
        // _directoryManager = new DirectoryManager();

        // string[] paths = new string[1];

        // paths[0] = @"F:\FUN\Films and Videos\Good watched films.txt";
   

        // foreach (string path in paths)
        // {
        //     string filePathFromIndexedFolder = "";
        //     string targetPath = @"Admin\IndexedFiles";

        //     // get the file attributes for file or directory
        //     FileAttributes attr = File.GetAttributes(path);

        //     // detect whether its a directory
        //     if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
        //     {
        //         try
        //         {
        //             filePathFromIndexedFolder = _directoryManager.CopyFolder(path, targetPath);
        //         }
        //         catch (Exception exception)
        //         {
        //             MessageBox.Show(exception.Message, "Error");
        //             return;
        //         }
        //     }
        //     else
        //     {
        //         print("This is a file");
        //         filePathFromIndexedFolder = CopyFileKeepingFolders(path, targetPath);
        //     }

        //     if (filePathFromIndexedFolder != "") {
        //         print("OK");
        //     }    
        //     _actionBlockController.CreateActionBlockByPath(filePathFromIndexedFolder);
        // }

        // _actionBlockController.SetActionBlocksToShow();
        // _actionBlockController.RefreshActionBlocksOnPage();
        

        // string CopyFileKeepingFolders(string fileToCopy, string targetDirectory)
        // {
        //     string directoryOfFileToCopy = Path.GetDirectoryName(fileToCopy);
        //     string fileNameToCopy = Path.GetFileName(fileToCopy);

        //     string directoryOfFileToCopyWithoutDriveOrNetworkShare = directoryOfFileToCopy.Substring(Path.GetPathRoot(directoryOfFileToCopy).Length);

        //     // If the last symbol in directory is not slash then add it.
        //     if (targetDirectory[targetDirectory.Length - 1] != '\\')
        //     {
        //         targetDirectory += @"\";
        //     }

        //     targetDirectory += directoryOfFileToCopyWithoutDriveOrNetworkShare + @"\";

        //     string targetFilePath = targetDirectory + fileNameToCopy;

        //     if (File.Exists(targetFilePath))
        //     {
        //         print("File already exists: " + targetFilePath);
        //         return "";
        //     }
        //     else 
        //     {

        //     }

        //     CreateDirectoryIfNotExist(targetDirectory);
        //     File.Copy(fileToCopy, targetFilePath);

        //     void CreateDirectoryIfNotExist(string directory)
        //     {
        //         try
        //         {
        //             // Determine whether the directory exists.
        //             if (Directory.Exists(directory))
        //             {

        //             }
        //             else 
        //             {
        //                 // Try to create the directory.
        //                 DirectoryInfo di = Directory.CreateDirectory(directory);
        //                 Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(directory));
        //             }
        //         }
        //         catch (Exception e)
        //         {
        //             Console.WriteLine("The process failed: {0}", e.ToString());
        //         }
        //         finally {}
        //     }

        //     return targetFilePath;
        // }
   
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