using System.Collections.Generic;
using Controllers;
using TMPro;
using UnityEngine;
using SFB;

public class ActionBlockSettingsController : MonoBehaviour
{
    [Header("View")]
    [SerializeField] private GameObject _actionBlockSettingsPage;
    [SerializeField] private GameObject TitleInputField;
    [SerializeField] private GameObject ContentInputField;
    [SerializeField] private GameObject ActionDropdown;
    [SerializeField] private GameObject TagsInputField;
    [SerializeField] private GameObject ImagePathInputField;
    [SerializeField] private GameObject _deleteButton;
    
    [Header("Links")]
    [SerializeField] private ActionBlockController _actionBlockController;
    [SerializeField] private ActionBlockService _actionBlockService;
    [SerializeField] private PageService _pageService;
    [SerializeField] private SearchController _searchController;
    [SerializeField] private AlertController _alertController;

    public string OriginalTitle => originalActionBlock.Title;
    public string TextOfTitleInputField => TitleInputField.GetComponent<TMP_InputField>().text;
    public string TextOfContentInputField => ContentInputField.GetComponent<TMP_InputField>().text;
    public string TextOfTagsInputField => TagsInputField.GetComponent<TMP_InputField>().text;
    public string TextOfImagePathInputField => ImagePathInputField.GetComponent<TMP_InputField>().text;
    
    private ActionBlockModel.ActionBlock originalActionBlock;
    
    
    public void OnClickButtonClose()
    {
        if (_pageService.PageState == PageService.PageStateEnum.ActionBlockModifier)
        {
            SetDefaultFields();
            HideDeleteButton();
        }
        
        HidePage();
        _searchController.ShowPage();
    }
    
    public void OnClickButtonChooseContent()
    {
        var extensions = new [] {
            new ExtensionFilter("All Files", "*" )
        };
        
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, true);

        if (paths.Length > 1)
        {
            _alertController.Show("Choose one file");
            return;
        }

        if (paths.Length < 1)
        {
            return;
        }

