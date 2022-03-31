using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Controllers;
using TMPro;
using UnityEngine;


public class ActionBlockCreatorController : MonoBehaviour
{
    [Header("View")]
    public GameObject TitleInputField;
    public GameObject ContentInputField;
    public GameObject ActionDropdown;
    public GameObject TagsInputField;
    public GameObject ImagePathInputField;
    
    [Header("Links")]
    [SerializeField] private GameObject _actionBlockSettingsPage;
    [SerializeField] private ActionBlockController _actionBlockController;
    [SerializeField] private ActionBlockModifierController _actionBlockModifierController;
    [SerializeField] private PageService _pageService;
    [SerializeField] private SearchController _searchController;
    
    
    public void ShowSettingsToCreateActionBlock()
    {
        _pageService.PageState = PageService.PageStateEnum.ActionBlockCreator;
        
        _searchController.HidePage();
        ShowSettingsForActionBlock();
    }
    
    public void ShowSettingsForActionBlock()
    {
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
    
    public void HidePage()
    {
        _actionBlockSettingsPage.SetActive(false);
    }

    public void OnClickButtonSaveNewSettingsActionBlock()
    {
        string title = TitleInputField.GetComponent<TMP_InputField>().text;
        string action = ActionBlockModel.ActionEnum.OpenPath;
        //ActionBlockModel.ActionEnum action = _actionDropdown.GetComponent<Dropdown>().value.ToString();
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
            bool isCreated = _actionBlockController.CreateActionBlock(actionBlock);
            if (isCreated == false) return;
        }
        else if (_pageService.PageState == PageService.PageStateEnum.ActionBlockModifier)
        {
            _actionBlockController.UpdateActionBlock(_actionBlockModifierController.OriginalTitle, actionBlock);
            _actionBlockModifierController.HideDeleteButton();
        }
        
        
        _searchController.ClearInputField();
        HidePage();
        _searchController.ShowPage();
        _actionBlockController.ShowActionBlocks();
        SetDefaultFields();
    }
    
    public void OnClickButtonCloseSettingsActionBlock()
    {
        HidePage();
        _searchController.ShowPage();
        
        if (_pageService.PageState == PageService.PageStateEnum.ActionBlockModifier)
        {
            SetDefaultFields();
            _actionBlockModifierController.HideDeleteButton();
        }
    }

    public void SetDefaultFields()
    {
        TitleInputField.GetComponent<TMP_InputField>().text = "";
        ContentInputField.GetComponent<TMP_InputField>().text = "";
        TagsInputField.GetComponent<TMP_InputField>().text = "";
        ImagePathInputField.GetComponent<TMP_InputField>().text = "";
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
        _actionBlockController.ShowActionBlocks();
        HidePage();
        SetDefaultFields();
    }
}
