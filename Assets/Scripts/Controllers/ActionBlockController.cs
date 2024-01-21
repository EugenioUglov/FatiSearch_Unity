using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using TMPro;
using UnityEngine;
using Unity.VisualScripting;


public class ActionBlockController : MonoBehaviour
{
    [Header("Links to scripts")]
    [SerializeField] private ActionBlockModel _model;
    [SerializeField] private ActionBlockView _view;

    [SerializeField] private ActionBlockSettingsController _actionBlockSettingsController;
    [SerializeField] private SearchController _searchController;
    [SerializeField] private AlertController _alertController;
    [SerializeField] private BottomMessageController _bottomMessageController;
    [SerializeField] private CommandController _commandController;
    [SerializeField] private PageService _pageService;
    [SerializeField] private LoaderFullscreenService _loaderFullscreenService;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _centralLogText;
    
    
    private string _userRequest = "";
    private HashSet<ActionBlockModel.ActionBlock> _actionBlocksToShow;
    private int _maxCountActionBlocksToShowAtTime = 10;
    private int _countShowedActionBlocks = 0;
    private bool _isMouseButtonLeftDown = false;
    private bool _isLoadingActionBlocks = false;
    private int _countProcessedFilesFromDirectories = 0;
    private int _countFilesFromDirectories = 0;
    private int _countDirectoriesForAutoCreationActionBlocks = 0;

