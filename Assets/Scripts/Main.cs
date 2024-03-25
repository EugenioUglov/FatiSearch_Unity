using UnityEngine;
using System.Collections;
using Controllers;
using System.IO;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Windows.Forms;

public class Main : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private ActionBlockController _actionBlockController;
    [SerializeField] private DragAndDropController _dragAndDropController;
    [SerializeField] private SearchController _searchController;
    [SerializeField] private CommandController _commandController;
    [SerializeField] private MessageFullscreenService _messageFullscreenService;
    [SerializeField] private ActionBlockService _actionBlockService;
    [SerializeField] private DialogMessageService _dialogService;
   

    private void Start()
    {
        _messageFullscreenService.Show(text: "Preparing all Action-Blocks", MessageFullscreenService.Title.Loading);

        StartCoroutine(StartWithNoFreeze());

        IEnumerator StartWithNoFreeze()
        {
            yield return null;

            UserSettings userSettings = new UserSettings();
            userSettings.ApplySettings();
            _dragAndDropController.Init();
            _searchController.Init();
            _actionBlockController.Init();
            _actionBlockService.Init(); 
            _messageFullscreenService.Hide();

            // _actionBlockService.CreateActionBlockByPathAsync(@"D:\Fun\Games my data\Install\Lies of P.(.v.1.5.0.0.build.13450100).(2023) [Decepticon] RePack\game-2.bin", (actionBlock) => { 
                // print(actionBlock.Title); 
                
            // });

            // string[] paths = {@"D:\Fun\Games my data\Install\Lies of P.(.v.1.5.0.0.build.13450100).(2023) [Decepticon] RePack\game-2.bin"};
            // _actionBlockService.CreateActionBlocksByPathsAsync(paths);


            // new DirectoryManager().CopyFileKeepingFoldersAsync(@"D:\Fun\Games my data\Install\Lies of P.(.v.1.5.0.0.build.13450100).(2023) [Decepticon] RePack\game-2.bin", @"Admin\IndexedFiles", (targetFilePath)=> { print("NICE:" + targetFilePath);});
            

            // _actionBlockscreenService.CreateActionBlocksByPathsNoFreezeWithCopyingFilesToProgramData(paths);
            // _messageFullscreenService.Hide();

            // new DirectoryManager().CopyFileKeepingFoldersAsync(paths[0], @"Admin\IndexedFiles",
            // (targetFilePath)=> { 
            //     print("NICE:" + targetFilePath); 
            //     _messageFullscreenService.Hide();
            // });

            // Create a CancellationTokenSource to facilitate cancellation
            // CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();



            // CancellationToken token = cancellationTokenSource.Token;

            // Task task = Task.Run(async () => await CopyFileAsync(onDone: (message)=> {
            //     print(message);
            // }));
            
            // _actionBlockscreenService.CreateActionBlocksByPathsNoFreezeWithCopyingFilesToProgramData(paths);


            // _actionBlockController.CreateActionBlockByPathAsync(@"D:\Archive\Alessandro laurea\Alessandro laurea2\boys girls families women men sponges", onDone: (actionBlock) => { print("Done"); });

            
        }
    }


    async Task CopyFileAsync(Action<string> onDone)
    {
        print("async");
        string fileToCopy = @"D:\Fun\Games my data\Install\Lies of P.(.v.1.5.0.0.build.13450100).(2023) [Decepticon] RePack\game-2.bin";
        string targetPath = @"Admin\IndexedFiles\game-2.bin";

        // Create a CancellationTokenSource to facilitate cancellation
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        
        // Cancel the file move operation
        // cancellationTokenSource.Cancel();

        // Start the file move operation in a separate task
        Task moveTask = Task.Run(() =>
        {
            if (cancellationTokenSource.Token.IsCancellationRequested) return;
            
            try
            {
                File.Copy(fileToCopy, targetPath);
                onDone("File moved successfully.");
            }
            catch (Exception ex)
            {
                onDone($"Error occurred: {ex.Message}");
            }
        });

        print("after task run");

        await moveTask;

        print("File move process stopped.");
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