using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;

public class ActionBlockModel : MonoBehaviour
{
    [SerializeField] private AlertController _alertController;
    [SerializeField] private DialogMessageService _dialogMessageService;
    
    public static class ActionEnum
    {
        public const string OpenPath = "OpenPath";
        public const string OpenPathAsAdministrator = "OpenPathAsAdministrator";
        public const string SelectPath = "SelectPath";
        public const string ShowInfo = "ShowInfo";
    }

    public static class ImagePath
    {
        private const string directoryWithImages = @"Admin\Images\FileType\";
        public const string ExcelFile = directoryWithImages + "Excel-file.png";
        public const string ExeFile = directoryWithImages + "Exe-file.png";
        public const string ImageFile = directoryWithImages + "Image-file.png";
        public const string LinkFile = directoryWithImages + "Link-file.png";
        public const string VideoFile = directoryWithImages + "Video-file.png";
        public const string WordFile = directoryWithImages + "Word-file.png";
        public const string TextFile = directoryWithImages + "Text-file.png";
        public const string PdfFile = directoryWithImages + "Pdf-file.png";
        public const string Folder = directoryWithImages + "Folder.png";
    }
    

    private bool _isCanceled = false;
    private string _actionBlocksFilePath = @"Admin\Action-Blocks.json";
    private string _backupFolderPath = "Backup";

    private OrderedDictionary _actionBlockByTitle = new OrderedDictionary();
    private Dictionary<string, HashSet<ActionBlock>> _actionBlocksByTag = new Dictionary<string, HashSet<ActionBlock>>();
    private StringManager _stringManager = new StringManager();
    private FileStreamManager _fileStreamManager = new FileStreamManager();

    private string _lastRequestOfActionBlocksByRequestAsync = null;
    private Action<ActionBlock[]> _lastActionOnGet = null;

    public struct ActionBlock
    {
        public string Title;
        public string Action;
        public string Content;
        public List<string> Tags;
        public string ImagePath;
        
        public ActionBlock(string title, string action, string content, List<string> tags, string imagePath = "")
        {
            this.Title = title;
            this.Action = action;
            this.Content = content;
            this.Tags = tags;
            this.ImagePath = imagePath;
        }
    }


    public List<ActionBlock> GetActionBlocks()
    {
        List<ActionBlock> actionBlocks = new List<ActionBlock>();
        
        ICollection actionBlocksFromDictionary = _actionBlockByTitle.Values;

        foreach (var actionBlock in actionBlocksFromDictionary)
        {
            actionBlocks.Add((ActionBlock)actionBlock);
        }
        
        return actionBlocks;
    }
    
    public ActionBlock GetActionBlockByTitle(string title)
    {
        string titleLowerCase = title.ToLower();
        
        ActionBlock actionBlock = new ActionBlock();

        try
        {
            actionBlock = (ActionBlock)_actionBlockByTitle[titleLowerCase];
        }
        catch
        {
            // Debug.Log("ActionBlock by title \"" + titleLowerCase + "\" is not found");
        }

        return actionBlock;
    }
    
    public List<ActionBlock> GetActionBlocksByTag(string tag)
    {
        string tagLowerCase = tag.ToLower();
        
        List<ActionBlock> actionBlocksByTag = new List<ActionBlock>();
        
        try
        {
            actionBlocksByTag = _actionBlocksByTag[tagLowerCase].ToList();
        }
        catch (Exception e)
        {
            // Debug.Log(e);
        }

        return actionBlocksByTag;
    }

    public ActionBlock[] GetActionBlocksByRequest(string request)
    {
        List<ActionBlock> actionBlocksToShow = new List<ActionBlock>();

        Dictionary<ActionBlock, int> priorityByActionBlock =
            new Dictionary<ActionBlock, int>();

        string[] tags = request.Split(' ');

        foreach (string tag in tags)
        {
            ActionBlock[] actionBlocksByTag = GetActionBlocksByTag(tag).ToArray().Reverse().ToArray();

            foreach (ActionBlock actionBlock in actionBlocksByTag)
            {
                if (priorityByActionBlock.ContainsKey(actionBlock))
                {
                    priorityByActionBlock[actionBlock] += 1;
                }
                else
                {
                    priorityByActionBlock[actionBlock] = 1;
                }
            }
        }

        // Sort array from min priority value.
        foreach (var pair in priorityByActionBlock.OrderBy(pair => pair.Value))
        {
            actionBlocksToShow.Add(pair.Key);
        }
        
        return actionBlocksToShow.ToArray().Reverse().ToArray(); 
    }

