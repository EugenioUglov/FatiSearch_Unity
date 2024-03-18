using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class SearchView : MonoBehaviour
    {
        [SerializeField] private Button _enterButton;
        [SerializeField] private GameObject _inputFieldGO;
        [SerializeField] private GameObject _textFromInputFieldGO;
        [SerializeField] private GameObject _searchPage;

        private TMP_InputField _inputFieldTMP;
        private int lastCaretPosition = 0;
        
        
        private void Awake()
        {
            _inputFieldTMP = _inputFieldGO.GetComponent<TMP_InputField>();
        }

        public string GetTextFromInputField()
        {
            string text = _inputFieldTMP.text;
            
            return text;
        }
        
        public void SetTextToInputField(string newText)
        {
           _inputFieldTMP.text = newText;
        }

        public void ClearInputField()
        {
            _inputFieldTMP.text = "";
        }
        
        public void ShowPage()
        {
            _searchPage.SetActive(true);
            FocusInputField();
            lastCaretPosition = _inputFieldTMP.caretPosition;
        } 
    
        public void HidePage()
        {
            _inputFieldTMP.caretPosition = lastCaretPosition;
            _searchPage.SetActive(false);
        }

        public void FocusInputField()
        {
            // if (_inputFieldTMP.isFocused)
            // {
            //     return;
            // }
            
            _inputFieldTMP.Select();
            _inputFieldTMP.ActivateInputField();
        }
        
        public bool IsSelected()
        {
            return _inputFieldTMP.isFocused;
        }

        public void OnEnterInputField()
        {
            _textFromInputFieldGO.GetComponent<TextMeshProUGUI>().color = new Color32(50, 50, 50, 255);
            FocusInputField();
        }
        
        public void BindChangeInputFieldValue(Action<string> callbackValueChanged)
        {
            _inputFieldTMP.onValueChanged.AddListener(delegate
            {
                OnValueInputFieldChanged();
                callbackValueChanged(GetTextFromInputField());
            });
        }
        
        private void OnValueInputFieldChanged()
        {
            _textFromInputFieldGO.GetComponent<TextMeshProUGUI>().color = new Color32(50, 50, 50, 128);
        }
    }
}