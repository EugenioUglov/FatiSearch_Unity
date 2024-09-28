using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Controllers;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

public class ActionBlockService : MonoBehaviour
{
    [HideInInspector] public bool IsMouseButtonLeftDown = false;
    

    [Header("Links to scripts")]
    [SerializeField] private ActionBlockModel _model;
    [SerializeField] private ActionBlockView _view;
    [SerializeField] private ActionBlockSettingsController _actionBlockSettingsController;
    [SerializeField] private SearchController _searchController;
    [SerializeField] private AlertController _alertController;
    [SerializeField] private BottomMessageController _bottomMessageController;
    [SerializeField] private MessageFullscreenService _messageFullscreenService;
    [SerializeField] private DialogMessageService _dialogMessageService;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _centralLogText;
    [SerializeField] private GameObject _results;
    

    private HashSet<ActionBlockModel.ActionBlock> _actionBlocksToShow;
    private HashSet<ActionBlockModel.ActionBlock> _showedActionBlocks;
    private DirectoryManager _directoryManager;
    private int _countShowedActionBlocks = 0;
    private int _maxCountActionBlocksToShowAtTime = 10;
    private int _countProcessedFilesFromDirectories = 0;
    private int _countFilesFromDirectories = 0;
    private int _countDirectoriesForAutoCreationActionBlocks = 0;
    private IEnumerator _coroutineToShowActionBlocks = null;


    public void Init()
    { 
        _directoryManager = new DirectoryManager();

        _actionBlocksToShow = new HashSet<ActionBlockModel.ActionBlock>();

        ActionBlockModel.ActionBlock[] actionBlocksFromFile = GetActionBlocksFromFile();
        
        SetActionBlocks(actionBlocksFromFile);

        SetActionBlocksToShow();
        RefreshActionBlocksOnPage();
        // IndexingFilesFromFilesToIndexDirectory();
    }

    public bool CreateActionBlocks(ActionBlockModel.ActionBlock[] actionBlocks, bool isShowError = true)
    {
        StartCoroutine(_model.CreateActionBlocksAsync(
            actionBlocks, 
            isShowError,
            onActionBlockProcessedCallback: OnAddActionBlock, 
            onEnd: ()=> {
                _model.SaveToFile();
                SetActionBlocksToShow();
                RefreshActionBlocksOnPage();
            }
        ));
        
        void OnAddActionBlock() 
        {
            StartCoroutine(AddFileProcessedStatusAsync());
            // _countProcessedFilesFromDirectories++;
            // _loaderFullscreenService.SetText(_countProcessedFilesFromDirectories + " / " + _countFilesFromDirectories + " files are processed");
            // print(_countProcessedFilesFromDirectories + " / " + _countFilesFromDirectories + " files are processed");
        }

        RefreshView();
        
        return true;
    }
    
    public bool CreateActionBlocksByDirectoriesAndShow()
    {
        _countProcessedFilesFromDirectories = 0;
        int countFilesInDirectory = 0;

        _messageFullscreenService.Show(
            text: "Preparing files for auto creation Action-Blocks",
            title: MessageFullscreenService.Title.Loading,
            onCancel: () => 
            {
            _model.CancelProcess();
            _messageFullscreenService.Hide();
            }
        );

        StartCoroutine(GetDirectoriesForAutoCreationActionBlocksFromFileAsync(
            onDirectoriesReceived: (directoriesForAutoCreationActionBlocks) => {
                _countDirectoriesForAutoCreationActionBlocks = directoriesForAutoCreationActionBlocks.Length;

                if (_countDirectoriesForAutoCreationActionBlocks == 0)
                {
                    return;
                }

                StartCoroutine(_model.GetFilesFromDirectoriesAsync(
                    directoriesForAutoCreationActionBlocks, 
                    onGetFile: (file) => {
                        countFilesInDirectory++;
                        _messageFullscreenService.Show("Preparing files for auto creation Action-Blocks | Count files processed: " + countFilesInDirectory);
                    },
                    onEnd: (receivedFiles) => {
                        _countFilesFromDirectories = receivedFiles.Count(); 
                        CreateActionBlocksByPaths(paths: receivedFiles, isShowError: false);

                        // _loaderFullscreenService.SetText(_countProcessedFilesFromDirectories + " / " + _countFilesFromDirectories + " files are processed");
                        // _loaderFullscreenService.Show(onCancel: () => {
                        //     _model.CancelProcess();
                        //     _loaderFullscreenService.Hide();
                        // });
                    }
                )); 
            }
        ));

        return true;
    }

