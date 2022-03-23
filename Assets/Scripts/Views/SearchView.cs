using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class SearchView : MonoBehaviour
    {
        [SerializeField] private Button _enterButton;
        [SerializeField] private GameObject _inputField;
        [SerializeField] private GameObject _searchPage;
        
        public string GetTextFromInputField()
        {
            string text = _inputField.GetComponent<TMP_InputField>().text;
            
            return text;
        }

        public void ClearInputField()
        {
            _inputField.GetComponent<TMP_InputField>().text = "";
        }
        
        public void ShowPage()
        {
            _searchPage.SetActive(true);
            _inputField.GetComponent<TMP_InputField>().GetComponent<TMP_InputField>().Select();
            _inputField.GetComponent<TMP_InputField>().GetComponent<TMP_InputField>().ActivateInputField();
            
            FocusInputField();
        } 
    
        public void HidePage()
        {
            _searchPage.SetActive(false);
        }

        public void FocusInputField()
        {
            _inputField.GetComponent<TMP_InputField>().Select();
            _inputField.GetComponent<TMP_InputField>().ActivateInputField();
        }
    }
}