using System;
using System.Collections.Generic;
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
         _actionBlockController.CreateActionBlockByPath(path);
      }

      _actionBlockController.SetActionBlocksToShow();
      _actionBlockController.RefreshActionBlocksOnPage();
   }
}