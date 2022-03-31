using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using B83.Win32;

public class DragAndDropController : MonoBehaviour
{
   [Header("Links")]
   [SerializeField] private DragAndDropService _dragAndDropService;
   [SerializeField] private ActionBlockController _actionBlockController;

   public void Init()
   {
      _dragAndDropService.CallbackGetDroppedFilesPaths = OnGetDroppedFilesPaths;
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
         _actionBlockController.SetActionBlocksToShow();
         _actionBlockController.RefreshActionBlocksOnPage();
      }
   }
}