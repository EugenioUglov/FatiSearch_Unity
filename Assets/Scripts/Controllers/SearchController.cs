using System;
using UnityEngine;
using Views;
using System.Collections.Generic;

namespace Controllers
{
    public class SearchController : MonoBehaviour
    {
        [SerializeField] private SearchView _view;

        private void Awake()
        {
            EventAggregator.AddListener<KeyClickedEvent>(this, OnKeyEnterClicked);
        }

        
        public void OnClickEnterButton()
        {
            OnEnterInputField();
        }

        public void ShowPage()
        {
            _view.ShowPage();
        }
        
        public void HidePage()
        {
            _view.HidePage();
        }
        

        
        private void OnEnterInputField()
        {
            string userRequest = _view.GetTextFromInputField().ToLower();
            SearchEnteredEvent searchEnteredEvent = new SearchEnteredEvent();
            searchEnteredEvent.Request = userRequest;
            EventAggregator.Invoke<SearchEnteredEvent>(searchEnteredEvent);
        }
        
        
        private void OnKeyEnterClicked(KeyClickedEvent keyClickedEvent)
        {
            if (keyClickedEvent.KeyCodeEntered == KeyCode.Return)
            {
                OnEnterInputField();    
            }
        }
    }
}