        ContentInputField.GetComponent<TMP_InputField>().text = paths[0];
    }
    
    public void OnClickButtonChooseImage()
    {
        var extensions = new [] {
            new ExtensionFilter("Image File", "png", "jpg", "jpeg", "ico", "gif")
        };
        
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, true);
    
        if (paths.Length > 1)
        {
            _alertController.Show("Choose one file");
            return;
        }
        
        if (paths.Length < 1)
        {
            return;
        }
        
        ImagePathInputField.GetComponent<TMP_InputField>().text = paths[0];
    }
    
    public void OnClickButtonSave()
    {
        string title = TitleInputField.GetComponent<TMP_InputField>().text;
        // string action = ActionBlockModel.ActionEnum.OpenPath;
        // Dropdown.options[GetComponent<TMP_Dropdown>().value].text
        // print("Dropdown index: "+ ActionDropdown.GetComponent<TMP_Dropdown>().value);
        string action = ActionDropdown.GetComponent<TMP_Dropdown>().options[ActionDropdown.GetComponent<TMP_Dropdown>().value].text;
        string content = ContentInputField.GetComponent<TMP_InputField>().text;
        string tagsTextFromInputField = TagsInputField.GetComponent<TMP_InputField>().text;
        List<string> tagsList = new List<string>();

        if (string.IsNullOrEmpty(tagsTextFromInputField) == false)
        {
            string[] tags_to_add = tagsTextFromInputField.Split(',');

            for (int i_tag = 0; i_tag < tags_to_add.Length; i_tag++)
            {
                // Delete empty spaces from sides of tags.
                string tagToAdd = tags_to_add[i_tag].Trim();
                
                tagsList.Add(tagToAdd);
            }
        }

        string imagePath = ImagePathInputField.GetComponent<TMP_InputField>().text;
        ActionBlockModel.ActionBlock actionBlock = new ActionBlockModel.ActionBlock(title, action, content, tagsList, imagePath);

        if (_pageService.PageState == PageService.PageStateEnum.ActionBlockCreator)
        {
            bool isCreated = _actionBlockService.CreateActionBlock(actionBlock);
            if (isCreated == false) return;
        }
        else if (_pageService.PageState == PageService.PageStateEnum.ActionBlockModifier)
        {
            bool isUpdated = _actionBlockService.UpdateActionBlock(OriginalTitle, actionBlock);
            if (isUpdated == false) return;
                
            HideDeleteButton();
        }
        
        HidePage();
    }
    
    public void OnClickButtonDelete()
    {
        _actionBlockService.DeleteActionBlock(originalActionBlock, onDone: () => { 
            _deleteButton.SetActive(false);
            
            HidePage();
            //_searchController.ShowPage();
            SetDefaultFields();
        });
    }
    

    private void UpdateActionBlock()
    {
        string title = TitleInputField.GetComponent<TMP_InputField>().text;
        string action = ActionBlockModel.ActionEnum.OpenPath;
        //ActionBlockModel.ActionEnum action = _actionDropdown.GetComponent<Dropdown>().value.ToString();
        string content = ContentInputField.GetComponent<TMP_InputField>().text;
        string tagsTextFromInputField = TagsInputField.GetComponent<TMP_InputField>().text;
        List<string> tagsList = new List<string>();
        
        if (string.IsNullOrEmpty(tagsTextFromInputField) == false)
        {
            string[] tags = tagsTextFromInputField.Split(',');
         
            // Delete empty spaces from sides of tags.
            for (int i_tag = 0; i_tag < tags.Length; i_tag++)
            {
                tagsList.Add(tags[i_tag].Trim());
            }
        }
        
        string imagePath = ImagePathInputField.GetComponent<TMP_InputField>().text;
        ActionBlockModel.ActionBlock actionBlockNew = new ActionBlockModel.ActionBlock(title, action, content, tagsList, imagePath);

        HidePage();
        SetDefaultFields();
    }
    
    
    #region View
    
    public void ShowSettingsToUpdateActionBlock(ActionBlockModel.ActionBlock actionBlock)
    {
        originalActionBlock = actionBlock;

        SetFieldsForActionBlockToModify(actionBlock);
        _pageService.PageState = PageService.PageStateEnum.ActionBlockModifier;
        
        _deleteButton.SetActive(true);
        _searchController.HidePage();
        
        _actionBlockSettingsPage.SetActive(true);
        
        TitleInputField.GetComponent<TMP_InputField>().Select();
        TitleInputField.GetComponent<TMP_InputField>().ActivateInputField();
    }
    
    public void ShowSettingsToCreateActionBlock()
    {
        _pageService.PageState = PageService.PageStateEnum.ActionBlockCreator;
        
        _deleteButton.SetActive(false);
        
        string title = TitleInputField.GetComponent<TMP_InputField>().text;
        string textFromSearchInputField = _searchController.GetTextFromInputField();

        if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(textFromSearchInputField) == false)
        {
            TitleInputField.GetComponent<TMP_InputField>().text = textFromSearchInputField;
        }

        _actionBlockSettingsPage.SetActive(true);
        
        TitleInputField.GetComponent<TMP_InputField>().Select();
        TitleInputField.GetComponent<TMP_InputField>().ActivateInputField();
    }
    
    public void HideDeleteButton()
    {
        _deleteButton.SetActive(false);
    }
    
    public void HidePage()
    {
        _actionBlockSettingsPage.SetActive(false);
    }
    
    public void SetDefaultFields()
    {
        TitleInputField.GetComponent<TMP_InputField>().text = "";
        ContentInputField.GetComponent<TMP_InputField>().text = "";
        TagsInputField.GetComponent<TMP_InputField>().text = "";
        ImagePathInputField.GetComponent<TMP_InputField>().text = "";
    }
    
    private void SetFieldsForActionBlockToModify(ActionBlockModel.ActionBlock actionBlock)
    {
        TitleInputField.GetComponent<TMP_InputField>().text = actionBlock.Title;
        ActionDropdown.GetComponent<TMP_Dropdown>().value = ActionDropdown.GetComponent<TMP_Dropdown>().options.FindIndex(option => option.text == actionBlock.Action);
        ContentInputField.GetComponent<TMP_InputField>().text = actionBlock.Content;
        
        SetTags();
        
        void SetTags()
        {
            TagsInputField.GetComponent<TMP_InputField>().text = "";
            
            for (int i = 0; i < actionBlock.Tags.Count; i++)
            {
                string tag = actionBlock.Tags[i];
                TagsInputField.GetComponent<TMP_InputField>().text += tag;
            
                if (i < actionBlock.Tags.Count - 1)
                {
                    TagsInputField.GetComponent<TMP_InputField>().text += ", ";
                }
            }
        }

        ImagePathInputField.GetComponent<TMP_InputField>().text = actionBlock.ImagePath;
    }
    
    #endregion View
}