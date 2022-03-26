using System.Collections;
using System.Collections.Generic;
using Controllers;
using SFB;
using TMPro;
using UnityEngine;

public class ActionBlockModifierController : MonoBehaviour
{
    [Header("View")]
    [SerializeField] private GameObject _deleteButton;

    
    [Header("Links")]
    [SerializeField] private ActionBlockCreatorController _actionBlockCreatorController;
    [SerializeField] private ActionBlockController _actionBlockController;
    [SerializeField] private PageService _pageService;
    [SerializeField] private SearchController _searchController;
    
    public string OriginalTitle => originalActionBlock.Title;
    
    private ActionBlockModel.ActionBlock originalActionBlock;


    #region View
    
    public void HideDeleteButton()
    {
        _deleteButton.SetActive(false);
    }
    
    #endregion View
    
    
    public void ShowSettingsToUpdateActionBlock(ActionBlockModel.ActionBlock actionBlock)
    {
        originalActionBlock = actionBlock;
        SetFieldsForActionBlockToModify(actionBlock);
        _actionBlockCreatorController.ShowSettingsToCreateActionBlock();
        _pageService.PageState = PageService.PageStateEnum.ActionBlockModifier;
        
        _deleteButton.SetActive(true);
        _searchController.HidePage();
    }

    public void OnClickButtonDelete()
    {
        _actionBlockController.DeleteActionBlock(originalActionBlock);
        _deleteButton.SetActive(false);
        _actionBlockController.ShowActionBlocks();
        _actionBlockCreatorController.HidePage();
        //_searchController.ShowPage();
        _actionBlockCreatorController.SetDefaultFields();
    }
    
    public void OnClickButtonChooseImage()
    {
        var extensions = new [] {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg" )
        };
        
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, true);
        
        _actionBlockCreatorController.ImagePathInputField.GetComponent<TMP_InputField>().text = paths[0];
    }
    
    public void OnClickButtonChooseContent()
    {
        var extensions = new [] {
            new ExtensionFilter("All Files", "*" )
        };
        
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, true);
        
        _actionBlockCreatorController.ContentInputField.GetComponent<TMP_InputField>().text = paths[0];
    }
    
    private void SetFieldsForActionBlockToModify(ActionBlockModel.ActionBlock actionBlock)
    {
        _actionBlockCreatorController.TitleInputField.GetComponent<TMP_InputField>().text = actionBlock.Title;
        _actionBlockCreatorController.ContentInputField.GetComponent<TMP_InputField>().text = actionBlock.Content;

        for (int i = 0; i < actionBlock.Tags.Count; i++)
        {
            string tag = actionBlock.Tags[i];
            _actionBlockCreatorController.TagsInputField.GetComponent<TMP_InputField>().text += tag;
            
            if (i < actionBlock.Tags.Count - 1)
            {
                _actionBlockCreatorController.TagsInputField.GetComponent<TMP_InputField>().text += ", ";
            }
        }

        _actionBlockCreatorController.ImagePathInputField.GetComponent<TMP_InputField>().text = actionBlock.ImagePath;
    }
    
    
}