    public IEnumerator GetActionBlocksByRequestAsync(string request, Action<ActionBlock[]> onGet = null)
    {
        // Search by tags.

        List<ActionBlock> actionBlocksToShow = new List<ActionBlock>();

        Dictionary<ActionBlock, int> priorityByActionBlock =
            new Dictionary<ActionBlock, int>();

        string[] tags = request.Split(' ');

        foreach (string tag in tags)
        {
            yield return null;

            ActionBlock[] actionBlocksByTag = GetActionBlocksByTag(tag).ToArray().Reverse().ToArray();

            foreach (ActionBlock actionBlock in actionBlocksByTag)
            {
                if (priorityByActionBlock.ContainsKey(actionBlock))
                {
                    priorityByActionBlock[actionBlock] += 1;
                }
                else
                {
                    priorityByActionBlock[actionBlock] = 1;
                }
            }
        }

        // Sort array from min priority value.
        foreach (var pair in priorityByActionBlock.OrderBy(pair => pair.Value))
        {
            yield return null;

            actionBlocksToShow.Add(pair.Key);
        }

        onGet(actionBlocksToShow.ToArray().Reverse().ToArray());
    }


    // public ActionBlock[] GetActionBlocksWithExactTagsByRequest(string request)
    // {
    //     // Search by tags.

    //     List<ActionBlock> actionBlocksToShow = new List<ActionBlock>();

    //     Dictionary<ActionBlock, int> priorityByActionBlock =
    //         new Dictionary<ActionBlock, int>();

    //     string[] tags = request.Split(' ');

    //     foreach (string tag in tags)
    //     {
    //         ActionBlock[] actionBlocksByTag = GetActionBlocksByTag(tag).ToArray().Reverse().ToArray();

    //         foreach ()

    //         foreach (ActionBlock actionBlock in actionBlocksByTag)
    //         {
    //             if (priorityByActionBlock.ContainsKey(actionBlock))
    //             {
    //                 priorityByActionBlock[actionBlock] += 1;
    //             }
    //             else
    //             {
    //                 priorityByActionBlock[actionBlock] = 1;
    //             }
    //         }
    //     }

    //     // Sort array from min priority value.
    //     foreach (var pair in priorityByActionBlock.OrderBy(pair => pair.Value))
    //     {
    //         actionBlocksToShow.Add(pair.Key);
    //     }
        
    //     return actionBlocksToShow.ToArray().Reverse().ToArray(); 
    // }

