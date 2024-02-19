using System;
using UnityEngine;
using System.Windows.Forms;
using System.IO;

public class DragAndDropController : MonoBehaviour
{
   [Header("Links")]
   [SerializeField] private DragAndDropService _dragAndDropService;
   [SerializeField] private ActionBlockController _actionBlockController;
   
   private DirectoryManager _directoryManager;


   public void Init()
   {
      _directoryManager = new DirectoryManager();
      _dragAndDropService.CallbackGetDroppedFilesPaths = OnGetDroppedFilePaths;
   }
   
   private void OnGetDroppedFilePaths(string[] paths)
   {
      DialogResult dialogResult = MessageBox.Show("Do you want to copy dragged file (files) to the program data?", "Confirmation", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
      
      if (dialogResult == DialogResult.Yes) {
         foreach (string path in paths)
         {
            string filePathFromIndexedFolder = "";
            string targetPath = @"Admin\IndexedFiles";

            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(path);

            // detect whether its a directory
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
               try
               {
                  filePathFromIndexedFolder = _directoryManager.CopyFolder(path, targetPath);
               }
               catch (Exception exception)
               {
                  MessageBox.Show(exception.Message, "Error");
                  return;
               }
            }
            else
            { 
               filePathFromIndexedFolder = CopyFileKeepingFolders(path, targetPath);
            }
            
            _actionBlockController.CreateActionBlockByPath(filePathFromIndexedFolder);
         }
      }
      else if (dialogResult == DialogResult.No) {
         foreach (string path in paths)
         {
            _actionBlockController.CreateActionBlockByPath(path);
         }
      }
      else if (dialogResult == DialogResult.Cancel) {
         return;
      }

      _actionBlockController.SetActionBlocksToShow();
      _actionBlockController.RefreshActionBlocksOnPage();

      string CopyFileKeepingFolders(string fileToCopy, string targetDirectory)
      {
         string directoryOfFileToCopy = Path.GetDirectoryName(fileToCopy);
         string fileNameToCopy = Path.GetFileName(fileToCopy);

         string directoryOfFileToCopyWithoutDriveOrNetworkShare = directoryOfFileToCopy.Substring(Path.GetPathRoot(directoryOfFileToCopy).Length);

         // If the last symbol in directory is not slash then add it.
         if (targetDirectory[targetDirectory.Length - 1] != '\\')
         {
               targetDirectory += @"\";
         }

         targetDirectory += directoryOfFileToCopyWithoutDriveOrNetworkShare + @"\";

         string targetFilePath = targetDirectory + fileNameToCopy;

         if (File.Exists(targetFilePath))
         {
               print("File already exists: " + targetFilePath);
               return "";
         }
         else 
         {

         }

         CreateDirectoryIfNotExist(targetDirectory);
         File.Copy(fileToCopy, targetFilePath);

         void CreateDirectoryIfNotExist(string directory)
         {
               try
               {
                  // Determine whether the directory exists.
                  if (Directory.Exists(directory))
                  {

                  }
                  else 
                  {
                     // Try to create the directory.
                     DirectoryInfo di = Directory.CreateDirectory(directory);
                     Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(directory));
                  }
               }
               catch (Exception e)
               {
                  Console.WriteLine("The process failed: {0}", e.ToString());
               }
               finally {}
         }

         return targetFilePath;
      }
   }
}