    public void CreateActionBlocksByPathsNoFreezeWithCopyingFilesToProgramData(string[] paths)
    {
        bool isCanceled = false;
        List<string> filePathsFromIndexedFolders = new List<string>();

        _messageFullscreenService.Show(
            text: "Copying files to program data.",
            title: MessageFullscreenService.Title.Loading,
            onCancel: () => { isCanceled = true; }
        );

        StartCoroutine(CreateActionBlockByPathsNoFreeze());

        IEnumerator CreateActionBlockByPathsNoFreeze()
        {
            // Copying files.
            foreach (string path in paths)
            {
                yield return null;
                if (isCanceled) yield break;

                string filePathFromIndexedFolder = "";
                string targetPath = @"Admin\IndexedFiles";

                // Get the file attributes for file or directory
                FileAttributes attr = File.GetAttributes(path);

                filePathFromIndexedFolder = "";
                bool isFileCopyingFinished = false;

                // Detect whether its a directory
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    try
                    {
                        _directoryManager.CopyFolderAsync(path, targetPath, onDone: (filePathFromIndexedFolderReceived) => {
                            filePathFromIndexedFolder = filePathFromIndexedFolderReceived;
                            isFileCopyingFinished = true;
                        });
                    }
                    catch (Exception exception)
                    {
                        _dialogMessageService.ShowMessage(exception.Message, "Error");

                        continue;
                    }
                }
                else
                {
                    _directoryManager.CopyFileKeepingFoldersAsync(
                        path, 
                        targetPath, 
                        onDone: (filePathFromIndexedFolderReceived) => {
                            filePathFromIndexedFolder = filePathFromIndexedFolderReceived;
                            isFileCopyingFinished = true;
                        }, 
                        onFail: (message) => { isFileCopyingFinished = true; }
                    );
                }

                while (isFileCopyingFinished == false)
                {
                    yield return new WaitForSeconds(1);
                }


                if (string.IsNullOrEmpty(filePathFromIndexedFolder))
                {
                    _alertController.Show($"Creating operation is canceled for path: \"{path}\". Action-Block already exists.");

                    continue;
                }

                filePathsFromIndexedFolders.Add(filePathFromIndexedFolder);
            }
            //

            _messageFullscreenService.Show(
                text: "Creating Action-Blocks.", 
                title: MessageFullscreenService.Title.Loading,
                onCancel: () => { isCanceled = true; }
            );

            IEnumerator pathsEnumerator = filePathsFromIndexedFolders.GetEnumerator();

            if (pathsEnumerator.MoveNext())
            {
                StartCoroutine(CreateActionBlockByPath((string)pathsEnumerator.Current));
            }
            else 
            {
                _messageFullscreenService.Hide();
            }


            IEnumerator CreateActionBlockByPath(string path)
            { 
                yield return null;
                if (isCanceled) yield break;

                CreateActionBlockByPathAsync(
                    path, 
                    onDone: (actionBlock) => 
                    {
                        if (pathsEnumerator.MoveNext())
                        {
                            string path = (string)pathsEnumerator.Current;
                            StartCoroutine(CreateActionBlockByPath(path));
                        }
                        else 
                        {
                            SetActionBlocksToShow();
                            RefreshActionBlocksOnPage();

                            _messageFullscreenService.Hide();
                        }
                    }
                );
            }
        }
    }
    
    public void CreateActionBlockByPathAsync(string path, Action<ActionBlockModel.ActionBlock> onDone, bool isShowError = true)
    {
        ActionBlockModel.ActionBlock actionBlock = _model.GetActionBlockObjectByPath(path);

        void GetSingularizedTagsAsync(string[] tags, Action<List<string>> onDone, Action onCancel)
        {
            StartCoroutine(GetSingularizedTags());

            IEnumerator GetSingularizedTags()
            {
                // Create a CancellationTokenSource to facilitate cancellation
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                _messageFullscreenService.Show(
                    text: $"Creating singularized tags for Action-Block: \"{actionBlock.Title}\".\nIt can take a while...\nYou can skip this process by clicking \"Cancel\" button", 
                    title: MessageFullscreenService.Title.Loading,
                    onCancel: ()  => 
                    { 
                        cancellationTokenSource.Cancel(); 
                        onCancel?.Invoke(); 
                    }
                );

                yield return null;
                    
                Task task = Task.Run(async () => await new TagManager().GetSyngularizedTagsAsync(
                    tags: tags,
                    onSingularizedTagsReady: (singularizedTagsFromAsync) => {
                        onDone?.Invoke(singularizedTagsFromAsync);
                    },
                    cancellationTokenSource: cancellationTokenSource
                ));
            }
        }

        GetSingularizedTagsAsync(
            tags: actionBlock.Tags.ToArray(),
            onDone: (singularizedTags) => 
            {
                print("GetSingularizedTagsAsync done");
                actionBlock.Tags = actionBlock.Tags.Concat(singularizedTags).ToList();
                OnMethodEnd();
            },
            onCancel: () =>
            {
                print("GetSingularizedTagsAsync canceled");
                OnMethodEnd();
            }
        );

        void OnMethodEnd()
        {
            _messageFullscreenService.Show(
                text: "Creating Action-Block.", 
                title: MessageFullscreenService.Title.Loading
            );
            
            bool isCreated = CreateActionBlock(actionBlock, isShowError, isAddTagsAutomatically: false);

            if (isCreated == false)
            {
                _alertController.Show("Action Block is not created: " + actionBlock.Title);
            }

            _messageFullscreenService.Hide();
            
            onDone?.Invoke(actionBlock);
        }
    }

    public bool CreateActionBlock(ActionBlockModel.ActionBlock actionBlock, bool isShowError = true, bool isAddTagsAutomatically = true)
    {
        // If path includes project path then start path from "\".
        string projectPath = Directory.GetCurrentDirectory();

        if (actionBlock.Content.Contains(projectPath))
        { 
            actionBlock.Content = actionBlock.Content.Replace(projectPath + @"\","");
        }
        //

        bool isCreated = _model.CreateActionBlock(actionBlock, isShowError, isAddTagsAutomatically);


        _model.SaveToFile();
        RefreshView();
        
        return isCreated;
    }
    
    public void CreateActionBlocksByPathsAsync(string[] paths)
    {
        bool isCanceled = false;

        _messageFullscreenService.Show(
            text: "Preparing Action-Blocks.", 
            title: MessageFullscreenService.Title.Loading,
            onCancel: () => { print("Creating Action-Block is canceled");  isCanceled = true; }
        );


        StartCoroutine(CreateActionBlocksByPathsNoFreeze());

        IEnumerator CreateActionBlocksByPathsNoFreeze()
        {
            _messageFullscreenService.Show();

            yield return null;

            IEnumerator pathsEnumerator = paths.GetEnumerator();

            if (pathsEnumerator.MoveNext())
            {
                StartCoroutine(CreateActionBlockByPath((string)pathsEnumerator.Current));
            }


            IEnumerator CreateActionBlockByPath(string path)
            { 
                yield return null;
                if (isCanceled) yield break;

                CreateActionBlockByPathAsync(
                    path, 
                    onDone: (actionBlock) => 
                    {
                        if (pathsEnumerator.MoveNext())
                        {
                            string path = (string)pathsEnumerator.Current;
                            StartCoroutine(CreateActionBlockByPath(path));
                        }
                        else 
                        {
                            SetActionBlocksToShow();
                            RefreshActionBlocksOnPage();

                            _messageFullscreenService.Hide();
                        }
                    }
                );
            }
        }
    }

    
    public bool CreateActionBlocksByPaths(string[] paths, bool isShowError = true)
    {
        StartCoroutine(GetActionBlocksByPathsAsync(
            paths, 
            onActionBlockReady: (actionBlock) => {
                // print("Action-Block ready: " + actionBlock.Title);
            },
            onActionBlocksReady: (actionBlocks) => {
                CreateActionBlocks(actionBlocks.ToArray(), isShowError);
            }
        ));

        return true;
    }

    public IEnumerator GetActionBlocksByPathsAsync(string[] paths, Action<ActionBlockModel.ActionBlock> onActionBlockReady = null, Action<List<ActionBlockModel.ActionBlock>> onActionBlocksReady = null)
    {
        List<ActionBlockModel.ActionBlock> actionBlocks = new List<ActionBlockModel.ActionBlock>();

        foreach (string path in paths)
        {
            yield return null;

            ActionBlockModel.ActionBlock actionBlock = _model.GetActionBlockObjectByPath(path);

            actionBlocks.Add(actionBlock);
            onActionBlockReady?.Invoke(actionBlock);
        }

        onActionBlocksReady?.Invoke(actionBlocks);
    }

    public int GetCountShowedActionBlocks()
    {
        return _countShowedActionBlocks;
    }

    public void SetActionBlocks(ActionBlockModel.ActionBlock[] actionBlocks)
    {
        _model.SetActionBlocks(actionBlocks);
    }
    
    public void SetActionBlocksToShow(HashSet<ActionBlockModel.ActionBlock> newActionBlocksToShow = null)
    {
        if (newActionBlocksToShow == null)
        {
            newActionBlocksToShow = _model.GetActionBlocks().ToHashSet();
        }
        
        _view.ClearActionBlocks();
        _view.SetSearchResultText("Found " + newActionBlocksToShow.Count() + " resuts");
        
        _countShowedActionBlocks = 0;
        _actionBlocksToShow = newActionBlocksToShow;
    }

    public ActionBlockModel.ActionBlock GetActionBlockByTitle(string title)
    {
        return _model.GetActionBlockByTitle(title);
    }

    public ActionBlockModel.ActionBlock[] GetActionBlocksFromFile()
    {
        return _model.GetActionBlocksFromFile();
    }

    public bool UpdateActionBlock(string title, ActionBlockModel.ActionBlock actionBlock)
    {
        bool isUpdated = _model.UpdateActionBlock(title, actionBlock);
        if (isUpdated == false) return isUpdated;
        
        _model.SaveToFile();
        RefreshView();
        
        return isUpdated;
    }

    public void DeleteActionBlock(ActionBlockModel.ActionBlock actionBlock, Action onDone = null)
    {
        _model.DeleteActionBlock(actionBlock, onDone: () => { 
            _model.SaveToFile();
            HashSet<ActionBlockModel.ActionBlock> actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
            SetActionBlocksToShow(actionBlocksToShow);
            RefreshActionBlocksOnPage();
            RefreshView();

            onDone?.Invoke();
        });
    }

    public void HideSettingsForActionBlocks()
    {
        _actionBlockSettingsController.HidePage();
    }

    public IEnumerator WaitForMouseButtonLeftUp(Action callbackMouseButtonLeftUp)
    {
        while (IsMouseButtonLeftDown)
        {
            yield return null;
        }
        
        callbackMouseButtonLeftUp.Invoke();
    }

    public IEnumerator GetDirectoriesForAutoCreationActionBlocksFromFileAsync(Action<string[]> onDirectoriesReceived)
    {
        yield return null;
        string[] directoriesForAutoCreationActionBlocks = _model.GetDirectoriesForAutoCreationActionBlocksFromFile();
        onDirectoriesReceived(directoriesForAutoCreationActionBlocks);
    }

    public bool ExecuteByTitle(string title)
    {
        ActionBlockModel.ActionBlock actionBlock = GetActionBlockByTitle(title);

        if (actionBlock.Title != null)
        {
            ExecuteByActionBlock(actionBlock);
            
            return true;
        }

        return false;
    }

    
    public void ExecuteByActionBlock(ActionBlockModel.ActionBlock actionBlock)
    {
        if (actionBlock.Action == ActionBlockModel.ActionEnum.OpenPath)
        {
            bool isOpened = _directoryManager.OpenDirectory(actionBlock.Content);
            
            if (isOpened)
            {
                _bottomMessageController.Show("Execution \"" + actionBlock.Title + "\"");
            }
            else
            {
                _alertController.Show("Not possible to execute Action-Block " + "\"" + actionBlock.Title + "\"");
            }
        }
        else if (actionBlock.Action == ActionBlockModel.ActionEnum.OpenPathAsAdministrator)
        {
            bool isOpened = _directoryManager.OpenDirectoryAsAdministrator(actionBlock.Content);
            
            if (isOpened)
            {
                _bottomMessageController.Show("Execution \"" + actionBlock.Title + "\"");
            }
            else
            {
                _alertController.Show("Not possible to execute Action-Block " + actionBlock.Title);
            }
        }
        else if (actionBlock.Action == ActionBlockModel.ActionEnum.SelectPath) 
        {
            _directoryManager.ShowInExplorer(actionBlock.Content);
        }
    }

    public void RefreshActionBlocksOnPage()
    {
        if (_actionBlocksToShow.Count <= _countShowedActionBlocks)
        {
            _searchController.ShowPage();

            return;
        }
        
        ActionBlockModel.ActionBlock[] actionBlocksToShowArray = _actionBlocksToShow.ToArray();
        int countShowedAtTime = 0;

        OnStartLoadingActionBlocksToShow();

        for (var i = 0; i < actionBlocksToShowArray.Length; i++)
        {
            if (_actionBlocksToShow.Count <= _countShowedActionBlocks || countShowedAtTime >= _maxCountActionBlocksToShowAtTime)
            {
                break;
            }

            ActionBlockModel.ActionBlock actionBlock = actionBlocksToShowArray[_countShowedActionBlocks];
            _view.AddActionBlock(actionBlock);
            _countShowedActionBlocks++;
            countShowedAtTime++;
        }

        OnActionBlocksShowed(_actionBlocksToShow.Count.ToString());
    }

    
    public void OnStartLoadingActionBlocksToShow()
    {
        HideSettingsForActionBlocks();
        _centralLogText.text = "Loading...";
    }

    public void RefreshView()
    {
        _searchController.ClearInputField();
        _searchController.ShowPage();
        HashSet<ActionBlockModel.ActionBlock> actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
        SetActionBlocksToShow(actionBlocksToShow);
        RefreshActionBlocksOnPage();
        _view.SetDefaultSettingsFields();
    }

    public IEnumerator RefreshActionBlocksAfterPause(float pauseSec, Action callbackEnd = null)
    {
        yield return new WaitForSeconds(pauseSec);
        // _view.ScrollToBottom();
        yield return new WaitForSeconds(pauseSec);

        
        RefreshActionBlocksOnPage();
        
        callbackEnd?.Invoke();
    }

    public void OnActionBlocksShowed(string countActionBlocks)
    {
        _centralLogText.text = "";
    }

    public HashSet<ActionBlockModel.ActionBlock> GetActionBlocksToShow()
    {
        return _actionBlocksToShow;
    }

    private IEnumerator AddFileProcessedStatusAsync()
    {
        yield return new WaitForSeconds(0.01f);
        _countProcessedFilesFromDirectories++;
        _messageFullscreenService.SetText(_countProcessedFilesFromDirectories + " / " + _countFilesFromDirectories + " files are processed");
    }

    
    private void CreateActionBlocksFromFolderToIndex(string[] directoriesForAutoCreationActionBlocks)
    {
        _countProcessedFilesFromDirectories = 0;
        int countFilesInDirectory = 0;

        _messageFullscreenService.Show(
        text: "Preparing files for auto creation Action-Blocks.",
        title: MessageFullscreenService.Title.Loading,
        onCancel: () => {
            _model.CancelProcess();
            _messageFullscreenService.Hide();
        });


        _countDirectoriesForAutoCreationActionBlocks = directoriesForAutoCreationActionBlocks.Length;

        if (_countDirectoriesForAutoCreationActionBlocks == 0)
        {
            return;
        }

        StartCoroutine(_model.GetFilesFromDirectoriesAsync(
            directoriesForAutoCreationActionBlocks, 
            onGetFile: (file) => {
                countFilesInDirectory++;
                _messageFullscreenService.SetText("Preparing files for auto creation Action-Blocks | Count files processed: " + countFilesInDirectory);
            },
            onEnd: (receivedFiles) => {
                _countFilesFromDirectories = receivedFiles.Count(); 
                CreateActionBlocksByPaths(paths: receivedFiles, isShowError: false);

                // _loaderFullscreenService.SetText(_countProcessedFilesFromDirectories + " / " + _countFilesFromDirectories + " files are processed");
                // _loaderFullscreenService.Show(onCancel: () => {
                //     _model.CancelProcess();
                //     _loaderFullscreenService.Hide();
                // });
            }
        )); 
    }

    public void ShowActionBlocksInScrollPanel(string userRequest)
    {
        _showedActionBlocks = null;
        _centralLogText.text = "Loading...";
        _view.ScrollToTop();
        _results.SetActive(false);
        _view.ClearActionBlocks();
        _view.AddLoadingText();

        if (_coroutineToShowActionBlocks != null)
        {
            StopCoroutine(_coroutineToShowActionBlocks);
            _coroutineToShowActionBlocks = null;
        }

        HashSet<ActionBlockModel.ActionBlock> actionBlocksToShow = new HashSet<ActionBlockModel.ActionBlock>();


        if (userRequest == "")
        {
            actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
            OnReceiveActionBlocks(actionBlocksToShow);
        }
        else
        {
            ActionBlockModel.ActionBlock actionBlockByTitle = _model.GetActionBlockByTitle(userRequest);
            // Not async.
            // HashSet<ActionBlockModel.ActionBlock> actionBlocksByRequest = _model.GetActionBlocksByRequest(userRequest).ToHashSet();
            _coroutineToShowActionBlocks = _model.GetActionBlocksByRequestAsync(
                request: userRequest,
                onGet: (actionBlocks) =>
                {
                    ShowActionBlocks(actionBlocks);
                }
            );

            StartCoroutine(_coroutineToShowActionBlocks);

            void ShowActionBlocks(ActionBlockModel.ActionBlock[] actionBlocks)
            {
                HashSet<ActionBlockModel.ActionBlock> actionBlocksByRequest = actionBlocks.ToHashSet();

                if (string.IsNullOrEmpty(actionBlockByTitle.Title) == false)
                {
                    actionBlocksToShow.Add(actionBlockByTitle);

                    foreach (var actionBlock in actionBlocksByRequest)
                    {
                        if (actionBlockByTitle.Title == actionBlock.Title) continue;
                        
                        actionBlocksToShow.Add(actionBlock);
                    }
                }
                else 
                {
                    actionBlocksToShow = actionBlocksByRequest;
                }

                OnReceiveActionBlocks(actionBlocksToShow);
            }
        }

        void OnReceiveActionBlocks(HashSet<ActionBlockModel.ActionBlock> actionBlocks)
        {
            _showedActionBlocks = actionBlocks;
            _results.SetActive(true);
            SetActionBlocksToShow(actionBlocks);
            RefreshActionBlocksOnPage();
            _view.DestroyLoadingText();
            _view.ScrollToTop();
            _centralLogText.text = "";
        }
    }

    public void ExecuteFirstShowedActionBlock()
    {
        if (_showedActionBlocks != null && _showedActionBlocks.Count > 0)
        {
            ExecuteByTitle(_showedActionBlocks.ToArray()[0].Title);
        }
    }

    public void IndexingFilesFromFilesToIndexDirectory()
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
        directoryManager.RemoveEmptyFolders(filesToIndexDirectory);

        CreateActionBlocksByPaths(paths: filePathsToIndex.ToArray(), isShowError: true);


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
}
