using UnityEngine;
using System.Windows.Forms;


public class DragAndDropController : MonoBehaviour
{
   [Header("Links")]
   [SerializeField] private DragAndDropService _dragAndDropService;
   [SerializeField] private ActionBlockController _actionBlockController;
   
   
   public void Init()
   {
      _dragAndDropService.CallbackGetDroppedFilesPaths = OnGetDroppedFilePaths;
   }
   
   private void OnGetDroppedFilePaths(string[] paths)
   {
      DialogResult dialogResult = MessageBox.Show("Do you want to copy dragged file (files) to the program data?", "Confirmation", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
      
      if (dialogResult == DialogResult.Yes) {
         _actionBlockController.CreateActionBlocksByPathsNoFreezeWithCopyingFilesToProgramData(paths);
      }
      else if (dialogResult == DialogResult.No) {
         _actionBlockController.CreateActionBlocksByPathsNoFreeze(paths);
      }
      else if (dialogResult == DialogResult.Cancel) {
         return;
      }
   }
}