    public ActionBlock GetActionBlockObjectByPath(string path)
    {
        List<string> tags = new List<string>();
        UserSettings settings = new UserSettings();

        SettingsData settingsData = settings.GetSettings();
        string fileName = Path.GetFileNameWithoutExtension(path);
        string[] foldersOfPath = path.Split('\\');
        string titleActionBlock = fileName;
        path = path.Replace(@"\\", @"\");


        AddDirectoryFoldersToTags();
        
        ActionBlock actionBlock = 
        new ActionBlock(titleActionBlock, ActionEnum.OpenPath, 
            path, tags);

        tags.Concat(GetTagsWithActionBlockTitle(actionBlock));
        tags = GetNormalizedTags(tags);
        actionBlock.Tags = tags;


        if (Convert.ToBoolean(settingsData.IsDirectoryInTitle))
        {
            actionBlock.Title = AddDirectoryToActionBlockTitle(titleActionBlock, foldersOfPath);
        }

        void AddDirectoryFoldersToTags()
        {
            for (int i = 0; i < foldersOfPath.Length - 1; i++)
            {
                // Add to tags folder names from path of a file.
                
                tags.Add(foldersOfPath[i]);
            }
        }

        string AddDirectoryToActionBlockTitle(string titleActionBlock, string[] foldersOfPath)
        {
            titleActionBlock += " (";

            for (int i = 0; i < foldersOfPath.Length - 1; i++)
            {
                if (i > 0)
                {
                    titleActionBlock += "\\";
                }

                titleActionBlock += foldersOfPath[i];
            }

            titleActionBlock += ")";

            return titleActionBlock;
        }


        return actionBlock;
    }

    public ActionBlock[] GetActionBlocksFromFile()
    {
        ActionBlock[] actionBlocksFromFile = new ActionBlock[]{};
        string actionBlocksJSONFromFile = _fileStreamManager.GetContentFromFile(_actionBlocksFilePath);

        if (string.IsNullOrEmpty(actionBlocksJSONFromFile) == false)
        {
            try
            {
                actionBlocksFromFile =
                    JsonConvert.DeserializeObject<ActionBlock[]>(actionBlocksJSONFromFile);
            }
            catch (Exception exception)
            {
                print("Warning! File with Action-Blocks.json not found");
                print(exception);
            }
        }

        return actionBlocksFromFile;
    }

    public string[] GetDirectoriesForAutoCreationActionBlocksFromFile()
    {
        string filePath = @"Admin\DirectoriesForAutoCreationActionBlocks.json";

        string[] directoriesFromFile = new string[]{};
        string directoriesJSONFromFile = _fileStreamManager.GetContentFromFile(filePath);

        if (string.IsNullOrEmpty(directoriesJSONFromFile) == false)
        {
            try
            {
                directoriesFromFile =
                    JsonConvert.DeserializeObject<string[]>(directoriesJSONFromFile);
            }
            catch (Exception exception)
            {
                print("Warning! File with DirectoriesForAutoCreationActionBlocks.json not found");
                print(exception);
            }
        }

        return directoriesFromFile;
    }

    public void SetActionBlocks(ActionBlock[] actionBlocks)
    {
        foreach (var actionBlock in actionBlocks)
        {
            AddActionBlockToVariables(actionBlock, false);
        }
    }

    public List<string> GetTitlesActionBlocksByTag(string tag)
    {
        string tagLowerCase = tag.ToLower();
        
        List<string> titlesActionBlocks = new List<string>();
        
        // Show Titles by tag.
        foreach (ActionBlock actionBlockByTag in GetActionBlocksByTag(tagLowerCase))
        {
            titlesActionBlocks.Add(actionBlockByTag.Title);
        }

        return titlesActionBlocks;
    }

    public bool CreateActionBlock(ActionBlock actionBlock, bool isShowError = true, bool isAddTagsAutomatically = true)
    {
        string titleForActionBlock = actionBlock.Title;

        if (IsActionBlockValidToAdd(actionBlock, isShowError) == false)
        {
            print("not valid action block");
            return false;
        }

        if (_actionBlockByTitle.Contains(titleForActionBlock.ToLower()))
        {
            ActionBlock existingActionBlock = GetActionBlockByTitle(titleForActionBlock.ToLower());

            if (existingActionBlock.Content.ToLower() != actionBlock.Content.ToLower())
            {
                titleForActionBlock = titleForActionBlock + " (" + actionBlock.Content + ")";

                if (_actionBlockByTitle.Contains(titleForActionBlock.ToLower()))
                {
                    print("Action-Block already exists");
                    return false;
                }

                string originalTitleOfExistingActionBlock = existingActionBlock.Title;
                string newTitleForExistingActionBlock = existingActionBlock.Title + " (" + existingActionBlock.Content + ")"; 
                existingActionBlock.Title = newTitleForExistingActionBlock;

                if (_actionBlockByTitle.Contains(newTitleForExistingActionBlock.ToLower()) == false)
                {
                    UpdateActionBlock(originalTitleOfExistingActionBlock, existingActionBlock);
                }
            }
            else
            {
                return false;
            }
        }

        ActionBlock actionBlockToCreate = actionBlock;

        if (isAddTagsAutomatically)
        {
            actionBlockToCreate.Tags = GetTagsWithActionBlockTitle(actionBlockToCreate);
            actionBlockToCreate.Tags = GetNormalizedTags(actionBlockToCreate.Tags);
        }
        
        actionBlockToCreate.Title = titleForActionBlock;

        if (string.IsNullOrEmpty(actionBlock.ImagePath))
        {
            actionBlockToCreate.ImagePath = GetImagePathByDirectory(actionBlock.Content);
        }

        AddActionBlockToVariables(actionBlockToCreate);


        string GetImagePathByDirectory(string directory)
        {
            string extension = Path.GetExtension(directory).ToLower();

            if (String.IsNullOrEmpty(extension)) {
                return ImagePath.Folder;
            }
            else {
                if (Extension.ExeExtensions.Contains(extension))
                {
                    return ImagePath.ExeFile;
                }
                else if (Extension.TextExtensions.Contains(extension))
                {
                    return ImagePath.TextFile;
                }
                else if (Extension.VideoExtensions.Contains(extension))
                {
                    return ImagePath.VideoFile;
                }
                else if (Extension.ImageExtensions.Contains(extension))
                {
                    return ImagePath.ImageFile;
                }
                else if (Extension.WordDocumentExtensions.Contains(extension))
                {
                    return ImagePath.WordFile;
                }
                else if (Extension.PdfDocumentExtensions.Contains(extension))
                {
                    return ImagePath.PdfFile;
                }
                else if (Extension.ExcelExtensions.Contains(extension))
                {
                    return ImagePath.ExcelFile;
                }
                else if (Extension.linkExtensions.Contains(extension))
                {
                    return ImagePath.LinkFile;
                }
            }

            return "";
        }
        
        
        return true;
    }

    public bool CreateActionBlocks(ActionBlock[] actionBlocks, bool isShowError = true, Action onActionBlockProcessedCallback = null)
    {
        foreach (ActionBlock actionBlock in actionBlocks)
        {
            string titleLowerCase = actionBlock.Title.ToLower();

            if (IsActionBlockValidToAdd(actionBlock, isShowError) == false)
            {
                onActionBlockProcessedCallback?.Invoke();
                continue;
            }

            string titleForActionBlock = actionBlock.Title;


            if (_actionBlockByTitle.Contains(titleLowerCase))
            {
                ActionBlock existingActionBlock = GetActionBlockByTitle(titleLowerCase);

                if (existingActionBlock.Content.ToLower() != actionBlock.Content.ToLower())
                {
                    titleForActionBlock = actionBlock.Title + " (" + actionBlock.Content + ")";

                    if (_actionBlockByTitle.Contains(titleForActionBlock.ToLower()))
                    {
                        onActionBlockProcessedCallback?.Invoke();
                        continue;
                    }

                    string originalTitleOfExistingActionBlock = existingActionBlock.Title;
                    string newTitleForExistingActionBlock = existingActionBlock.Title + " (" + existingActionBlock.Content + ")"; 
                    existingActionBlock.Title = newTitleForExistingActionBlock;
                    if (_actionBlockByTitle.Contains(newTitleForExistingActionBlock.ToLower()) == false)
                    {
                        UpdateActionBlock(originalTitleOfExistingActionBlock, existingActionBlock);
                    }
                }
            }
            
            ActionBlock actionBlockToCreate = actionBlock;
            actionBlockToCreate.Tags = GetTagsWithActionBlockTitle(actionBlockToCreate);
            actionBlockToCreate.Tags = GetNormalizedTags(actionBlockToCreate.Tags);
            actionBlockToCreate.Title = titleForActionBlock;
            AddActionBlockToVariables(actionBlockToCreate);

            onActionBlockProcessedCallback?.Invoke();
        }

        return true;
    }

    public IEnumerator CreateActionBlocksAsync(ActionBlock[] actionBlocks, bool isShowError = true, Action onActionBlockProcessedCallback = null, Action onEnd = null)
    {
        foreach (ActionBlock actionBlock in actionBlocks)
        {
            yield return null;

            if (_isCanceled) 
            {
                _isCanceled = false;
                yield break;
            } 


            CreateActionBlock(actionBlock, isShowError);
            onActionBlockProcessedCallback?.Invoke();
        }

        onEnd?.Invoke();
    }

    public bool UpdateActionBlock(string originalTitle, ActionBlock actionBlock)
    {
        string originalTitleLowerCase = originalTitle.ToLower();
        string newTitle = actionBlock.Title;

        if (IsActionBlockValidToAdd(actionBlock, isShowError: true) == false)
        {
            return false;
        }

        if (string.Equals(originalTitle, actionBlock.Title) == false)
        {
            actionBlock.Tags = GetTagsWithActionBlockTitle(actionBlock);
        }
        // actionBlock.Tags = GetNormalizedTags(actionBlock.Tags);
        _actionBlockByTitle.Remove(originalTitleLowerCase);
        AddActionBlockToVariables(actionBlock);
        UpdateIndexActionBlockByTags();

        return true;
    }

    public void DeleteActionBlock(ActionBlock actionBlock, Action onDone = null)
    {
        _dialogMessageService.ShowMessage("Do you want to delete also the original file?", "Confirmation", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information, onClickButton: (dialogResult) => {
            DirectoryManager directoryManager = new DirectoryManager();

            if (dialogResult == DialogResult.Yes) {
                try {
                    directoryManager.DeleteByPath(actionBlock.Content);
                } catch (IOException ioExp) {
                    _dialogMessageService.ShowMessage(ioExp.Message, "Error.");
                }
            }
            else if (dialogResult == DialogResult.Cancel) {
                onDone?.Invoke();
            }

            
            // int isFromIndexedFilesFolder = actionBlock.Content.IndexOf("Admin\\IndexedFiles");

            // if (isFromIndexedFilesFolder >= 0)
            // {
            //     try {
            //         // Check if file exists with its full path
            //         if (File.Exists(actionBlock.Content)) 
            //         {
            //             // If file found, delete it
            //             File.Delete(actionBlock.Content);
            //             Console.WriteLine("File deleted.");
            //         } else Console.WriteLine("File not found");
            //     } catch (IOException ioExp) {
            //         Console.WriteLine(ioExp.Message);
            //     }
            // }
            
            _actionBlockByTitle.Remove(actionBlock.Title.ToLower());
            UpdateIndexActionBlockByTags();
            onDone?.Invoke();
        });

   
    }

    public void DeleteAllActionBlocks()
    {
        _actionBlockByTitle.Clear();
    }
    
    public IEnumerator GetFilesFromDirectoriesAsync(string[] fileDirectories, Action<string> onGetFile = null, Action<string[]> onEnd = null)
    {
        List<string> files = new List<string>();

        foreach (string directory in fileDirectories)
        {
            foreach(var file in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)) 
            {
                yield return null;
                
                files.Add(file);

                onGetFile?.Invoke(file);
            }
        }

        onEnd?.Invoke(files.ToArray());
    }

