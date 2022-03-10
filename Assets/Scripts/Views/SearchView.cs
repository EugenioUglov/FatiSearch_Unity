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
        } 
    
        public void HidePage()
        {
            _searchPage.SetActive(false);
        } 
    }
}