using UnityEngine;
using Views;

namespace Controllers
{
    public class SearchController : MonoBehaviour
    {
        [SerializeField] private SearchView _view;
        [SerializeField] private PageService _pageService;

        private void Awake()
        {
            EventAggregator.AddListener<KeyClickedEvent>(this, OnKeyClicked);
        }

        public void Init()
        {
            _view.BindChangeInputFieldValue(OnValueInputFieldChanged);
            _view.FocusInputField();
            ShowPage();
        }
        
        public string GetTextFromInputField()
        {
            return _view.GetTextFromInputField();
        }
        
        public void OnClickEnterButton()
        {
            OnEnterInputField();
        }


        public void ShowPage()
        {
            _pageService.PageState = PageService.PageStateEnum.SearchPage;
            
            _view.ShowPage();
        }
        
        public void HidePage()
        {
            _view.HidePage();
        }

        public void ClearInputField()
        {
            _view.ClearInputField();
        }

        
        private void OnEnterInputField()
        {
            string userRequest = _view.GetTextFromInputField().ToLower();
            SearchEnteredEvent searchEnteredEvent = new SearchEnteredEvent();

            searchEnteredEvent.Request = userRequest;
            EventAggregator.Invoke<SearchEnteredEvent>(searchEnteredEvent);

            _view.OnEnterInputField();
        }

        private void OnKeyClicked(KeyClickedEvent keyClickedEvent)
        {
            if (_pageService.PageState == PageService.PageStateEnum.SearchPage)
            {
                
                if (keyClickedEvent.KeyCodeEntered == KeyCode.Return)
                {
                    OnEnterInputField();
                }
            }
        }

        private void OnValueInputFieldChanged(string text)
        {
            ValueChangedInInputFieldSearchEvent valueChangedInInputFieldSearchEvent = new ValueChangedInInputFieldSearchEvent();
            valueChangedInInputFieldSearchEvent.Request = text;
            EventAggregator.Invoke<ValueChangedInInputFieldSearchEvent>(valueChangedInInputFieldSearchEvent);
        }
    }
}