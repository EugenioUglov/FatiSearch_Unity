using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Unity.VisualScripting;


public class ActionBlockModel : MonoBehaviour
{
    public static class ActionEnum
    {
        public const string OpenPath = "OpenPath";
        public const string SelectPath = "SelectPath";
        public const string ShowInfo = "ShowInfo";
    }
    
    [Header("Links")] 
    [SerializeField] private FileController _fileController;
    
    private string actionBlocksFilePath = "Action-Blocks.json";

    //private List<ActionBlock> _actionBlocks = new List<ActionBlock>();
    //private Dictionary<string, ActionBlock> _actionBlockByTitle = new Dictionary<string, ActionBlock>();
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
        
        /*
        foreach (KeyValuePair<string, ActionBlock> item in _actionBlockByTitle)
        {
            actionBlocks.Add(item.Value);
        }
        */


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
            Debug.Log(e);
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
            ActionBlock[] actionBlocksByTag = GetActionBlocksByTag(tag).ToArray();

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
        
        Dictionary<ActionBlock, int> priorityByActionBlockSortedFromMin =
            new Dictionary<ActionBlock, int>();
            
        // Sort array from min value.
        foreach (var pair in priorityByActionBlock.OrderBy(pair => pair.Value))
        {
            priorityByActionBlockSortedFromMin[pair.Key] = pair.Value;
            actionBlocksToShow.Add(pair.Key);
        }

        
        return actionBlocksToShow.ToArray(); 
    }

    public ActionBlock[] GetActionBlocksFromFile()
    {
        string actionBlocksJSONFromFile = _fileController.GetContentFromFile(actionBlocksFilePath);
        ActionBlock[] actionBlocksFromFile = new ActionBlock[]{};

        if (string.IsNullOrEmpty(actionBlocksJSONFromFile) == false)
        {
            try
            {
                actionBlocksFromFile =
                    JsonConvert.DeserializeObject<ActionBlock[]>(actionBlocksJSONFromFile);

                if (actionBlocksFromFile.Length == 0)
                {

                }

                print(actionBlocksJSONFromFile);
            }
            catch (Exception exception)
            {
                print(exception);
                print("Error! File with Action-Blocks not found");
            }
        }

        return actionBlocksFromFile;
    }

    public void SetActionBlocks(ActionBlock[] actionBlocks)
    {
        foreach (var actionBlock in actionBlocks)
        {
            AddActionBlockToVariables(actionBlock, false);
        }
    
        print("Set Action-Blocks");
        
        //_actionBlocks = actionBlocks.ToList();
        //UpdateIndexActionBlocks();
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


    
    public void CreateActionBlock(ActionBlock actionBlock)
    {
        string titleLowerCase = actionBlock.Title.ToLower();


        // If title already exists.
        //if (_actionBlockByTitle.TryGetValue(titleLowerCase, out var val))
        if (_actionBlockByTitle.Contains(titleLowerCase))
        {
            throw new InvalidOperationException("Action-Block with title \"" + actionBlock.Title + "\" already exists");
        }
        
        if (string.IsNullOrEmpty(titleLowerCase))
        {
            throw new InvalidOperationException("Error! Title is not defined.");
        }


        // Add tags from title words.
        AddTitleToTag();        
        
        void AddTitleToTag()
        {
            foreach (var tag in actionBlock.Tags)
            {
                if (tag == actionBlock.Title)
                {
                    print("Tag already exists " + actionBlock.Title);
                    return;
                }
            }
        
            print("Tag has been added " + actionBlock.Title);
            actionBlock.Tags.Add(actionBlock.Title);
        }
        

        AddActionBlockToVariables(actionBlock);
        
        print("Action-BLock " + actionBlock.Title + " created");
        
        OnUpdateActionBlocks();
    }

    public void UpdateActionBlock(string originalTitle, ActionBlock actionBlock)
    {
        // Add tags from title words.
        AddTitleToTag();        
        
        void AddTitleToTag()
        {
            foreach (var tag in actionBlock.Tags)
            {
                if (tag == actionBlock.Title)
                {
                    print("Tag already exists " + actionBlock.Title);
                    return;
                }
            }
        
            print("Tag has been added " + actionBlock.Title);
            actionBlock.Tags.Add(actionBlock.Title);
        }
        
        string titleLowerCase = originalTitle.ToLower();
        _actionBlockByTitle.Remove(titleLowerCase);
        
        AddActionBlockToVariables(actionBlock);
        
        print("Action-BLock " + actionBlock.Title + " has been modified");
        
        OnUpdateActionBlocks();
    }

    private void AddActionBlockToVariables(ActionBlock actionBlock, bool isAddToStart = true)
    {
        //_actionBlocks.Insert(0, actionBlock);
        
        string titleLowerCase = actionBlock.Title.ToLower();

        /*
        // Add Action-Block by title.
        _actionBlockByTitle[titleLowerCase] = actionBlock;
        */
        if (isAddToStart)
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
    }


    public void DeleteActionBlock(ActionBlock actionBlock)
    {
        _actionBlockByTitle.Remove(actionBlock.Title.ToLower());
        UpdateIndexActionBlockByTags();
        OnUpdateActionBlocks();
    }
    
    /*
    private void UpdateIndexActionBlocks()
    {
        ActionBlock[] actionBlocks = GetActionBlocks().ToArray();
        
        foreach (var actionBlock in actionBlocks)
        {
            string titleLowerCase = actionBlock.Title.ToLower();
        
            // Add Action-Block by title.
            _actionBlockByTitle[titleLowerCase] = actionBlock;
            
            
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
                    if (_actionBlocksByTag.TryGetValue(tagWordLowCase, out HashSet<ActionBlock> value1) == false) {
                        _actionBlocksByTag[tagWordLowCase] = new HashSet<ActionBlock>(){};
                    }
                

                    _actionBlocksByTag[tagWordLowCase].Add(actionBlock);
                }
            }
        }
    }
    */

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

    private void OnUpdateActionBlocks()
    {
        Save();
    }
    
    private void Save()
    {
        ActionBlock[] actionBlocks = GetActionBlocks().ToArray();
        string actionBlocksJSON = JsonConvert.SerializeObject(actionBlocks);
        _fileController.Save(actionBlocksFilePath, actionBlocksJSON);

        print("Action-Blocks have been saved");
    }
}
