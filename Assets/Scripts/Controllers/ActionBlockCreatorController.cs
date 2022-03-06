using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public GameObject DeleteButton;
    
    
    [Header("Links")]
    [SerializeField] private GameObject _actionBlockSettingsPage;
    [SerializeField] private ActionBlockController _actionBlockController;
    [SerializeField] private ActionBlockModifierController _actionBlockModifierController;
    [SerializeField] private PageController _pageController;
    [SerializeField] private SearchController _searchController;
    
    
    public void ShowSettingsToCreateActionBlock()
    {
        _pageController.PageState = PageController.PageStateEnum.ActionBlockCreator;
        
        _searchController.HidePage();
        ShowSettingsForActionBlock();
    }
    
    public void ShowSettingsForActionBlock()
    {
        _actionBlockSettingsPage.SetActive(true);
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
            string[] tags = tagsTextFromInputField.Split(',');
         
            print("Count tags: " + tags.Length);

            // Delete empty spaces from sides of tags.
            for (int i_tag = 0; i_tag < tags.Length; i_tag++)
            {
                print("tag : " + tags[i_tag]);
                tagsList.Add(tags[i_tag].Trim());
            }
        }
        
        

        string imagePath = ImagePathInputField.GetComponent<TMP_InputField>().text;
        
        ActionBlockModel.ActionBlock actionBlock = new ActionBlockModel.ActionBlock(title, action, content, tagsList, imagePath);
        
        
        if (_pageController.PageState == PageController.PageStateEnum.ActionBlockCreator)
        {
            _actionBlockController.CreateActionBlock(actionBlock);
        }
        else if (_pageController.PageState == PageController.PageStateEnum.ActionBlockModifier)
        {
            _actionBlockController.UpdateActionBlock(_actionBlockModifierController.OriginalTitle, actionBlock);
            DeleteButton.SetActive(false);
        }

        OnEnd();
    }
    
    public void OnClickButtonCloseSettingsActionBlock()
    {
        HidePage();
        _searchController.ShowPage();
        
        if (_pageController.PageState == PageController.PageStateEnum.ActionBlockModifier)
        {
            print("PageState Modify close");
            SetDefaultFields();
            DeleteButton.SetActive(false);
        }
    }


    private void SetDefaultFields()
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
        print("tagsTextFromInputField: " + tagsTextFromInputField);
        
        if (string.IsNullOrEmpty(tagsTextFromInputField) == false)
        {
            string[] tags = tagsTextFromInputField.Split(',');
         
            print("Count tags: " + tags.Length);

            // Delete empty spaces from sides of tags.
            for (int i_tag = 0; i_tag < tags.Length; i_tag++)
            {
                print("tag : " + tags[i_tag]);
                tagsList.Add(tags[i_tag].Trim());
            }
        }
        
        

        string imagePath = ImagePathInputField.GetComponent<TMP_InputField>().text;
        
        ActionBlockModel.ActionBlock actionBlockNew = new ActionBlockModel.ActionBlock(title, action, content, tagsList, imagePath);
        
       
        _actionBlockController.ShowActionBlocks();
        HidePage();
        //_searchController.ShowPage();
        SetDefaultFields();
    }

    public void OnEnd()
    {
        _actionBlockController.ShowActionBlocks();
        HidePage();
        //_searchController.ShowPage();
        SetDefaultFields();
    }
    
}
