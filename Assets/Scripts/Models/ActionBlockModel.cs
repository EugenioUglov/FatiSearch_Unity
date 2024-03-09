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
    
    [Header("Links")] 
    [SerializeField] private FileController _fileController;

    private bool _isCanceled = false;
    private string _actionBlocksFilePath = @"Admin\Action-Blocks.json";
    private string _backupFolderPath = "Backup";

    private OrderedDictionary _actionBlockByTitle = new OrderedDictionary();
    private Dictionary<string, HashSet<ActionBlock>> _actionBlocksByTag = new Dictionary<string, HashSet<ActionBlock>>();
    private StringManager _stringManager = new StringManager();


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
            Debug.Log("ActionBlock by title \"" + titleLowerCase + "\" is not found");
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
        // Search by tags.

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

    public ActionBlock[] GetActionBlocksFromFile()
    {
        ActionBlock[] actionBlocksFromFile = new ActionBlock[]{};
        string actionBlocksJSONFromFile = _fileController.GetContentFromFile(_actionBlocksFilePath);

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
        string directoriesJSONFromFile = _fileController.GetContentFromFile(filePath);

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

    public bool CreateActionBlock(ActionBlock actionBlock, bool isShowError = true)
    {
        OnStartChangeActionBlocksVariables();

        string titleForActionBlock = actionBlock.Title;

        if (IsActionBlockValidToAdd(actionBlock, isShowError) == false)
        {
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
        actionBlockToCreate.Title = titleForActionBlock;
        actionBlockToCreate.Tags = GetTagsWithActionBlockTitle(titleForActionBlock, actionBlockToCreate);
        actionBlockToCreate.Tags = GetNormalizedTags(actionBlockToCreate.Tags);

        if (string.IsNullOrEmpty(actionBlock.ImagePath))
        {
            actionBlockToCreate.ImagePath = GetImagePathByDirectory(actionBlock.Content);
        }

        AddActionBlockToVariables(actionBlockToCreate);

        string GetImagePathByDirectory(string directory)
        {
            string extension = Path.GetExtension(directory);

            if (String.IsNullOrEmpty(extension)) {
                return ImagePath.Folder;
            }
            else {
                if (extension.ToLower().Equals(".exe"))
                {
                    return ImagePath.ExeFile;
                }
                else if (extension.ToLower().Equals(".txt") || extension.ToLower().Equals(".md") || extension.ToLower().Equals(".ini"))
                {
                    return ImagePath.TextFile;
                }
                else if (extension.ToLower().Equals(".avi") || extension.ToLower().Equals(".mp4") || extension.ToLower().Equals(".mov") || extension.ToLower().Equals(".wmv") || extension.ToLower().Equals(".mkv"))
                {
                    return ImagePath.VideoFile;
                }
                else if (extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".png") || extension.ToLower().Equals(".svg") || extension.ToLower().Equals(".ai") || extension.ToLower().Equals(".psd") || extension.ToLower().Equals(".bmp") || extension.ToLower().Equals(".tif") || extension.ToLower().Equals(".tiff") || extension.ToLower().Equals(".raw"))
                {
                    return ImagePath.ImageFile;
                }
                else if (extension.ToLower().Equals(".doc") || extension.ToLower().Equals(".docx"))
                {
                    return ImagePath.WordFile;
                }
                else if (extension.ToLower().Equals(".pdf"))
                {
                    return ImagePath.PdfFile;
                }
                else if (extension.ToLower().Equals(".csv") || extension.ToLower().Equals(".xls") || extension.ToLower().Equals(".xlsx") || extension.ToLower().Equals(".xml"))
                {
                    return ImagePath.ExcelFile;
                }
                else if (extension.ToLower().Equals(".url"))
                {
                    return ImagePath.LinkFile;
                }
            }

            return "";
        }

        bool IsValidUrl(string url)
        {
            string pattern = @"^(https?|ftp)://[^\s/$.?#].[^\s]*$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(url);
        }

        return true;
    }

    public bool CreateActionBlocks(ActionBlock[] actionBlocks, bool isShowError = true, Action onActionBlockProcessedCallback = null)
    {
        foreach (ActionBlock actionBlock in actionBlocks)
        {
            print(actionBlock.Title);
            OnStartChangeActionBlocksVariables();
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
            actionBlockToCreate.Title = titleForActionBlock;
            actionBlockToCreate.Tags = GetTagsWithActionBlockTitle(titleForActionBlock, actionBlockToCreate);
            actionBlockToCreate.Tags = GetNormalizedTags(actionBlockToCreate.Tags);
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
        OnStartChangeActionBlocksVariables();
        
        string originalTitleLowerCase = originalTitle.ToLower();
        string newTitle = actionBlock.Title;

        if (IsActionBlockValidToAdd(actionBlock, isShowError: true) == false)
        {
            return false;
        }

        if (string.Equals(originalTitle, actionBlock.Title) == false)
        {
            actionBlock.Tags = GetTagsWithActionBlockTitle(originalTitleLowerCase, actionBlock);
        }
        actionBlock.Tags = GetNormalizedTags(actionBlock.Tags);
        _actionBlockByTitle.Remove(originalTitleLowerCase);
        AddActionBlockToVariables(actionBlock);
        UpdateIndexActionBlockByTags();

        return true;
    }

    public void DeleteActionBlock(ActionBlock actionBlock)
    {
        DialogResult dialogResult = MessageBox.Show("Do you want to delete also the original file?", "Confirmation", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);

        DirectoryManager directoryManager = new DirectoryManager();

        if (dialogResult == DialogResult.Yes) {
            try {
                directoryManager.DeleteByPath(actionBlock.Content);
            } catch (IOException ioExp) {
                MessageBox.Show(ioExp.Message, "Error.");
            }
        }
        else if (dialogResult == DialogResult.Cancel) {
            return;
        }

        
        OnStartChangeActionBlocksVariables();

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
    }

    public void DeleteAllActionBlocks()
    {
        OnStartChangeActionBlocksVariables();
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
        _fileController.Save(_actionBlocksFilePath, actionBlocksJSON);
    }


    private List<string> GetTagsWithActionBlockTitle(string originalTitle, ActionBlock actionBlock)
    {
        string titleLowerCase = actionBlock.Title.ToLower();
        string titleWithoutSpecialSymbols = _stringManager.GetTextWithoutSpecialSymbols(actionBlock.Title.ToLower());
        string titleWithoutSpecialSymbolsAndSplitedCamelCase = _stringManager.SplitCamelCase(titleWithoutSpecialSymbols);
        List<string> tags = actionBlock.Tags;
            
        // Add tags from title words.
        AddTitleToTag();

        if (string.Equals(titleLowerCase, titleWithoutSpecialSymbolsAndSplitedCamelCase) == false)
        {
            AddTitleWithoutSpecialSymbolsToTag();
        }
        
        void AddTitleToTag()
        {
            foreach (var tag in tags)
            {
                if (tag == actionBlock.Title)
                {
                    print("Tag already exists " + actionBlock.Title);
                    return;
                }
            }
            
            tags.Add(actionBlock.Title);
        }

        void AddTitleWithoutSpecialSymbolsToTag()
        {
            foreach (var tag in tags)
            {
                if (tag == titleWithoutSpecialSymbolsAndSplitedCamelCase)
                {
                    // Tag as the title without spec symbols exists.
                    return;
                }
            }
            
            tags.Add(titleWithoutSpecialSymbolsAndSplitedCamelCase);
        }

        return tags;
    }

    private List<string> GetNormalizedTags(List<string> tags)
    {
        List<string> normalizedTags = new List<string>();
            
        foreach (string tag in tags)
        {
            // Delete empty spaces from sides of tags.
            string tagTrimmed = tag.Trim();

            // Repalce multiple spaces to one space.
            string tagTrimmedWithoutMultipleSpaces = Regex.Replace(tagTrimmed, @"\s+", " ");

            string normalizedTag = tagTrimmedWithoutMultipleSpaces;
            
            if (string.IsNullOrEmpty(normalizedTag) == false)
            {
                normalizedTags.Add(normalizedTag);
            }
        }

        return normalizedTags;
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
        
        _fileController.Save(_backupFolderPath + "/" + "Action-Blocks_" + 
                             DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json", 
            actionBlocksJSON);
    }

    private void OnStartChangeActionBlocksVariables()
    {

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
}
