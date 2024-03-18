using UnityEngine;
using System.Windows.Forms;

public class DragAndDropController : MonoBehaviour
{
   [Header("Links")]
   [SerializeField] private DragAndDropService _dragAndDropService;
   [SerializeField] private ActionBlockController _actionBlockController;
   [SerializeField] private ActionBlockService _actionBlockService;
   [SerializeField] private DialogMessageService _dialogueMessageService;
   
   
   public void Init()
   {
      _dragAndDropService.CallbackGetDroppedFilesPaths = OnGetDroppedFilePaths;
   }
   
   private void OnGetDroppedFilePaths(string[] paths)
   {
      _dialogueMessageService.ShowMessage("Do you want to copy dragged file (files) to the program data?", "Confirmation", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information, (dialogResult) => { 
         if (dialogResult == DialogResult.Yes) {
            _actionBlockService.CreateActionBlocksByPathsNoFreezeWithCopyingFilesToProgramData(paths);
         }
         else if (dialogResult == DialogResult.No) {
            _actionBlockService.CreateActionBlocksByPathsAsync(paths);
         }
         else if (dialogResult == DialogResult.Cancel) {
            return;
         }
      });
   }
}