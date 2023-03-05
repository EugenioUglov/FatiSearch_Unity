using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Unity.VisualScripting;
using System.Text.RegularExpressions;


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
    
    [Header("Links")] 
    [SerializeField] private FileController _fileController;
    
    private string actionBlocksFilePath = "Action-Blocks.json";
    private string backupFolderPath = "Backup";

    private OrderedDictionary _actionBlockByTitle = new OrderedDictionary();
    private Dictionary<string, HashSet<ActionBlock>> _actionBlocksByTag = new Dictionary<string, HashSet<ActionBlock>>();


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

    public ActionBlock[] GetActionBlocksFromFile()
    {
        ActionBlock[] actionBlocksFromFile = new ActionBlock[]{};
        string actionBlocksJSONFromFile = _fileController.GetContentFromFile(actionBlocksFilePath);

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
        string filePath = "DirectoriesForAutoCreationActionBlocks.json";

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


        
        string titleLowerCase = actionBlock.Title.ToLower();

        // foreach (DictionaryEntry item in _actionBlockByTitle)
        // {
        //     print(item.Key);
        //     print(item.Value);
        // }
        if (IsTitleValid(titleLowerCase) == false)
        {
            return false;
        }

        // string normalizedTitle = GetNormalizedTitle(titleLowerCase);

        string normalizedTitle = titleLowerCase;

        if (_actionBlockByTitle.Contains(normalizedTitle))
        {
            ActionBlock existingActionBlock = GetActionBlockByTitle(normalizedTitle);

            if (existingActionBlock.Content != actionBlock.Content)
            {
                normalizedTitle = normalizedTitle + " (" + actionBlock.Content + ")";

                if (_actionBlockByTitle.Contains(normalizedTitle))
                {
                    return false;
                }

                string originalTitleOfExistingActionBlock = existingActionBlock.Title;
                string newTitleForExistingActionBlock = existingActionBlock.Title + " (" + existingActionBlock.Content + ")"; 
                existingActionBlock.Title = newTitleForExistingActionBlock;
                UpdateActionBlock(originalTitleOfExistingActionBlock, existingActionBlock);
            }
        }
        
        actionBlock.Title = normalizedTitle;
        actionBlock.Tags = GetUpdatedTagsForCreationActionBlock(normalizedTitle, actionBlock);
        
        AddActionBlockToVariables(actionBlock);
        OnUpdateActionBlocks();

        return true;
        
        
        bool IsTitleValid(string title)
        {
            // If title already exists.
            if (_actionBlockByTitle.Contains(title))
            {
                ActionBlock existingActionBlock = GetActionBlockByTitle(title);

                if (existingActionBlock.Content == actionBlock.Content)
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
        
            if (string.IsNullOrEmpty(title))
            {
                if (isShowError)
                {
                    _alertController.Show("Error! Title is not defined.");
                }
                
                return false;
            }

            return true;
        }
    }

    public bool CreateActionBlocks(ActionBlock[] actionBlocks, bool isShowError = true)
    {
        foreach (ActionBlock actionBlock in actionBlocks)
        {
            OnStartChangeActionBlocksVariables();

            
            string titleLowerCase = actionBlock.Title.ToLower();

            // foreach (DictionaryEntry item in _actionBlockByTitle)
            // {
            //     print(item.Key);
            //     print(item.Value);
            // }
            if (IsTitleValid(titleLowerCase, actionBlock) == false)
            {
                return false;
            }

            // string normalizedTitle = GetNormalizedTitle(titleLowerCase);

            string normalizedTitle = titleLowerCase;

            if (_actionBlockByTitle.Contains(normalizedTitle))
            {
                ActionBlock existingActionBlock = GetActionBlockByTitle(normalizedTitle);

                if (existingActionBlock.Content != actionBlock.Content)
                {
                    normalizedTitle = normalizedTitle + " (" + actionBlock.Content + ")";

                    if (_actionBlockByTitle.Contains(normalizedTitle))
                    {
                       continue;
                    }

                    string originalTitleOfExistingActionBlock = existingActionBlock.Title;
                    string newTitleForExistingActionBlock = existingActionBlock.Title + " (" + existingActionBlock.Content + ")"; 
                    existingActionBlock.Title = newTitleForExistingActionBlock;
                    UpdateActionBlock(originalTitleOfExistingActionBlock, existingActionBlock);
                }
            }
            
            ActionBlock actionBlockToCreate = actionBlock;
            actionBlockToCreate.Title = normalizedTitle;
            actionBlockToCreate.Tags = GetUpdatedTagsForCreationActionBlock(normalizedTitle, actionBlockToCreate);
            AddActionBlockToVariables(actionBlockToCreate);
        }

        OnUpdateActionBlocks();

        return true;
        
        bool IsTitleValid(string title, ActionBlock actionBlockToAdd)
        {
            // If title already exists.
            if (_actionBlockByTitle.Contains(title))
            {
                ActionBlock existingActionBlock = GetActionBlockByTitle(title);

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
        
            if (string.IsNullOrEmpty(title))
            {
                if (isShowError)
                {
                    _alertController.Show("Error! Title is not defined.");
                }
                
                return false;
            }

            return true;
        }
    }

    public bool UpdateActionBlock(string originalTitle, ActionBlock actionBlock)
    {
        OnStartChangeActionBlocksVariables();
        
        string originalTitleLowerCase = originalTitle.ToLower();
        string newTitle = actionBlock.Title;

        if (IsTitleValid(originalTitleLowerCase, newTitle) == false)
        {
            return false;
        }

        actionBlock.Tags = GetUpdatedTagsForCreationActionBlock(originalTitleLowerCase, actionBlock);

        _actionBlockByTitle.Remove(originalTitleLowerCase);
        AddActionBlockToVariables(actionBlock);
        UpdateIndexActionBlockByTags();
        
        print("Action-BLock " + actionBlock.Title + " has been updated");
        
        OnUpdateActionBlocks();

        return true;
        
        bool IsTitleValid(string originalTitle, string newTitle)
        {
            if (originalTitle != newTitle)
            {
                // If title already exists.
                if (_actionBlockByTitle.Contains(newTitle))
                {
                    _alertController.Show("Action-Block with this title already exists: " + newTitle);
                    return false;
                }
                
                if (string.IsNullOrEmpty(newTitle))
                {
                    _alertController.Show("Error! Title is not defined.");
                    return false;
                }
            }
            
            return true;
        }
    }


    public void DeleteActionBlock(ActionBlock actionBlock)
    {
        OnStartChangeActionBlocksVariables();
        _actionBlockByTitle.Remove(actionBlock.Title.ToLower());
        UpdateIndexActionBlockByTags();
        OnUpdateActionBlocks();
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

        int _maxCountActionBlocksToCreate = 50;
        
        if (GetActionBlocks().Count() >= _maxCountActionBlocksToCreate)
        {
            _alertController.Show("The limit has been reached.\nMaximum amount of Action-Blocks in demo version is " + _maxCountActionBlocksToCreate + ".");

            return false;
        }

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


    
    private void OnUpdateActionBlocks()
    {
        Save();
    }

    private void OnStartChangeActionBlocksVariables()
    {

    }
    
    private void Save()
    {
        ActionBlock[] actionBlocks = GetActionBlocks().ToArray();
        string actionBlocksJSON = JsonConvert.SerializeObject(actionBlocks);
        _fileController.Save(actionBlocksFilePath, actionBlocksJSON);
    }
    
    private string GetTextWithoutSpecialSymbols(string textToCheck)
    {
        return Regex.Replace(textToCheck, @"[^0-9a-zA-Z]", " ");
        
        /*
        // 2 Way
        StringBuilder sb = new StringBuilder();
        
        foreach (char c in str) {
            if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) {
                sb.Append(c);
            }
        }
        
        return sb.ToString();
        */
    }
    
    
    private List<string> GetUpdatedTagsForCreationActionBlock(string originalTitle, ActionBlock actionBlock)
    {
        string titleLowerCase = actionBlock.Title.ToLower();
        string titleWithoutSpecialSymbols = GetTextWithoutSpecialSymbols(actionBlock.Title.ToLower());
        List<string> tags = actionBlock.Tags;
            
        // Add tags from title words.
        AddTitleToTag();

        if (string.Equals(titleLowerCase, titleWithoutSpecialSymbols) == false && originalTitle != actionBlock.Title)
        {
            AddTitleWithoutSpecialSymbolsToTag();
        }
        
        NormalizeTags();
        
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
                if (tag == titleWithoutSpecialSymbols)
                {
                    // Tag as the title without spec symbols exists.
                    return;
                }
            }
            
            tags.Add(titleWithoutSpecialSymbols);
        }

        void NormalizeTags()
        {
            List<string> newTags = new List<string>();
                
            foreach (string tag in tags)
            {
                // Delete empty spaces from sides of tags.
                string tagTrimmed = tag.Trim();
                
                if (string.IsNullOrEmpty(tagTrimmed) == false)
                {
                    newTags.Add(tagTrimmed);
                }
            }

            tags = newTags;
        }

        return tags;
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

        CreateFolderIfMissing(backupFolderPath);
        
        _fileController.Save(backupFolderPath + "/" + "Action-Blocks_" + 
                             DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json", 
            actionBlocksJSON);
    }
}