    public IEnumerator GetFilesFromDirectoryAsync(string directory, Action<string> onGetFile = null, Action<string[]> onEnd = null)
    {
        List<string> files = new List<string>();

        foreach(var file in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)) 
        {
            yield return null;
            
            files.Add(file);

            onGetFile?.Invoke(file);
        }
        
        onEnd?.Invoke(files.ToArray());
    }

    public void CancelProcess()
    {
        _isCanceled = true;
    }
    
    public void SaveToFile()
    {
        ActionBlock[] actionBlocks = GetActionBlocks().ToArray();
        string actionBlocksJSON = JsonConvert.SerializeObject(actionBlocks);
        _fileStreamManager.Save(_actionBlocksFilePath, actionBlocksJSON);
    }


    private List<string> GetTagsWithActionBlockTitle(ActionBlock actionBlock)
    {
        string titleLowerCase = actionBlock.Title.ToLower();
        List<string> tags = actionBlock.Tags;
            
        // Add tags from title words.
        AddTitleToTag();
        // AddTitleWithoutSpecialSymbolsAndSplitedCamelCaseToTag();
        
        void AddTitleToTag()
        {
            foreach (string tag in tags)
            {
                if (tag == actionBlock.Title)
                {
                    return;
                }
            }
            
            tags.Add(actionBlock.Title);
        }

        void AddTitleWithoutSpecialSymbolsAndSplitedCamelCaseToTag()
        {
            string titleWithSplitedCamelCase = _stringManager.SplitCamelCase(actionBlock.Title);
            string titleWithoutSpecialSymbolsAndWithSplitedCamelCase = _stringManager.GetTextWithoutSpecialSymbols(titleWithSplitedCamelCase);

            if (string.Equals(titleLowerCase, titleWithoutSpecialSymbolsAndWithSplitedCamelCase))
            {
                return;
            }

            foreach (var tag in tags)
            {
                if (tag == titleWithoutSpecialSymbolsAndWithSplitedCamelCase)
                {
                    // Tag as the title without spec symbols exists.
                    return;
                }
            }
            
            tags.Add(titleWithoutSpecialSymbolsAndWithSplitedCamelCase);
        }

        return tags;
    }

    private List<string> GetNormalizedTags(List<string> tags)
    {
        return new TagManager().GetNormalizedTags(tags);
    }

    private void CreateFolderIfMissing(string path)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(path);
            }
        }
        catch (IOException ioex)
        {
            _alertController.Show(ioex.ToString());
        }
    }

    private void CreateBackupFileWithActionBlocks()
    {
        ActionBlock[] actionBlocks = GetActionBlocks().ToArray();
        string actionBlocksJSON = JsonConvert.SerializeObject(actionBlocks);

        CreateFolderIfMissing(_backupFolderPath);
        
        _fileStreamManager.Save(_backupFolderPath + "/" + "Action-Blocks_" + 
                             DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json", 
            actionBlocksJSON);
    }

    private bool IsActionBlockValidToAdd(ActionBlock actionBlockToAdd,  bool isShowError = true)
    {
        // If title already exists then check also content, if content different then Action-Block valid.
        if (_actionBlockByTitle.Contains(actionBlockToAdd.Title))
        {
            ActionBlock existingActionBlock = GetActionBlockByTitle(actionBlockToAdd.Title);

            if (existingActionBlock.Content == actionBlockToAdd.Content)
            {
                if (isShowError)
                {
                    _alertController.Show("Action-Block with this title already exists.");
                }

                return false;
            }
            else
            {
                return true; 
            }

            return false;
        }
    
        // If title is empty then Action-Block is invalid.
        if (string.IsNullOrEmpty(actionBlockToAdd.Title))
        {
            if (isShowError)
            {
                _alertController.Show("Error! Title is not defined.");
            }
            
            return false;
        }


        return true;
    }
    
    private void UpdateIndexActionBlockByTags()
    {
        _actionBlocksByTag.Clear();

        ActionBlock[] actionBlocks = GetActionBlocks().ToArray();

        foreach (var actionBlock in actionBlocks)
        {
            // Add Action-Blocks by tag.
            // Separated elements by ",".
            foreach (string tagPhrase in actionBlock.Tags)
            {
                string[] wordsFromTag = tagPhrase.Split(' ');


                // Separated elements by " ".
                foreach (string tagWord in wordsFromTag)
                {
                    // Skip empty symbols.
                    if (string.IsNullOrEmpty(tagWord)) continue;

                    string tagWordLowCase = tagWord.ToLower();

                    // Create new List for tags if doesn't exist yes.
                    if (_actionBlocksByTag.TryGetValue(tagWordLowCase, out HashSet<ActionBlock> value1) == false)
                    {
                        _actionBlocksByTag[tagWordLowCase] = new HashSet<ActionBlock>() { };
                    }

                    _actionBlocksByTag[tagWordLowCase].Add(actionBlock);
                }
            }
        }
    }

    private bool AddActionBlockToVariables(ActionBlock actionBlock, bool isAddToBeginning = true)
    {
        string titleLowerCase = actionBlock.Title.ToLower();

        if (isAddToBeginning)
        {
            // Insert a new key to the beginning of the OrderedDictionary
            _actionBlockByTitle.Insert(0, titleLowerCase, actionBlock);
        }
        else
        {
            _actionBlockByTitle.Add(titleLowerCase, actionBlock);
        }

        // Add Action-Blocks by tag.
        // Separated elements by ",".
        foreach (string tagPhrase in actionBlock.Tags)
        {
            string[] wordsFromTag = tagPhrase.Split(' ');
            
            // Separated elements by " ".
            foreach (string tagWord in wordsFromTag)
            {
                // Skip empty symbols.
                if (tagWord == "") continue;
                
                string tagWordLowCase = tagWord.ToLower();
                
                // Create new List for tags if doesn't exist yes.
                if (_actionBlocksByTag.TryGetValue(tagWordLowCase, out HashSet<ActionBlock> value1) == false) {
                    _actionBlocksByTag[tagWordLowCase] = new HashSet<ActionBlock>(){};
                }
                
                _actionBlocksByTag[tagWordLowCase].Add(actionBlock);
            }
        }

        return true;
    }

    private static class  Extension {
        public static string[] ExeExtensions {get; private set;} = new string[] {"exe"};
        public static string[] TextExtensions {get; private set;} = new string[] {"txt", "md", "ini"};
        public static string[] VideoExtensions {get; private set;} = new string[] {"avi", "mp4", "mov", "mkv"};
        public static string[] ImageExtensions {get; private set;} = new string[] {"jpeg", "jpg", "png", "svg", "ai", "psd", "bmp", "tif", "tiff", "raw"};
        public static string[] WordDocumentExtensions {get; private set;} = new string[] {"doc", "docx"};
        public static string[] PdfDocumentExtensions {get; private set;} = new string[] {"pdf"};
        public static string[] ExcelExtensions {get; private set;} = new string[] {"csv", "xls", "xlsx", "xml"};
        public static string[] linkExtensions {get; private set;} = new string[] {"url"};
    }
}