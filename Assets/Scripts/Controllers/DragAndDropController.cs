using System;
using UnityEngine;
using System.Windows.Forms;
using System.IO;
using System.Collections;

public class DragAndDropController : MonoBehaviour
{
   [Header("Links")]
   [SerializeField] private DragAndDropService _dragAndDropService;
   [SerializeField] LoaderFullscreenService _loaderFullscreenService;
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
         StartCoroutine(CreateActionBlockByPathsNoFreeze());

         IEnumerator CreateActionBlockByPathsNoFreeze()
         {
            _loaderFullscreenService.Show();
            
            foreach (string path in paths)
            {
               yield return null;
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

                     continue;
                  }
               }
               else
               {
                  filePathFromIndexedFolder = _directoryManager.CopyFileKeepingFolders(path, targetPath);
               }

               if (string.IsNullOrEmpty(filePathFromIndexedFolder))
               {
                  MessageBox.Show("Creating operation is canceled. Action-Block already exists from path: " + path, "Warning");

                  continue;
               }

               _actionBlockController.CreateActionBlockByPath(filePathFromIndexedFolder);
            }

            OnActionBlocksCreated();
            _loaderFullscreenService.Hide();
         }
      }
      else if (dialogResult == DialogResult.No) {
         StartCoroutine(CreateActionBlockByPathsNoFreeze());

         IEnumerator CreateActionBlockByPathsNoFreeze()
         {
            _loaderFullscreenService.Show();
            
            foreach (string path in paths)
            {
               yield return null;

               _actionBlockController.CreateActionBlockByPath(path);
            }

            OnActionBlocksCreated();

            _loaderFullscreenService.Hide();
         }
      }
      else if (dialogResult == DialogResult.Cancel) {
         return;
      }

      void OnActionBlocksCreated()
      { 
         _actionBlockController.SetActionBlocksToShow();
         _actionBlockController.RefreshActionBlocksOnPage();
      }
   }
}