    private DirectoryManager _directoryManager;

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            _isMouseButtonLeftDown = true;
        }
        else
        {
            _isMouseButtonLeftDown = false;
        }
    }
    
    public void Init()
    {
        _directoryManager = new DirectoryManager();
        
        EventAggregator.AddListener<ActionBlockClickedEvent>(this, OnActionBlockClicked);
        EventAggregator.AddListener<ActionBlockSettingsClickedEvent>(this, OnActionBlockSettingsClicked);
        EventAggregator.AddListener<ActionBlockFileLocationClickedEvent>(this, OnActionBlockFileLocationClicked);
        EventAggregator.AddListener<SearchEnteredEvent>(this, OnSearchEntered);
        EventAggregator.AddListener<CommandEnteredEvent>(this, OnCommandEntered);
        EventAggregator.AddListener<ValueChangedInInputFieldSearchEvent>(this, OnValueChangedInInputFieldSearch);

        _view.BindScrollbarValueChange(OnScrollbarValueChange);


        _actionBlocksToShow = new HashSet<ActionBlockModel.ActionBlock>();
        ActionBlockModel.ActionBlock[] actionBlocksFromFile = GetActionBlocksFromFile();
        
        SetActionBlocks(actionBlocksFromFile);
        HashSet<ActionBlockModel.ActionBlock> actionBlocksToShow = _model.GetActionBlocks().ToHashSet();

        IndexingFilesFromFilesToIndexDirectory();

        SetActionBlocksToShow();
        RefreshActionBlocksOnPage();
    }

    public void OnStartLoadingActionBlocksToShow()
    {
        HideSettingsForActionBlocks();
        _centralLogText.text = "Loading...";
    }
    
    public void OnActionBlocksShowed(string countActionBlocks)
    {
        _centralLogText.text = "";
    }

    public ActionBlockModel.ActionBlock[] GetActionBlocksFromFile()
    {
        return _model.GetActionBlocksFromFile();
    }

    public void SetActionBlocks(ActionBlockModel.ActionBlock[] actionBlocks)
    {
        _model.SetActionBlocks(actionBlocks);
    }
    
    public ActionBlockModel.ActionBlock GetActionBlockByTitle(string title)
    {
        return _model.GetActionBlockByTitle(title);
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

    public void HideSettingsForActionBlocks()
    {
        _actionBlockSettingsController.HidePage();
    }

    public bool CreateActionBlock(ActionBlockModel.ActionBlock actionBlock, bool isShowError = true)
    {
        bool isCreated = _model.CreateActionBlock(actionBlock, isShowError);
        _model.SaveToFile();
        RefreshView();
        
        return isCreated;
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
                _loaderFullscreenService.Hide();
                _loaderFullscreenService.SetText("");
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

    public bool CreateActionBlockByPath(string path, bool isShowError = true)
    {
        ActionBlockModel.ActionBlock actionBlock = GetActionBlockObject(path);
        
        CreateActionBlock(actionBlock, isShowError);

        return true;
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

    public bool UpdateActionBlock(string title, ActionBlockModel.ActionBlock actionBlock)
    {
        bool isUpdated = _model.UpdateActionBlock(title, actionBlock);
        if (isUpdated == false) return isUpdated;
        
        _model.SaveToFile();
        RefreshView();
        
        return isUpdated;
    }
    
    public void DeleteActionBlock(ActionBlockModel.ActionBlock actionBlock)
    {
        _model.DeleteActionBlock(actionBlock);
        _model.SaveToFile();
        HashSet<ActionBlockModel.ActionBlock> actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
        SetActionBlocksToShow(actionBlocksToShow);
        RefreshActionBlocksOnPage();
        RefreshView();
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

    public void OnClickButtonCreate()
    {
        _searchController.HidePage();
        _actionBlockSettingsController.ShowSettingsToCreateActionBlock();
    }

    public void SetActionBlocksToShow(HashSet<ActionBlockModel.ActionBlock> newActionBlocksToShow = null)
    {
        _view.ScrollToTop();
        if (newActionBlocksToShow == null)
        {
            newActionBlocksToShow = _model.GetActionBlocks().ToHashSet();
        }
        
        _view.ClearActionBlocks();
        _view.SetSearchResultText("Found " + newActionBlocksToShow.Count() + " resuts");
        
        _countShowedActionBlocks = 0;
        _actionBlocksToShow = newActionBlocksToShow;
        _view.ScrollToTop();
    }

    public void OnCancelCreateActionBlocksByDirectories()
    {
        _loaderFullscreenService.OnCancel(
            onCancel: () => {
                _loaderFullscreenService.Hide();
            });
    }
    
    
    private void OnActionBlockClicked(ActionBlockClickedEvent actionBlockClickedEvent)
    {
        string titleActionBlock = actionBlockClickedEvent.Title;
        ExecuteByTitle(titleActionBlock);
    }

    private void OnActionBlockSettingsClicked(ActionBlockSettingsClickedEvent actionBlockSettingsClickedEvent)
    {
        string titleActionBlock = actionBlockSettingsClickedEvent.Title;
        ActionBlockModel.ActionBlock actionBlock = GetActionBlockByTitle(titleActionBlock);
        _actionBlockSettingsController.ShowSettingsToUpdateActionBlock(actionBlock);
    }

    private void OnActionBlockFileLocationClicked(ActionBlockFileLocationClickedEvent actionBlockFileLocationClickedEvent)
    {
        string titleActionBlock = actionBlockFileLocationClickedEvent.Title;
        ActionBlockModel.ActionBlock actionBlock = GetActionBlockByTitle(titleActionBlock);
        _directoryManager.GoToFileLocation(path: actionBlock.Content);
    }

    private void OnSearchEntered(SearchEnteredEvent searchEnteredEvent)
    {
        string userRequest = searchEnteredEvent.Request;
        HashSet<ActionBlockModel.ActionBlock> actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
        
        if (userRequest == "")
        {
            actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
        }
        else
        {
            bool isExecutedByTitle = ExecuteByTitle(userRequest);
            actionBlocksToShow = _model.GetActionBlocksByRequest(userRequest).ToHashSet();
        }
        
        SetActionBlocksToShow(actionBlocksToShow);
        RefreshActionBlocksOnPage();
    }

    private void OnCommandEntered(CommandEnteredEvent commandEnteredEvent)
    {
        string commandRequest = commandEnteredEvent.Request.ToLower();

        if (commandRequest == "execute all found results")
        {
            OnRequestExecuted();

            foreach (ActionBlockModel.ActionBlock actionBlock in _actionBlocksToShow)
            {
                ExecuteByTitle(actionBlock.Title);
            }
        }
        else if (commandRequest == "update")
        {
            OnRequestExecuted();
            CreateActionBlocksByDirectoriesAndShow();
        }
        else if (commandRequest.Contains("exact tags"))
        {
            OnRequestExecuted();

            string[] tagsFromRequest = _userRequest.Split(' ');
            HashSet<ActionBlockModel.ActionBlock> _actionBlocksByExactTagsToShow = new HashSet<ActionBlockModel.ActionBlock>();
            
            foreach (var actionBlock in _actionBlocksToShow)
            {
                // In all tags of request.
                foreach (var tagFromReqeust in tagsFromRequest)
                {
                    // In all tags of Action Block.
                    foreach (string tagFromActionBlock in actionBlock.Tags)
                    {
                        if (tagFromReqeust == tagFromActionBlock)
                        {
                            _actionBlocksByExactTagsToShow.Add(actionBlock);
                            break;
                        }
                    }
                }
            }

            SetActionBlocksToShow(_actionBlocksByExactTagsToShow.ToHashSet());


            RefreshActionBlocksOnPage();
            _view.DestroyLoadingText();

        }
        else if (commandRequest == "delete all action-blocks")
        {
            OnRequestExecuted();
            _model.DeleteAllActionBlocks();
            RefreshView();
            _model.SaveToFile();
        }

        void OnRequestExecuted()
        {
            _commandController.ClearInputField();
        }
    }

    private void OnValueChangedInInputFieldSearch(ValueChangedInInputFieldSearchEvent valueChangedInInputFieldSearchEvent)
    {
        string userRequest = valueChangedInInputFieldSearchEvent.Request;
        _userRequest = userRequest;
        HashSet<ActionBlockModel.ActionBlock> actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
        _view.ClearActionBlocks();
        _view.AddLoadingText();

        if (userRequest == "")
        {
            actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
            SetActionBlocksToShow(actionBlocksToShow);
            RefreshActionBlocksOnPage();
            _view.DestroyLoadingText();
        }
        else
        {
            // Not async.
            actionBlocksToShow = _model.GetActionBlocksByRequest(userRequest).ToHashSet(); 
            SetActionBlocksToShow(actionBlocksToShow.ToHashSet());
            RefreshActionBlocksOnPage();
            _view.DestroyLoadingText();
            //

            // StartCoroutine(_model.GetActionBlocksByRequestAsync(
            //     userRequest, 
            //     onGet: (actionBlocksToShow) => {
            //         SetActionBlocksToShow(actionBlocksToShow.ToHashSet());
            //         RefreshActionBlocksOnPage();
            //         _view.DestroyLoadingText();
            //     }
            // ));
        }
    }
    
    private void ExecuteByActionBlock(ActionBlockModel.ActionBlock actionBlock)
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
                _alertController.Show("Not possible to execute Action-Block " + actionBlock.Title);
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
            _directoryManager.GoToFileLocation(actionBlock.Content);
        }
    }

    private void RefreshView()
    {
        _searchController.ClearInputField();
        _searchController.ShowPage();
        HashSet<ActionBlockModel.ActionBlock> actionBlocksToShow = _model.GetActionBlocks().ToHashSet();
        SetActionBlocksToShow(actionBlocksToShow);
        RefreshActionBlocksOnPage();
        _view.SetDefaultSettingsFields();
    }
    
    private void OnScrollbarValueChange(float value)
    {
        if (value < 0) return;

        if (_isLoadingActionBlocks == false && value <= 0.004f)
        {
            if (_actionBlocksToShow.Count <= _countShowedActionBlocks)
            {
                return;
            }

            _isLoadingActionBlocks = true;

            
            bool isMouseButtonLeftDownOnStartRefreshActionBlocks = _isMouseButtonLeftDown;
            
            _view.AddLoadingText();
            
            _view.BlockScrollCapability();
            StartCoroutine(RefreshActionBlocksAfterPause(0.1f, OnRefreshed));

            void OnRefreshed()
            {   
                _view.DestroyLoadingText();
                
                if (isMouseButtonLeftDownOnStartRefreshActionBlocks)
                {
                    StartCoroutine(WaitForMouseButtonLeftUp(OnMouseButtonLeftUp));

                    void OnMouseButtonLeftUp()
                    {
                        _view.UnblockScrollCapability();
                    }
                }
                else
                {
                    _view.UnblockScrollCapability();
                }

                _isLoadingActionBlocks = false;
            }
        }
    }
    
    private IEnumerator WaitForMouseButtonLeftUp(Action callbackMouseButtonLeftUp)
    {
        while (_isMouseButtonLeftDown)
        {
            yield return null;
        }
        
        callbackMouseButtonLeftUp.Invoke();
    }
    
    private IEnumerator RefreshActionBlocksAfterPause(float pauseSec, Action callbackEnd = null)
    {
        yield return new WaitForSeconds(pauseSec);
        // _view.ScrollToBottom();
        yield return new WaitForSeconds(pauseSec);

        
        RefreshActionBlocksOnPage();
        
        callbackEnd?.Invoke();
    }

    private bool CreateActionBlocksByDirectoriesAndShow()
    {
        _countProcessedFilesFromDirectories = 0;
        int countFilesInDirectory = 0;

        _loaderFullscreenService.SetText("Preparing files for auto creation Action-Blocks");
        _loaderFullscreenService.Show(onCancel: () => {
            _model.CancelProcess();
            _loaderFullscreenService.Hide();
        });

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
                        _loaderFullscreenService.SetText("Preparing files for auto creation Action-Blocks | Count files processed: " + countFilesInDirectory);
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

    public void CreateActionBlocksFromFolderToIndex(string[] directoriesForAutoCreationActionBlocks)
    {
        _countProcessedFilesFromDirectories = 0;
        int countFilesInDirectory = 0;

        _loaderFullscreenService.SetText("Preparing files for auto creation Action-Blocks");
        _loaderFullscreenService.Show(onCancel: () => {
            _model.CancelProcess();
            _loaderFullscreenService.Hide();
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
                _loaderFullscreenService.SetText("Preparing files for auto creation Action-Blocks | Count files processed: " + countFilesInDirectory);
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

    private IEnumerator GetDirectoriesForAutoCreationActionBlocksFromFileAsync(Action<string[]> onDirectoriesReceived)
    {
        yield return null;
        string[] directoriesForAutoCreationActionBlocks = _model.GetDirectoriesForAutoCreationActionBlocksFromFile();
        onDirectoriesReceived(directoriesForAutoCreationActionBlocks);
    }

    private IEnumerator AddFileProcessedStatusAsync()
    {
        yield return new WaitForSeconds(0.01f);
        _countProcessedFilesFromDirectories++;
        _loaderFullscreenService.SetText(_countProcessedFilesFromDirectories + " / " + _countFilesFromDirectories + " files are processed");
    }

    private IEnumerator GetActionBlocksByPathsAsync(string[] paths, Action<ActionBlockModel.ActionBlock> onActionBlockReady = null, Action<List<ActionBlockModel.ActionBlock>> onActionBlocksReady = null)
    {
        List<ActionBlockModel.ActionBlock> actionBlocks = new List<ActionBlockModel.ActionBlock>();

        foreach (string path in paths)
        {
            yield return null;

            ActionBlockModel.ActionBlock actionBlock = GetActionBlockObject(path);

            actionBlocks.Add(actionBlock);
            onActionBlockReady?.Invoke(actionBlock);
        }

        onActionBlocksReady?.Invoke(actionBlocks);
    }


    private ActionBlockModel.ActionBlock GetActionBlockObject(string path)
    {
        UserSettings settings = new UserSettings();
        List<string> tags = new List<string>();
        string fileName = Path.GetFileNameWithoutExtension(path);
        string[] foldersOfPath = path.Split('\\');
        string titleActionBlock = fileName;
        SettingsData settingsData = settings.GetSettings();
                
        if (Convert.ToBoolean(settingsData.IsDirectoryInTitle)) 
        {
            titleActionBlock += " (";    
        }

        for (int i = 0; i < foldersOfPath.Length - 1; i++)
        {
            // Add to tags folder names from path of a file.
            
            tags.Add(foldersOfPath[i]);

            if (Convert.ToBoolean(settingsData.IsDirectoryInTitle)) 
            {
                if (i > 0) 
                {
                    titleActionBlock += "\\";
                }

                titleActionBlock += foldersOfPath[i];
            }
        }

        if (Convert.ToBoolean(settingsData.IsDirectoryInTitle)) 
        {
            titleActionBlock += ")";    
        }
        

        ActionBlockModel.ActionBlock actionBlock = 
        new ActionBlockModel.ActionBlock(titleActionBlock, ActionBlockModel.ActionEnum.OpenPath, 
            path, tags);

        return actionBlock;
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
        // RemoveFolders(filesToIndexDirectoryInfo);
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
                    print("Move file " + targetPath);
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